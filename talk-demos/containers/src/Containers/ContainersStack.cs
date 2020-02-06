using System.IO;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;

namespace Containers
{
    public class ContainersStack : Stack
    {
        public ContainersStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            var vpc = Vpc.FromLookup(this, id = "DefaultVpc", new VpcLookupOptions
            {
                IsDefault = true
            });

            if (vpc == null)
            {
                throw new System.NullReferenceException($"Unable to determine default VPC in region {this.Region}");
            }

            var cluster = new Cluster(this, "Cluster", new ClusterProps
            {
                Vpc = vpc
            });

            var taskDef = new FargateTaskDefinition(this, "FargateTaskDefinition");

            var currentDir = Directory.GetCurrentDirectory();
            var path = Path.GetFullPath(Path.Combine(currentDir, @"dotnetapp/"));

            var containerOptions = new ContainerDefinitionOptions
            {
                Image = ContainerImage.FromAsset("dotnetapp")
            };

            var portMapping = new PortMapping()
            {
                ContainerPort = 80,
                HostPort = 80
            };

            taskDef.AddContainer("Container", containerOptions).AddPortMappings(portMapping);

            var serviceProps = new ApplicationLoadBalancedFargateServiceProps()
            {
                MemoryLimitMiB = 512,
                Cpu = 256,
                TaskDefinition = taskDef
            };

            ApplicationLoadBalancedFargateService service
                = new ApplicationLoadBalancedFargateService(this, "DotnetFargateApp", serviceProps);
        }
    }
}
