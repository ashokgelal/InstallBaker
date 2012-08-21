#!/bin/bash

awk -f tools/test-bad-conversions.awk $(find . -name "*.cs") | tee tmp.$$

exit_code=$(cat tmp.$$ | wc -l)
rm tmp.$$
exit $exit_code
