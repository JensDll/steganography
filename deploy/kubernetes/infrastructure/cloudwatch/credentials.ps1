[CmdletBinding(DefaultParameterSetName = 'CDK')]
param (
  [Parameter(Position = 0)]
  [string]$UserName = 'CloudwatchAccessImageDataHiding',

  [Parameter(ParameterSetName = 'CDK')]
  [switch]$Destroy,

  [Parameter(ParameterSetName = 'Credentials')]
  [switch]$Recreate
)

try {
  Push-Location $PSScriptRoot

  $Env:AWS_USER_NAME = $UserName

  # https://github.com/PowerShell/PowerShell/issues/10967
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

New-AWSCredentials -UserName $UserName -Recreate:$Recreate -Verbose:$VerbosePreference
