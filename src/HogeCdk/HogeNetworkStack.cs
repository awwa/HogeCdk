using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace HogeCdk
{
    /// <summary>
    /// ネットワーク関連のリソースを生成するスタック
    /// ステートフルなもののみ集めること
    /// イメージ的には他のリソースの基盤となるものを集め、他のスタックの依存関係の元になること
    /// </summary>
    public class HogeNetworkStack : Stack
    {
        public Vpc Vpc { get; set; }

        public HogeNetworkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // VPC
            var vpcProps = new VpcProps
            {
                //Cidr = "10.1.0.0/16",
                SubnetConfiguration = new SubnetConfiguration[]
                {
                    new SubnetConfiguration
                    {
                        Name = "public",
                        CidrMask = 24,
                        SubnetType = SubnetType.PUBLIC,

                    },
                    new SubnetConfiguration
                    {
                        Name = "private",
                        CidrMask = 24,
                        SubnetType = SubnetType.PRIVATE_WITH_EGRESS,
                    }
                },
                MaxAzs = 2
            };
            var vpc = new Vpc(this, "vpc", vpcProps);

            // Expose properties
            Vpc = vpc;
        }
    }
}
