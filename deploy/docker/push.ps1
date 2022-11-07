<#
.PARAMETER Provider
  The container registry provider.
#>

[CmdletBinding()]
param (
  [Parameter(Mandatory)]
  [ValidateSet('docker', 'aws')]
  [Alias('p')]
  [string]$Provider
)

switch ($Provider) {
  aws {
    $aws_account_id = aws sts get-caller-identity --query 'Account' --output text
    $aws_region = aws configure get region --profile default

    $repository = "$aws_account_id.dkr.ecr.$aws_region.amazonaws.com/image-data-hiding"
    $platform = 'linux/arm64'
  }
  docker {
    $repository = 'jensdll/image-data-hiding'
    $platform = 'linux/arm64,linux/amd64'
  }
}

$nginx_alpine_version = '1.23.2'

docker buildx bake --file "$PSScriptRoot/docker-bake.hcl" --push `
  --set "nginx-base.context=$PSScriptRoot/nginx-base" `
  --set "*.platform=$platform" `
  --set "nginx-base.args.ALPINE_VERSION=$nginx_alpine_version" `
  --set "nginx-base.tags=${repository}:nginx-base.$nginx_alpine_version-alpine"
