#!/bin/bash

if (( $# != 2 )); then
  echo "Usage: $(basename $0) <tool dir> <file>"
  exit 0
fi

tooldir=$1
file=$2

narrange=${tooldir}/NArrange/bin/narrange-console.exe
config=${tooldir}/NArrange/ChanConfig.xml

function arrange {
  echo "ARRANGING $file"
  $narrange $1 arranged.$$ -c:$config > /dev/null
  diff $1 arranged.$$ > /dev/null
  if (( $? )); then
    cp arranged.$$ $1 > /dev/null
  fi
  rm arranged.$$ > /dev/null
}

if [[ -e $file ]]; then
    arrange $file
fi

exit 0
