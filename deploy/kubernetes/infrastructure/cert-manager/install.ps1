[CmdletBinding()]
param (
  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmInstallArgs
)

helm upgrade cert-manager bitnami/cert-manager --install --namespace=cert-manager `
  --create-namespace `
  --values="$PSScriptRoot/values.yaml" $HelmInstallArgs
