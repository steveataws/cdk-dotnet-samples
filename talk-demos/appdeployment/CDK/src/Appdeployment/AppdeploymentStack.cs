using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.AutoScaling;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.CodeDeploy;

namespace Appdeployment
{
    /// <summary>
    /// Application resources stack constructs the VPC, App load balancer and Auto scaling
    /// group that will contain the deployed application.
    /// </summary>
    public class AppdeploymentStack : Stack
    {
        internal AppdeploymentStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            #region Application hosting resources

            var vpc = new Vpc(this, "appVpc", new VpcProps
            {
                MaxAzs = 3
            });

            var image = new LookupMachineImage(new LookupMachineImageProps
            {
                // maps to "Amazon Linux 2 with .NET Core 3.0 and Mono 5.18"
                Name = "amzn2-ami-hvm-2.0.*-x86_64-gp2-mono-*",
                Owners = new [] { "amazon" }
            });

            var userData = UserData.ForLinux();
            userData.AddCommands(new string[]
            {
                "sudo yum install -y httpd",
                "sudo systemctl start httpd",
                "sudo systemctl enable httpd"
            });

            var scalingGroup = new AutoScalingGroup(this, "appASG", new AutoScalingGroupProps
            {
                Vpc = vpc,
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MEDIUM),
                MachineImage = image,
                MinCapacity = 1,
                MaxCapacity = 4,
                AllowAllOutbound = true,
                UserData = userData
            });

            var alb = new ApplicationLoadBalancer(this, "appLB", new ApplicationLoadBalancerProps
            {
                Vpc = vpc,
                InternetFacing = true
            });

            var albListener = alb.AddListener("Port80Listener", new BaseApplicationListenerProps
            {
                Port = 80
            });

            albListener.AddTargets("Port80ListenerTargets", new AddApplicationTargetsProps
            {
                Port = 80,
                Targets = new [] { scalingGroup }
            });

            albListener.Connections.AllowDefaultPortFromAnyIpv4("Open access to port 80");

            scalingGroup.ScaleOnRequestCount("ScaleOnModestLoad", new RequestCountScalingProps
            {
                TargetRequestsPerSecond = 1
            });

            #endregion

            #region CI/CD resources

            var _sourceOutput = new Artifact_("Source");
            var _buildOutput = new Artifact_("Build");

            var build = new PipelineProject(this, "CodeBuild", new PipelineProjectProps
            {
                // relative path to sample app's file (single html page for now)
                BuildSpec = BuildSpec.FromSourceFilename("talk-demos/appdeployment/SimplePage/buildspec.yml"),
                Environment = new BuildEnvironment
                {
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_2
                },
            });

            var appDeployment = new ServerApplication(this, "appDeployment");
            // we will use CodeDeploy's default one-at-a-time deployment mode as we are
            // not specifying a deployment config
            var deploymentGroup = new ServerDeploymentGroup(this, "appDeploymentGroup", new ServerDeploymentGroupProps
            {
                Application = appDeployment,
                InstallAgent = true,
                AutoRollback = new AutoRollbackConfig
                {
                    FailedDeployment = true
                },
                AutoScalingGroups = new [] { scalingGroup }
            });

            // SecretValue.SsmSecure is not currently supported for setting OauthToken,
            // and haven't gotten the SecretsManager approach to work either so
            // resorting to keeping my token in an environment var for now!
            var oauthToken = SecretValue.PlainText(System.Environment.GetEnvironmentVariable("GitHubPersonalToken"));

            var pipeline = new Pipeline(this, "sampleappPipeline", new PipelineProps
            {
                Stages = new StageProps[]
                {
                    new StageProps
                    {
                        StageName = "Source",
                        Actions = new IAction[]
                        {
                            new GitHubSourceAction(new GitHubSourceActionProps
                            {
                                ActionName = "GitHubSource",
                                Branch = "master",
                                Repo = this.Node.TryGetContext("repo-name").ToString(),
                                Owner = this.Node.TryGetContext("repo-owner").ToString(),
                                OauthToken = oauthToken,
                                Output = _sourceOutput
                            })
                        }
                    },

                    new StageProps
                    {
                        StageName = "Build",
                        Actions = new IAction[]
                        {
                            new CodeBuildAction(new CodeBuildActionProps
                            {
                                ActionName = "Build-app",
                                Project = build,
                                Input = _sourceOutput,
                                Outputs = new Artifact_[] { _buildOutput },
                                RunOrder = 1
                            })
                        }
                    },

                    new StageProps
                    {
                        StageName = "Deploy",
                        Actions = new IAction[]
                        {
                            new CodeDeployServerDeployAction(new CodeDeployServerDeployActionProps
                            {
                                ActionName = "Deploy-app",
                                Input = _buildOutput,
                                RunOrder = 2,
                                DeploymentGroup = deploymentGroup
                            })
                        }
                    }
                }
            });

            #endregion
        }
    }
}
