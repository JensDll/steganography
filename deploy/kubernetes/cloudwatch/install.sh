#!/bin/bash

userName=$(cdk synth --json | jq --raw-output ".Resources[] | select(.Type == \"AWS::IAM::User\") | .Properties.UserName")

echo "$userName"
# export AWS_ACCESS_KEY_ID
# AWS_ACCESS_KEY_ID=$(aws configure get aws_access_key_id --profile cloudwatch_access)
# export AWS_SECRET_ACCESS_KEY
# AWS_SECRET_ACCESS_KEY=$(aws configure get aws_secret_access_key --profile cloudwatch_access)

# kubectl apply -k .
