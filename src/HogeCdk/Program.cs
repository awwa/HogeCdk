using Amazon.CDK;

namespace HogeCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new HogeCdkStack(app, "HogeCdkStack");

            app.Synth();
        }
    }
}
