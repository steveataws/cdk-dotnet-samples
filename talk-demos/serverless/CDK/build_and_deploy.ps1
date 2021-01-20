[CmdletBinding()]
param(
    # sets the credential profile to be used in the cdk deployment
    [Parameter(Mandatory)]
    [string]$ProfileName,

    # deployment region (not all regions support Amazon Transcribe yet)
    [Parameter(Mandatory)]
    [string]$Region,

    # email to subscribe to completion notifications. If specified, check the email after
    # deployment to confirm the subscription
    [Parameter(Mandatory)]
    [string]$SubscriberEmail,

    # optional language code for Amazon Transcribe. If not specified, the stack will default to
    # 'en-us'
    [string]$LanguageCode
)

# Build the two Lambda functions and place the zip bundles into the assets folder for the
# CDK deployment process to pick up. Note that these commands assume you have the
# Amazon.Lambda.Tools global tools package installed (dotnet tool install -g Amazon.Lambda.Tools).

Write-Host "...Building Lambda function packages"

dotnet lambda package --project-location ../Functions/StartTranscriptionJob --output-package ./assets/StartTranscriptionJob.zip
dotnet lambda package --project-location ../Functions/NotifyTranscriptionJobComplete --output-package ./assets/NotifyTranscriptionJobComplete.zip

# Deploy the stack using the CDK CLI; the app will determine target region
# from the supplied context, overriding any defaults associated with my
# default credential profile. To delete this stack, use 'cdk destroy -c Region=value'
$command = "cdk deploy -c Region=$Region -c SubscriberEmail=$SubscriberEmail --profile $ProfileName"
if ($LanguageCode) {
    $command += " -c LanguageCode=$LanguageCode"
}

Write-Host "...Deploying stack with command: $command"
Invoke-Expression $command
