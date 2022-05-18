[CmdletBinding()]
param (
  [Parameter(Position = 0, Mandatory)]
  [ValidateSet('install', 'delete')]
  [string]$Action,

  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmArgs
)

$namespace = 'traefik'

switch ($Action) {
  install {
    helm upgrade traefik traefik/traefik --install --namespace=$namespace `
      --create-namespace `
      --values="$PSScriptRoot/values.yaml" $HelmArgs
  }
  delete {
    helm delete traefik --namespace=$namespace $HelmArgs
  
    if (-not ($HelmArgs -contains '--dry-run')) {
      kubectl delete namespace $namespace
    }
  }
}