using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.S3.Notifications;

namespace CustomResources
{
    public class CustomResourcesStack : Stack
    {
        public CustomResourcesStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            var bucket = new Bucket(this, "BucketForEventNotifications");

            var queue = new Queue(this, "ObjectCreatedEventQueue");

            bucket.AddEventNotification(EventType.OBJECT_CREATED, new SqsDestination(queue));

            // output the name of the bucket and queue resources that were created with automatic
            // names, for reference
            new CfnOutput(this, "BucketName", new CfnOutputProps
            {
                Value = bucket.BucketName
            });
            new CfnOutput(this, "ObjectCreatedEventQueueName", new CfnOutputProps
            {
                Value = queue.QueueName
            });
        }
    }
}
