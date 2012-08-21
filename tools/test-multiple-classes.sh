#!/bin/bash

awk -f tools/test-multiple-classes.awk $(find . -name "*.cs") | tee tmp.$$

exit_code=$(cat tmp.$$ | wc -l)
rm tmp.$$
exit $exit_code
