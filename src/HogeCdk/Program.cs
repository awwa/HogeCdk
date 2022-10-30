using Amazon.CDK;
using static HogeCdk.HogeStorageStack;

namespace HogeCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            //new HogeCdkStack(app, "HogeCdkStack");
            var network = new HogeNetworkStack(app, "HogeNetworkStack");

            new HogeComputeStack(app, "HogeComputeStack", new HogeComputeStackProps
            {
                Vpc = network.Vpc,
            });

            new HogeStorageStack(app, "HogeStorageStack", new HogeStorageStackProps
            {
                Vpc = network.Vpc,
            });

            app.Synth();
        }
    }
}
