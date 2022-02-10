#!/bin/bash

result=$(git diff --name-only HEAD~1 HEAD)
readonly result

echo "$result"