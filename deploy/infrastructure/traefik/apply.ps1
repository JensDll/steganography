[CmdletBinding()]
param (
  [Parameter(Position = 0, Mandatory)]
  [ValidateSet('install', 'delete', 'dashboard')]
  [string]$Action,
  [string]$Version = '20.8.0',
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
  throw 'Failed to configure namespace'
}

switch ($Action) {
  install {
    helm upgrade traefik traefik/traefik --install --namespace=$namespace `
      --version=$Version `
      --values=$valuesYaml $HelmArgs
  }
  delete {
    helm delete traefik --namespace=$namespace $HelmArgs
    kubectl delete namespace $namespace $dryRun
  }
  dashboard {
    $podName = kubectl get pods --selector 'app.kubernetes.io/name=traefik' --output=name --namespace=$namespace
    kubectl port-forward $podName 9000:9000 --namespace=$namespace
  }
}
