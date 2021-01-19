using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.IAM;

namespace HelloWorld
{
    public class HelloWorldStack : Stack
    {
        public HelloWorldStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            // Create bucket to host a static website
            var bucket = new Bucket(this, "WebsiteBucket", new BucketProps
            {
                BucketName = "cdk-helloworld-website-demo",
                PublicReadAccess = true,
                WebsiteIndexDocument = "index.html"
            });

            // deploy the site
            new BucketDeployment(this, "WebsiteBucketDeployment", new BucketDeploymentProps
            {
                DestinationBucket = bucket,
                Sources = new [] { Source.Asset("./site-contents") }
            });

            // emit the url of the website for convenience
            new CfnOutput(this, "BucketWebsiteUrl", new CfnOutputProps
            {
                Value = bucket.BucketWebsiteUrl
            });
        }
    }
}
