#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
RESET="\033[0m"
RED="\033[0;31m"
YELLOW="\033[0;33m"

__usage()
{
  echo "Usage: $(basename "${BASH_SOURCE[0]}") [options] [-- args...]

Arguments:
    args...               Arguments passed to helm upgrade --install.

Options:
    --recreate | -r       Recreate credentials.
"

  exit 2
}

__error() {
    echo -e "${RED}error: $*${RESET}" 1>&2
}

__warn() {
    echo -e "${YELLOW}warning: $*${RESET}"
}

cd "$DIR" || exit

helmArgs=()

while [[ $# -gt 0 ]]
do
  # Replace leading "--" with "-" and convert to lowercase
  declare -l opt="${1/#--/-}"

  case "$opt" in
  -\?|-help|-h)
    __usage
    ;;
  -recreate|-r)
    recreate=true
    ;;
  -)
    shift
    helmArgs+=("$@")
    break
    ;;
  *)
    __error "Unknown option: $1" && __usage
    ;;
  esac

  shift
done

userName=$(cdk synth --json | jq --raw-output ".Resources[] | select(.Type == \"AWS::IAM::User\") | .Properties.UserName")

cdk deploy

createCredentials()
{
  read -r -a credentials <<< "$(aws iam create-access-key --user-name "$userName" --query "AccessKey.[AccessKeyId, SecretAccessKey]" --output text)"
  echo "${credentials[@]}" > "$DIR/aws.credentials"
}

keyIds=$(aws iam list-access-keys --user-name "$userName" --query "AccessKeyMetadata[].AccessKeyId" --output text)

if [[ -n $keyIds && $recreate == true ]]
then
  for keyId in $keyIds
  do
    aws iam delete-access-key --access-key-id "$keyId" --user-name "$userName"
  done

  createCredentials
elif [[ -f "$DIR/aws.credentials" ]]
then
  read -r -a credentials <<< "$(cat "$DIR/aws.credentials")"
else
  createCredentials
fi

helm upgrade external-dns bitnami/external-dns --install --namespace=external-dns \
  --create-namespace \
  --set "aws.credentials.accessKey=${credentials[0]},aws.credentials.secretKey=${credentials[1]}" \
  --values="$DIR/values.yaml" "${helmArgs[@]}"
