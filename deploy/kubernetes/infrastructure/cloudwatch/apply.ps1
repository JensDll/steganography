[CmdletBinding()]
param (
  [Parameter(Position = 0, Mandatory)]
  [ValidateSet('install', 'delete', 'debug')]
  [string]$Action,

  [Alias('u')]
  [string]$UserName = 'CloudwatchAccessImageDataHiding'
)

Import-Module $PSScriptRoot/../../aws-credentials.psm1 -Force

$credentials = Read-AwsCredentials -UserName $userName -Verbose:$VerbosePreference

$Env:AWS_ACCESS_KEY = $credentials.AccessKey
$Env:AWS_SECRET_KEY = $credentials.SecretKey

switch ($Action) {
  install {
    kubectl apply -k $PSScriptRoot
  }
  delete {
    kubectl delete -k $PSScriptRoot
  }
  debug {
    kubectl kustomize $PSScriptRoot
  }
}
