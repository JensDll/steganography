#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

install()
{
  if ! kubectl get namespace traefik &> /dev/null
  then
    kubectl create namespace traefik
  fi

  helm install traefik traefik/traefik --namespace=traefik --values="$DIR/values.yaml"
}

uninstall()
{
  helm uninstall traefik --namespace=traefik
  kubectl delete namespace traefik
}

debug()
{
  helm install traefik traefik/traefik --namespace=traefik  --values="$DIR/values.yaml" --dry-run --debug
}

case $1 in
install)
  install
  ;;
uninstall)
  uninstall
  ;;
debug)
  debug
  ;;
esac