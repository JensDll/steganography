#!/bin/bash

kubectl create secret docker-registry "$1" --docker-server=docker.io \
  --docker-username="$2" \
  --docker-password="$3" \
  --docker-email="$4" \
  --namespace="$5"
