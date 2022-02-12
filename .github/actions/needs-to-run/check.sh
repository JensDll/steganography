#!/bin/bash

git fetch --depth=1 origin +refs/heads/"$GITHUB_BASE_REF":refs/remotes/origin/"$GITHUB_BASE_REF"
git branch --track "$GITHUB_BASE_REF" origin/"$GITHUB_BASE_REF"

while [ -z "${mergeBase:=$(git merge-base "$GITHUB_BASE_REF" HEAD)}" ]
do     
  git fetch --deepen=1 origin "$GITHUB_BASE_REF"
done

while read -r line
do
  echo "$line"
  if [[ $line == $1* ]]
  then
    echo "::set-output name=result::true"
    exit 0
  fi
done < <(git diff --name-only HEAD "$mergeBase")

echo "::set-output name=result::false"