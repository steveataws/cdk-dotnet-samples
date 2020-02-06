using Amazon.CDK;

namespace Containers
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App(null);

            new ContainersStack(app, "ContainersStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
                }
            });

            app.Synth();
        }
    }
}
