#!/usr/bin/bash

__usage()
{
  echo "Usage: $(basename "${BASH_SOURCE[0]}") [Options] [[--] <Arguments>...]
Options:
    --provider | -p               The container registry provider.
"

  exit 2
}

declare -r short=p:,h
declare -r long=provider:,help
opts=$(getopt --alternative --name push --options $short --longoptions $long -- "$@")
readonly opts

eval set -- "$opts"

while [[ $# -gt 0 ]]
do
  case "$1" in
    --provider|-p)
      provider="$2"
      shift 2
      ;;
    --help|-h)
      __usage
      ;;
    --)
      shift
      break
      ;;
    *)
      __usage
      ;;
  esac
done

if [[ $provider == "aws" ]]
then
  export AWS_ACCOUNT_ID
  AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query 'Account' --output text)

  docker buildx bake --file docker-bake-aws.hcl --push
  
  aws ecs update-service --cluster app-cluster --service web --force-new-deployment 1> /dev/null
  aws ecs update-service --cluster app-cluster --service api --force-new-deployment 1> /dev/null
elif [[ $provider == "scaleway" ]]
then
  docker buildx bake --file docker-bake-scaleway.hcl --push
  kubectl rollout restart -f k8s/app.yaml 
fi
