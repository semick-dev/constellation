# Media Downloader

> A CLI app that runs against a storage queue.

## youtube-dl download method

Needs 2 arguments from the enqueued work item:

- [ ] Youtube URL
- [ ] One of two options: [`bestaudio`, `bestvideo+bestaudio`]
- [ ] And starts to download to the staging. On arrival in staging, update owner to jellyfin
