#!/usr/bin/bash

# Finds and stages all of the completed downloads (discovered by thumbnail.jpg) and attempts staging on each
if [[ ! $STAGING_DIRECTORY ]]; then
    target_directory=${1:?"Must provide a staging directory as an input to this script."}
else
    target_directory=$STAGING_DIRECTORY
fi

for i in $(find ./ -name "*.jpg"); do
    ./stage-file.sh $i $target_directory
done
