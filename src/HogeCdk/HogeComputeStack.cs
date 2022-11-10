using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.ElasticLoadBalancingV2.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.StepFunctions;
using Constructs;
using System;
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
            var alb = new ApplicationLoadBalancer(this, "alb", new ApplicationLoadBalancerProps
            {
                Vpc = p.Vpc,
                InternetFacing = true,
                Http2Enabled = true,
            });

            // SecurityGroup for EC2
            var sgEc2 = new SecurityGroup(this, "sg-ec2", new SecurityGroupProps
            {
                Vpc = p.Vpc,
                // SecurityGroupName = "ec2i",
                // Description = "for EC2 Instance",
            });
            sgEc2.Connections.AllowFrom(alb, new Port(new PortProps
            {
                StringRepresentation = "alb_to_ec2",
                FromPort = 5000,
                ToPort = 5000,
                Protocol = Amazon.CDK.AWS.EC2.Protocol.TCP
            }));

            // IAM Role for EC2
            var roleEc2 = new Role(this, "role-ec2", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
            });
            roleEc2.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMManagedInstanceCore"));
            var instanceProfileEc2 = new CfnInstanceProfile(this, "instance-profile-ec2", new CfnInstanceProfileProps
            {
                Roles = new string[] { roleEc2.RoleName },
            });

            // UserData for EC2
            var userData = UserData.ForLinux(new LinuxUserDataOptions
            {
                Shebang = "#!/bin/bash",
            });
            userData.AddCommands(File.ReadAllText("ec2/userdata.sh", System.Text.Encoding.UTF8));

            var ec2 = new Instance_(this, "ec2", new Amazon.CDK.AWS.EC2.InstanceProps
            {
                Vpc = p.Vpc,
                MachineImage = new Amazon.CDK.AWS.EC2.AmazonLinuxImage(new AmazonLinuxImageProps
                {
                    Generation = Amazon.CDK.AWS.EC2.AmazonLinuxGeneration.AMAZON_LINUX_2,
                }),
                InstanceType = InstanceType.Of(InstanceClass.T2, InstanceSize.MICRO),
                SecurityGroup = sgEc2,
                VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PUBLIC },
                UserData = userData,
                Role = roleEc2
            });

            // TargetGroup for ALB
            var targetGroup = new ApplicationTargetGroup(this, "alb-tg", new ApplicationTargetGroupProps
            {
                Vpc = p.Vpc,
                Port = 5000,
                Protocol = ApplicationProtocol.HTTP,
                //HealthCheck = new HealthCheck
                //{
                //    Path = "/",
                //    Port = "80",
                //    Protocol = Protocol.HTTP,
                //    HealthyThresholdCount = 2,
                //    UnhealthyThresholdCount = 2,
                //    Interval = Duration.Seconds(30),
                //    Timeout = Duration.Seconds(5),
                //},
                TargetType = TargetType.INSTANCE,
            });
            targetGroup.AddTarget(new InstanceIdTarget(ec2.InstanceId, 5000));

            // Add listener for ALB with TargetGroup
            alb.AddListener("alb-listener", new ApplicationListenerProps
            {
                Port = 80,
                DefaultTargetGroups = new IApplicationTargetGroup[]
                {
                    targetGroup
                },
            });

            // RDS
            var rds = new ServerlessCluster(this, "aurora-serverless", new ServerlessClusterProps
            {
                Engine = DatabaseClusterEngine.AURORA_POSTGRESQL,
                Vpc = p.Vpc,
                Credentials = Credentials.FromGeneratedSecret("postgres"),
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
            rds.Connections.AllowDefaultPortFrom(sgEc2, "allow ec2 to rds");

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
