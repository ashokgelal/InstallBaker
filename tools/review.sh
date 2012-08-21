#!/usr/bin/bash

set -x 

if (( $# == 0 )) || $1 == "help"; then
  echo "usage $0 <sha>"
  exit 1
fi

git diff $(git rev-list --max-count=2 "$1")
