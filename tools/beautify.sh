#!/bin/bash

if (( $# != 1 )); then
  echo "Usage: $(basename $0) <tool dir>"
  exit 0
fi

tooldir=$1

echo "beautifying..."
for file in $(${tooldir}/modified-cs-files.sh); do
  ${tooldir}/arrange.sh $tooldir $file
done

exit 0
