[CmdletBinding()]
param (
  [Alias('r')]
  [switch]$RecreateCredentials,

  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmInstallArgs
)

Import-Module $PSScriptRoot/../aws-credentials.psm1 -Force

try {
  Push-Location $PSScriptRoot
  
  $json = cdk synth --json | ConvertFrom-Json -AsHashtable
  
  $user = $json.Resources.Values | Where-Object { $_.Type -eq 'AWS::IAM::User' }
  $userName = $user.Properties.UserName

  cdk deploy
} finally {
  Pop-Location
}

$credentials = Get-AwsCredentials -UserName $userName -Recreate:$RecreateCredentials -Verbose:$VerbosePreference

$Env:AWS_ACCESS_KEY = $credentials.SecretKey
$Env:AWS_SECRET_KEY = $credentials.AccessKey

kubectl kustomize $PSScriptRoot
