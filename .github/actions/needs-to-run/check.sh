#!/bin/bash

result=$(git diff --name-only "$PR_BASE_SHA" HEAD)
readonly result

echo "$result"
echo "The project passed is $1"
