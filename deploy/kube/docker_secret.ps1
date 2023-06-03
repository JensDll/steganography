param(
  [string]$Username = 'jensdll',
  [Parameter(Mandatory)]
  [string]$Email,
  [string]$Namespace = $(kubectl config view --minify -o jsonpath='{..namespace}'),
  [Parameter(Mandatory)]
  [securestring]$Token
)

kubectl create secret docker-registry docker-pull --docker-server=docker.io `
  --docker-username="$Username" `
  --docker-password=$(ConvertFrom-SecureString -SecureString $Token -AsPlainText) `
  --docker-email="$Email" `
  --namespace="$Namespace"
