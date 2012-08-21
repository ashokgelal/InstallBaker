#!/bin/bash

for dir in $(awk '$1 ~ /path/ { print $3 }' .gitmodules); do
  cd $dir
  pwd
  if [[ -d tools ]]; then 
    git checkout master
    git pull
    cd tools
    git checkout master
    git pull
    cd ..
    git commit -am "tooldir update"
    git push
  fi
  cd ..
done
