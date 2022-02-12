#!/bin/bash

declare -r pattern=".*/(${1//+([ ,])/|})/.*"

git fetch --depth=1 origin +refs/heads/"$GITHUB_BASE_REF":refs/remotes/origin/"$GITHUB_BASE_REF"
git branch --track "$GITHUB_BASE_REF" origin/"$GITHUB_BASE_REF"

while [[ -z "${mergeBase:=$(git merge-base "$GITHUB_BASE_REF" HEAD)}" ]]
do     
  git fetch --deepen=1 origin "$GITHUB_BASE_REF"
done

while read -r line
do
  if [[ $line =~ $pattern ]]
  then
    echo "Change found in: $line"
    echo "::set-output name=${BASH_REMATCH[1]}::true"
  fi
done < <(git diff --name-only HEAD "$mergeBase")
