#!/bin/bash

build_command="$PROGRAMFILES/Microsoft Visual Studio 10.0/Common7/IDE/devenv.com"

if (( $(ls -lah *.sln | wc -l) != 1 )); then
  echo "There must be only one solution file for the SuperProject"
  exit 2
fi

solution=$(ls -lah *.sln | awk '{ print $9 }')

# test for top level directry
cd Installer
if (( $? )); then
  echo please move to your top level repo directory
  exit 1
fi
cd -

# test for submodules that need to be pushed

git submodule foreach 'git status' | grep -e "ahead" -e "diverged"
if (( ! $? )); then
  echo "please push modified submodules before sending"
  exit 2
fi

# test for upstream commits that need to be pulled
git fetch
git status | grep -e "behind" -e "diverged"
if (( ! $? )); then
  exit 3
fi

# build and test
status=0
for config in $(awk -f tools/get-sln-configs.awk "$solution");do 
  echo "building $config"
  "$build_command" $solution -rebuild "$config" | awk 'BEGIN { warnings=0; errors=0 } END { print "warnings:", warnings; print "errors:",errors; exit (errors) } /Compile complete/ { warnings += $6; errors += $4}'
  
  ((status=$?))

  for i in $(ls "$USERPROFILE" | grep tmp$); do
    rm "$USERPROFILE/$i"
  done

  if (( $status )); then
    echo "failed configuration: $config"
    exit $status
  fi
done

echo "looks good from here"
exit 0
