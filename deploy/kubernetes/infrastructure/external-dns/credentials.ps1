[CmdletBinding(DefaultParameterSetName = 'CDK')]
param (
  [Parameter(Position = 0)]
  [string]$UserName = 'Route53AccessImageDataHiding',

  [Parameter(Position = 1)]
  [string]$DomainName = 'imagehiding.com',

  [Parameter(ParameterSetName = 'CDK')]
  [switch]$Destroy,

  [Parameter(ParameterSetName = 'Credentials')]
  [switch]$Recreate
)

Import-Module $PSScriptRoot/../../aws-credentials.psm1 -Force

try {
  Push-Location $PSScriptRoot

  $Env:AWS_USER_NAME = $UserName
  $Env:AWS_DOMAIN_NAME = $DomainName

  cdk synth --quiet || $(exit 1)

  if ($Destroy) {
    cdk destroy
    Remove-AwsCredentials -UserName $UserName
    return
  } else {
    cdk deploy --require-approval never
  }
} finally {
  Pop-Location
}

New-AwsCredentials -UserName $UserName -Recreate:$Recreate -Verbose:$VerbosePreference
