#!/bin/bash

RESET="\033[0m"
RED="\033[0;31m"
YELLOW="\033[0;33m"

__usage()
{
  echo "Usage: $(basename "${BASH_SOURCE[0]}") [options]

Options:
    --provider | -p     The container registry provider.
                        Supported providers:
                            - aws
                            - docker
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
  *)
    __error "Unknown option: $1" && __usage
    ;;
  esac

  shift
done

script_root=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

case "$provider" in
aws)
  aws_account_id=$(aws sts get-caller-identity --query 'Account' --output text)
  aws_region=$(aws configure get region)
  repository="${aws_account_id}.dkr.ecr.${aws_region}.amazonaws.com/image-data-hiding"
  platform="linux/arm64"
  ;;
docker)
  repository="jensdll/image-data-hiding"
  platform="linux/arm64,linux/amd64"
  ;;
*)
  __error "Unknown provider: $provider" && __usage
esac

nginx_alpine_version="1.23.2"

docker buildx bake --file "$script_root/docker-bake.hcl" --push \
  --set "nginx-base.context=$script_root/nginx-base" \
  --set "*.platform=$platform" \
  --set "nginx-base.args.ALPINE_VERSION=$nginx_alpine_version" \
  --set "nginx-base.tags=$repository:nginx-base.1.23.2-alpine"
