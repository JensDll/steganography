#!/bin/bash

git fetch --depth=1 origin +refs/heads/"$GITHUB_BASE_REF":refs/remotes/origin/"$GITHUB_BASE_REF"
git branch --track "$GITHUB_BASE_REF" origin/"$GITHUB_BASE_REF"

while [ -z "${mergeBase:=$(git merge-base "$GITHUB_BASE_REF" HEAD)}" ]
do     
  git fetch --deepen=5 origin "$GITHUB_BASE_REF"
done

echo "The project to search for is: $1"
echo "The head is: $(git rev-parse HEAD)"
echo "The merge base is: $mergeBase"
echo "The diff is:"
git diff --name-only HEAD "$mergeBase"

echo "::set-output name=result::true"