#!/bin/bash

result=$(git diff --name-only "$PR_BASE_SHA" HEAD)
readonly result

env

echo "The HEAD SHA from env is: $PR_BASE_SHA"
echo "The HEAD SHA is: $(git rev-parse HEAD)"
echo "The project passed is: $1"
echo "The diff is:"
echo "$result"