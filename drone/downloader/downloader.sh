# assumes that environment variable DRONE_DOWNLOADER_SAS is loaded with a valid azure storage connection string or SAS URL
while :; do
  drone-downloader $DRONE_DOWNLOADER_SAS
  sleep 10
done
