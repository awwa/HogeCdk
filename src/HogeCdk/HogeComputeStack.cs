using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.StepFunctions;
using Constructs;
using System.IO;
using System.Linq;

namespace HogeCdk
{

    public class HogeComputeStackProps : StackProps
    {
        public Vpc Vpc { get; set; }
    }

    /// <summary>
    /// コンピュート関連のリソースを生成するスタック
    /// ステートレスなものや、サブのワークロード単位でスタックの分離を検討すること
    /// </summary>
    public class HogeComputeStack : Stack
    {
        public HogeComputeStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var p = (HogeComputeStackProps)props;
            // ALB
            var sgAlb = new CfnSecurityGroup(this, "sg-alb", new CfnSecurityGroupProps
            {
                GroupDescription = "for ALB",
                VpcId = p.Vpc.VpcId,
                SecurityGroupIngress = new CfnSecurityGroup.IngressProperty[]
                {
                    new CfnSecurityGroup.IngressProperty
                    {
                        IpProtocol = "tcp",
                        FromPort = 80,
                        ToPort = 80,
                        CidrIp = "0.0.0.0/0"
                    }
                }
            });
            var alb = new CfnLoadBalancer(this, "alb", new CfnLoadBalancerProps
            {
                Scheme = "internet-facing",
                SecurityGroups = new string[] { sgAlb.Ref },
                Subnets = p.Vpc.PublicSubnets.Select(x => x.SubnetId).ToArray(),
            });

            // EC2
            var sgEc2 = new CfnSecurityGroup(this, "sg-ec2", new CfnSecurityGroupProps
            {
                VpcId = p.Vpc.VpcId,
                GroupDescription = "for EC2",
                SecurityGroupIngress = new CfnSecurityGroup.IngressProperty[]
                {
                    new CfnSecurityGroup.IngressProperty
                    {
                        IpProtocol = "tcp",
                        FromPort = 5000,
                        ToPort = 5000,
                        SourceSecurityGroupId = sgAlb.AttrGroupId,
                    },
                },
            });
            var ec2 = new CfnInstance(this, "ec2", new CfnInstanceProps
            {
                ImageId = "ami-02c3627b04781eada",
                InstanceType = "t2.micro",
                KeyName = "awwa500-key-pair-ap-northeast-1",
                SecurityGroupIds = new string[] { sgEc2.AttrGroupId },
                SubnetId = p.Vpc.PublicSubnets[0].SubnetId,
            });

            // Lambda
            var hello = new Function(this, "hello-handler", new FunctionProps
            {
                Runtime = Runtime.NODEJS_16_X,      // execution environment
                Code = Code.FromAsset("lambda"),    // Code loaded from the "lambda" directory
                Handler = "hello.handler"           // file is "hello", function is "handler"
            });

            // Step Functions
            using (var reader = new StreamReader("step-functions/sm1.asl.json"))
            {
                var role = new Role(this, "state-machine-role", new RoleProps
                {
                    AssumedBy = new ServicePrincipal("states.amazonaws.com"),
                });
                var json = reader.ReadToEnd();
                var stateMachine = new CfnStateMachine(this, "state-machine", new CfnStateMachineProps
                {
                    RoleArn = role.RoleArn,
                    DefinitionString = json,
                });
            }
        }
    }
}
