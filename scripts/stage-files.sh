#!/usr/bin/bash

# Finds and stages all of the completed downloads (discovered by thumbnail.jpg) and attempts staging on each

if [[ ! $STAGING_DIRECTORY ]]; then
    target_directory=${2:?"Must provide a staging directory as second argument."}
else
    target_directory=$STAGING_DIRECTORY
fi

for i in $(find ./ -name "*.jpg"); do
    ./stage-file.sh $i $target_directory
done
