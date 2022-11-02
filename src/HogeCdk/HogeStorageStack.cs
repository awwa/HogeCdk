using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace HogeCdk
{
    /// <summary>
    /// ストレージ関連のリソースを生成するスタック
    /// ステートフルなもののみ集めること
    /// </summary>
    public class HogeStorageStack : Stack
    {
        public class HogeStorageStackProps : StackProps
        {
            public Vpc Vpc { get; set; }
        }

        public HogeStorageStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var p = (HogeStorageStackProps)props;
            // S3
            new Bucket(this, "bucket", new BucketProps
            {
                //BucketName = "hoge-bucket",
                RemovalPolicy = RemovalPolicy.DESTROY,  // DESTROYは開発中のみ使用すること
                EventBridgeEnabled = true,
            });

            // RDS
            // new DatabaseInstance(this, "rds", new DatabaseInstanceProps
            // {
            //     Engine = DatabaseInstanceEngine.POSTGRES,
            //     // InstanceType = InstanceType.Of(InstanceClass.BURSTABLE2, InstanceSize.SMALL),
            //     AllocatedStorage = 8,
            //     StorageType = StorageType.STANDARD,
            //     Vpc = p.Vpc,
            //     VpcSubnets = new SubnetSelection
            //     {
            //         SubnetType = SubnetType.PRIVATE_ISOLATED,
            //     },
            //     MultiAz = false,
            //     // MasterUsername = "hoge",
            //     // MasterUserPassword = SecretValue.PlainText("hoge"),
            //     RemovalPolicy = RemovalPolicy.DESTROY,  // DESTROYは開発中のみ使用すること
            // });

            // Aurora Serverless
            new ServerlessCluster(this, "aurora-serverless", new ServerlessClusterProps
            {
                Engine = DatabaseClusterEngine.AURORA_POSTGRESQL,
                Vpc = p.Vpc,
                Credentials = Credentials.FromGeneratedSecret("hoge"),
                ClusterIdentifier = "db-endpoint-test",
                DefaultDatabaseName = "hoge",
                ParameterGroup = ParameterGroup.FromParameterGroupName(this, "aurora-serverless-param-group", "default.aurora-postgresql10"),
                VpcSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PRIVATE_ISOLATED,
                },
                RemovalPolicy = RemovalPolicy.DESTROY,  // DESTROYは開発中のみ使用すること
                Scaling = new ServerlessScalingOptions
                {
                    AutoPause = Duration.Minutes(5),
                    MinCapacity = AuroraCapacityUnit.ACU_2,
                    MaxCapacity = AuroraCapacityUnit.ACU_2,
                },
                EnableDataApi = false,
            });


            // DynamoDB
            new Table(this, "dynamo-db", new TableProps
            {
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING,
                },
                RemovalPolicy = RemovalPolicy.DESTROY,  // DESTROYは開発中のみ使用すること
            });
        }
    }
}
