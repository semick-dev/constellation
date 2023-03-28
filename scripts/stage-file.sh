#!/usr/bin/bash
target_file=${1:?"Must provide a target file."}
target_directory=${2:?"Must provide a staging directory."}

if [[ ! -e $1 ]]; then
    echo "File must exist to move it to staging."
    exit 1
fi

if [[ ! -d $2 ]]; then
    echo "Creating staging directory ${2}"
    mkdir $2
fi

stage_file() {
    local file_name=$1
    local staging_directory=$2

    if [[ ! file_name =~ .*.json ]]; then
        # we will need to update the target file name to find the json metadata
        file_name = "${file_name%.*}"
    fi

    local new_file_name=(jq '.title' $file_name)
    local extension=(jq '.ext' $file_name)
    local channel=(jq '.channel' $file_name)
    local video_id=(jq '.id' $file_name)
    local target_directory="$staging_directory/$channel/"

    # todo, make this a case statement
    if [[ $extension == ".webm" ]]; then
        extension=".m4a"
    fi

    old_file_name="${video_id}{extension}"
    new_file_name="${new_file_name}${extension}"

    # now all we need to do is copy to the staging directory
    echo "mv $old_file_name " + "$target_directory/$new_file_name"
}

stage_file $1 $2

