#!/bin/bash

tools/big-brother.sh

for dir in $(awk '$1 ~ /path/ { print $3 }' .gitmodules); do
  cd $dir
  pwd
  if [[ -d tools ]]; then 
    tools/big-brother.sh
  fi
  cd - >/dev/null
done
