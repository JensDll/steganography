#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

helm upgrade cert-manager bitnami/cert-manager --install --namespace=cert-manager \
  --create-namespace \
  --values="$DIR/values.yaml" "$@"
