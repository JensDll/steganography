#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
RESET="\033[0m"
RED="\033[0;31m"
YELLOW="\033[0;33m"

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
  REPOSITORY="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com/image-data-hiding"
  PLATFORM="linux/arm64"
  ;;
docker)
  REPOSITORY="jensdll/image-data-hiding"
  PLATFORM="linux/arm64,linux/amd64"
  ;;
*)
  __error "Unknown provider: $provider" && __usage
esac

docker buildx bake --file "$DIR/docker-bake.hcl" --push \
  --set "nginx-base.context=$DIR/nginx-base" \
  --set "*.platform=$PLATFORM" \
  --set "nginx-base.args.ALPINE_VERSION=1.23.2" \
  --set "nginx-base.tags=$REPOSITORY:nginx-base.1.23.2-alpine"
