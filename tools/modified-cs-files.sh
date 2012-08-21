#!/bin/bash

git status -s | grep -v \.Designer\.cs > status.$$

if [ -e .gitmodules ]; then
    awk '$1 ~ /path/ && $2 ~ /=/ {print $3}' .gitmodules > modules.$$

    # find modified files in submodules
    for i in $(cat modules.$$); do
    grep "M $i" status.$$ > /dev/null
    if (( ! $? )); then
        cd $i > /dev/null
        git status -s | grep -v \.Designer\.cs > status.$$
        awk '$1 ~ /M/ && $2 ~ /\.cs$/ {print $2}' status.$$ >> files.$$
        awk '$1 ~ /A/ && $2 ~ /\.cs$/ {print $2}' status.$$ >> files.$$
        awk '$1 ~ /\?\?/ && $2 ~ /\.cs$/ {print $2}' status.$$ >> files.$$
        for j in $(cat files.$$); do 
        echo -n "$i/"
        echo $j
        done
        rm files.$$
        rm status.$$
        cd - > /dev/null
    fi
    done
    rm modules.$$
fi

# find modified files here
awk '$1 ~ /M/ && $2 ~ /\.cs$/ {print $2}' status.$$
awk '$1 ~ /A/ && $2 ~ /\.cs$/ {print $2}' status.$$
awk '$1 ~ /\?\?/ && $2 ~ /\.cs$/ {print $2}' status.$$
rm status.$$
