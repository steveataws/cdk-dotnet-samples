using Amazon.CDK;

namespace Transcription
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            new TranscriptionStack(app, "TranscriptionStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    // use explicit context setting to determine target region,
                    // overriding any environment or credential profile default
                    Region = app.Node.TryGetContext("Region").ToString()
                }
            });

            app.Synth();
        }
    }
}
