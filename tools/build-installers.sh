#!/bin/bash

# The path to VS is different in Cygwin vs MSysGit
uname -a | grep CYGWIN
if (( $? )); then
   PFTarget=$PROGRAMFILES
else
   PFTarget=/cygdrive/c/$(echo $PROGRAMFILES | cut -d\\ -f2) 
fi

build_command="$PFTarget/Microsoft Visual Studio 10.0/Common7/IDE/devenv.com"

if (( $(ls -lah *.sln | wc -l) != 1 )); then
  echo "There must be only one solution file for the SuperProject"
  exit 1
fi

solution=$(ls *.sln)

# test for top level directry
cd Installer
if (( $? )); then
  echo please move to your top level repo directory
  exit 2
fi
cd -

# remove previous installers
find Installer -name "*.msi" | xargs rm -f

# make/clean installers directory
if [[ ! -d tools/installers/ ]]; then
  mkdir tools/installers/
else
  rm -f tools/installers/*
fi

# build installer
  echo "BuildCommand = $build_command $solution -rebuild Release|x86"
  "$build_command" $solution -rebuild "Release|x86"

  for i in $(ls "$USERPROFILE" | grep tmp$); do
    rm -f "$USERPROFILE"/$i
  done

# copy installers to tools dir
for file in $(find Installer -name "*.msi"); do
  cp -v $file tools/installers
done
