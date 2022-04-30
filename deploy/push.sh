#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
RESET="\033[0m"
RED="\033[0;31m"
YELLOW="\033[0;33m"

__usage()
{
  echo "Usage: $(basename "${BASH_SOURCE[0]}") --provider name [Options]

Required:
    --provider | -p       The container registry provider.
                          Supported providers:
                            - aws
                            - scaleway | scw

Options:
    --tag      | -t       The container image tag.
    --update   | -u       Update the services to run the new container image.
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
  -update|-u)
    declare -r update=true
    ;;
  *)
    __usage
    ;;
  esac

  shift
done

if [[ $provider == "aws" ]]
then
  export AWS_ACCOUNT_ID
  AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query 'Account' --output text)

  docker buildx bake --file docker-bake-aws.hcl --push
  
  if [[ $update == true ]]
  then
    echo "Updating services..."
    aws ecs update-service --cluster app-cluster --service web --force-new-deployment 1> /dev/null
    aws ecs update-service --cluster app-cluster --service api --force-new-deployment 1> /dev/null
  fi
elif [[ $provider == "scaleway" || $provider == "scw" ]]
then
  docker buildx bake --file docker-bake-scaleway.hcl --push

  if [[ $update == true ]]
  then
    echo "Updating services..."
    kubectl rollout restart -f scaleway-k8s/deployment.yaml
  fi
else
  __error "Provider with name '$provider' is not supported by this script" && __usage
fi
