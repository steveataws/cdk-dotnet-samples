using Amazon.CDK;
using System;

namespace ResourceEvents
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App(null);
            new ResourceEventsStack(app, "ResourceEventsStack", new StackProps());
            app.Synth();
        }
    }
}
