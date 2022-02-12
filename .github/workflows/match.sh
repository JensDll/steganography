#!/bin/bash

shopt -s extglob

declare -r projects=${1//+([ ,])/|}
declare -r pattern=".*/($projects)/.*"

IFS="|"
for project in $projects
do
  echo "::set-output name=$project::false"
done
unset IFS

echo "Fetching commits of pull request ..."

git fetch --depth=1 origin +refs/heads/"$GITHUB_BASE_REF":refs/remotes/origin/"$GITHUB_BASE_REF"
git branch --track "$GITHUB_BASE_REF" origin/"$GITHUB_BASE_REF"

while [[ -z "${mergeBase:=$(git merge-base "$GITHUB_BASE_REF" HEAD)}" ]]
do     
  git fetch --deepen=5 origin "$GITHUB_BASE_REF"
done

echo "The pattern is: $pattern"
echo "Checking for changes ..."

while read -r line
do
  line="/$line"
  if [[ $line =~ $pattern ]]
  then
    echo "Match found in: $line. The match was: ${BASH_REMATCH[1]}"
    echo "::set-output name=${BASH_REMATCH[1]}::true"
  fi
done < <(git diff --name-only HEAD "$mergeBase")
