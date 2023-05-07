#!/usr/bin/bash

# Currently this script expects to be invoked from _within_ the downloads directory.
# Specifically where we access the info json, we are passing "./" in front of the path
# so that file names starting with '-' don't break the commands.
target_file=${1:?"Must provide a target file as first argument."}

echo $STAGING_DIRECTORY

if [[ ! $STAGING_DIRECTORY ]]; then
    target_directory=${2:?"Must provide a staging directory as second argument."}
else
    target_directory=$STAGING_DIRECTORY
fi

if [[ ! -e $1 ]]; then
    echo "File must exist to move it to staging."
    exit 1
fi

if [[ ! -d $target_directory ]]; then
    echo "Creating staging directory ${target_directory}"
    mkdir $target_directory
fi

stage_file() {
    local file_name=$1
    local staging_directory=$2

    if [[ ! "$file_name" =~ .*.json ]]; then
        # we will need to update the target file name to find the json metadata
        file_name=${file_name%.*}.info.json
    fi

    local new_file_name=$(jq -r '.title' ./$file_name)
    local extension=$(jq -r '.ext' ./$file_name)
    local channel=$(jq -r '.channel' ./$file_name)
    local video_id=$(jq -r '.id' ./$file_name)
    local target_directory=$staging_directory$channel/
    local tmp_file=/tmp/$video_id.info.json
    local new_poster=${new_file_name}.jpg
    local old_poster=${video_id}.jpg
    local webm_file="./$video_id.webm"
    local mkv_file="./$video_id.mkv"

    if [[ $extension == "webm" ]]; then
        extension=m4a
    fi

    if [[ $extension == "mkv" ]]; then
        extension=mp4
    fi

    local old_file_name=${video_id}.${extension}
    local new_file_name=${new_file_name}.${extension}

    # handle webm
    if [[ -e "$webm_file" ]]; then
        if [[ ! -e "$old_file_name" && "$old_file_name" =~ .*.m4a ]]; then
            ffmpeg -i "$webm_file" -vn "$old_file_name"
        fi

        rm "$webm_file"
    fi

    # handle mkv
    if [[ -e "$mkv_file" ]]; then
        if [[ ! -e "$old_file_name" && "$old_file_name" =~ .*.mp4 ]]; then
            ffmpeg -i $mkv_file -codec copy -strict -2 "$old_file_name"
        fi
    fi

    mkdir -p "$target_directory"

    mv "./$old_file_name" "$target_directory$new_file_name"
    mv "./$old_poster" "$target_directory$new_poster"

    # update downloaded bit
    jq '.downloaded = true' ./$file_name > $tmp_file && mv $tmp_file ./$file_name
    echo "Wrote $target_directory$new_file_name and thumbnail"
}

stage_file $1 $target_directory
