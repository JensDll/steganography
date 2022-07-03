[CmdletBinding()]
param (
  [Parameter(Position = 0, Mandatory)]
  [ValidateSet('install', 'delete', 'dashboard')]
  [string]$Action,

  [Parameter(ValueFromRemainingArguments)]
  [string[]]$HelmArgs
)

$namespace = 'traefik'

switch ($Action) {
  install {
    if ($HelmArgs -contains '--dry-run') {
      kubectl apply -f "$PSScriptRoot/middleware.yaml" --namespace=$namespace --dry-run=client
    } else {
      kubectl apply -f "$PSScriptRoot/middleware.yaml" --namespace=$namespace
    }

    helm upgrade traefik traefik/traefik --install --namespace=$namespace `
      --create-namespace `
      --values="$PSScriptRoot/values.yaml" $HelmArgs
  }
  delete {
    helm delete traefik --namespace=$namespace $HelmArgs
  
    if ($HelmArgs -contains '--dry-run') {
      kubectl delete -f "$PSScriptRoot/middleware.yaml" --namespace=$namespace --dry-run=client
      kubectl delete namespace $namespace --dry-run=client
    } else {
      kubectl delete -f "$PSScriptRoot/middleware.yaml" --namespace=$namespace
      kubectl delete namespace $namespace
    }
  }
  dashboard {
    $podName = kubectl get pods --selector "app.kubernetes.io/name=traefik" --output=name --namespace=$namespace
    kubectl port-forward $podName 9000:9000 --namespace=$namespace
  }
}