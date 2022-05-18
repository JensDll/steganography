[CmdletBinding()]
param (
  [Parameter(Position = 0, Mandatory)]
  [ValidateSet('install', 'delete')]
  [string]$Action,

  [Alias('u')]
  [string]$UserName = 'Route53AccessImageDataHiding',

  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmArgs
)

Import-Module $PSScriptRoot/../../aws-credentials.psm1 -Force

$credentials = Read-AwsCredentials -UserName $userName -Verbose:$VerbosePreference

switch ($Action) {
  install {
    helm upgrade external-dns bitnami/external-dns --install --namespace=external-dns `
      --create-namespace `
      --set "aws.credentials.accessKey=$($credentials.AccessKey),aws.credentials.secretKey=$($credentials.SecretKey)" `
      --values="$PSScriptRoot/values.yaml" $HelmArgs
  }
  delete {
    helm delete cert-manager --namespace=cert-manager $HelmArgs

    if (-not ($HelmArgs -contains '--dry-run')) {
      kubectl delete namespace external-dns
    }
  }
}
