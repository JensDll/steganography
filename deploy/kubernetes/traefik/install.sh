#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

helm upgrade traefik traefik/traefik --install --namespace=traefik \
  --create-namespace \
  --values="$DIR/values.yaml" "$@"
