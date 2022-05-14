<#
.PARAMETER Provider
    The container registry provider.

.PARAMETER Tag
    The tag to use for the image.
#>

[CmdletBinding()]
param (
  [Parameter(Mandatory)]
  [ValidateSet('docker', 'aws')]
  [Alias('p')]
  [string]$Provider,

  [Alias('t')]
  [string]$Tag
)

switch ($Provider) {
  aws {
    $AWS_ACCOUNT_ID = aws sts get-caller-identity --query 'Account' --output text
    $AWS_REGION = aws configure get region --profile default

    $Env:DOCKER_BAKE_REPOSITORY = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/image-data-hiding"
    $Env:DOCKER_BAKE_PLATFORM = 'linux/arm64'

    docker buildx bake --file "$PSScriptRoot/docker-bake.hcl" --print
  }
  docker {
    $Env:DOCKER_BAKE_REPOSITORY = 'jensdll/image-data-hiding'
    $Env:DOCKER_BAKE_PLATFORM = 'linux/arm64,linux/amd64'

    docker buildx bake --file "$PSScriptRoot/docker-bake.hcl" --print
  }
}