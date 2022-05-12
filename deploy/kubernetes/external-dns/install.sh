#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

cd "$(dirname "${BASH_SOURCE[0]}")" || exit 1

cdk synth --json | jq "*.UserName"

# read -r -a credentials <<< "$(aws iam create-access-key --user-name "$username" --query "AccessKey.[AccessKeyId, SecretAccessKey]" --output text)"

# aws configure set aws_access_key_id "${credentials[0]}" --profile "$username"
# aws configure set aws_secret_access_key "${credentials[1]}" --profile "$username"

# aws_access_key_id=$(aws configure get aws_access_key_id --profile "$username")
# aws_secret_access_key=$(aws configure get aws_secret_access_key --profile "$username")

# helm upgrade external-dns bitnami/external-dns --install --namespace=external-dns \
#   --create-namespace \
#   --set "aws.credentials.accessKey=$aws_access_key_id,aws.credentials.secretKey=$aws_secret_access_key" \
#   --values="$DIR/values.yaml" "$@"
