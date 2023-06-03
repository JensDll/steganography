[CmdletBinding()]
param (
  [Parameter(Position = 0, Mandatory)]
  [ValidateSet('install', 'delete')]
  [string]$Action,
  [string]$Version = 'v1.12.1',
  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmArgs
)

$namespaceYaml = Join-Path $PSScriptRoot 'namespace.yaml'
$valuesYaml = Join-Path $PSScriptRoot 'values.yaml'

$isDryRun = $HelmArgs -contains '--dry-run'
$dryRun = "--dry-run=$($isDryRun ? 'client' : 'none')"

if ($(kubectl apply -f $namespaceYaml $dryRun) -match '^namespace/(?<namespace>\S+)') {
  $namespace = $Matches.namespace
} else {
  Write-Error 'Failed to configure namespace'
  exit 1
}

switch ($Action) {
  install {
    helm upgrade cert-manager jetstack/cert-manager --install --namespace=$namespace `
      --version=$Version `
      --values=$valuesYaml $HelmArgs
  }
  delete {
    helm delete cert-manager --namespace=$namespace $HelmArgs
    kubectl delete namespace $namespace $dryRun
  }
}
