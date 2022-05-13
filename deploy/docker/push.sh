#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
RESET="\033[0m"
RED="\033[0;31m"
YELLOW="\033[0;33m"

export DOCKER_BAKE_REPOSITORY
export DOCKER_BAKE_CONTEXT
DOCKER_BAKE_CONTEXT="$(git rev-parse --show-toplevel)/services"
export DOCKER_BAKE_PLATFORM

__usage()
{
  echo "Usage: $(basename "${BASH_SOURCE[0]}") [options]

Options:
    --provider | -p       The container registry provider.
                          Supported providers:
                            - aws
                            - docker

    --tag      | -t       The container image tag.
"

  exit 2
}

__error() {
    echo -e "${RED}error: $*${RESET}" 1>&2
}

__warn() {
    echo -e "${YELLOW}warning: $*${RESET}"
}

while [[ $# -gt 0 ]]
do
  # Replace leading "--" with "-" and convert to lowercase
  declare -l opt="${1/#--/-}"

  case "$opt" in
  -\?|-help|-h)
    __usage
    ;;
  -provider|-p)
    shift
    declare -r provider="$1"
    [[ -z $provider ]] && __error "Missing value for parameter --provider" && __usage
    ;;
  -tag|-t)
    shift
    declare -r tag="$1"
    [[ -z $tag ]] && __error "Missing value for parameter --tag" && __usage
    export TAG=$tag
    ;;
  *)
    __error "Unknown option: $1" && __usage
    ;;
  esac

  shift
done

case "$provider" in
aws)
  AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query 'Account' --output text)
  AWS_REGION=$(aws configure get region)

  DOCKER_BAKE_REPOSITORY="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com/image-data-hiding"
  DOCKER_BAKE_PLATFORM="linux/arm64"

  docker buildx bake --file "$DIR/docker-bake.hcl" --push
  ;;
docker)
  DOCKER_BAKE_REPOSITORY="jensdll/image-data-hiding"
  DOCKER_BAKE_PLATFORM="linux/arm64,linux/amd64"

  docker buildx bake --file "$DIR/docker-bake.hcl" --push
  ;;
*)
  __error "Unknown provider: $provider" && __usage
esac
