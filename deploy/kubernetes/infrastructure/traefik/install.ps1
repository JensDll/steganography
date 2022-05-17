[CmdletBinding()]
param (
  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmInstallArgs
)

helm upgrade traefik traefik/traefik --install --namespace=traefik `
  --create-namespace `
  --values="$PSScriptRoot/values.yaml" $HelmInstallArgs
