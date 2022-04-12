#!/usr/bin/bash

export AWS_ACCOUNT_ID
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query 'Account' --output text)

docker buildx bake --file docker-bake.hcl --push

# aws ecs update-service --cluster app-cluster --service web --force-new-deployment
# aws ecs update-service --cluster app-cluster --service api --force-new-deployment