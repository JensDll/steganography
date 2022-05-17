[CmdletBinding()]
param (
  [Alias('r')]
  [switch]$RecreateCredentials,

  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmInstallArgs
)

Import-Module $PSScriptRoot/../../aws-credentials.psm1 -Force

try {
  Push-Location $PSScriptRoot
  
  $json = cdk synth --json | ConvertFrom-Json -AsHashtable
  
  $user = $json.Resources.Values | Where-Object { $_.Type -eq 'AWS::IAM::User' }
  $userName = $user.Properties.UserName

  cdk deploy
}
finally {
  Pop-Location
}

$credentials = Get-AwsCredentials -UserName $userName -Recreate:$RecreateCredentials -Verbose:$VerbosePreference

helm upgrade external-dns bitnami/external-dns --install --namespace=external-dns `
  --create-namespace `
  --set "aws.credentials.accessKey=$($credentials.AccessKey),aws.credentials.secretKey=$($credentials.SecretKey)" `
  --values="$PSScriptRoot/values.yaml" $HelmInstallArgs
