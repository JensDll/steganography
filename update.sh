#!/usr/bin/bash

export AWS_ACCOUNT_ID
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query 'Account' --output text)

docker buildx bake --file docker-bake.hcl --print && \
aws ecs update-service --cluster app --service web --force-new-deployment