#!/bin/bash

result=$(git diff --name-only "$GITHUB_BASE_REF" HEAD)
readonly result

echo "$result"
echo "The project passed is $1"
