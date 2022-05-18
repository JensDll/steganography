[CmdletBinding()]
param (
  [Parameter(Position = 0, Mandatory)]
  [ValidateSet('install', 'delete')]
  [string]$Action,

  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmArgs
)

$namespace = 'cert-manager'

switch ($Action) {
  install {
    helm upgrade cert-manager bitnami/cert-manager --install --namespace=$namespace `
      --create-namespace `
      --values="$PSScriptRoot/values.yaml" $HelmArgs
  }
  delete {
    helm delete cert-manager --namespace=$namespace $HelmArgs

    if (-not ($HelmArgs -contains '--dry-run')) {
      kubectl delete namespace $namespace
    }
  }
}