#!/bin/bash

RESET="\033[0m"
RED="\033[0;31m"
YELLOW="\033[0;33m"

__usage()
{
  echo "Usage: $(basename "${BASH_SOURCE[0]}") [Options] [[--] <Arguments>...]

Arguments:
    <Arguments>...        Arguments passed to the command. Variable number of arguments allowed.

Options:
    --provider | -p       The container registry provider (required).
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
  opt="$(echo "${1/#--/-}" | awk '{print tolower($0)}')"

  case "$opt" in
  -\?|-help|-h)
    __usage
    ;;
  -provider|-p)
    shift
    provider="$1"
    [[ -z $provider ]] && __error "Missing value for parameter --provider" && __usage
    ;;
  -tag|-t)
    shift
    export TAG="$1"
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

  docker buildx bake --file aws/docker-bake.hcl --push
  
  # aws ecs update-service --cluster app-cluster --service web --force-new-deployment 1> /dev/null
  # aws ecs update-service --cluster app-cluster --service api --force-new-deployment 1> /dev/null
elif [[ $provider == "scaleway" ]]
then
  docker buildx bake --file scaleway/docker-bake.hcl --push

  # kubectl rollout restart -f k8s/app.yaml
else
  __error "Missing value for parameter --provider" && __usage
fi
