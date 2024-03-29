# Media Downloader

> A CLI app that runs against a storage queue and downloads media based on enqueued work items.

Each unit of work includes a `PayloadType` which identifies how the bot should do work.

## Environment Variables

Honors the following configuration environment variables, running in context of CWD if not overwritten.

| variable | purpose | default |
|---|---|---|
| DRONE_DOWNLOADER_DIRECTORY | Working directory for `drone-downloader`. | `cwd` when `drone-downloader` is invoked. |
| DRONE_DOWNLOADER_LOG_DIRECTORY | Where `drone-downloader` writes logs. | `<cwd>/logs` |
| DRONE_DOWNLOADER_DOWNLOAD_DIRECTORY | Where `drone-downloader` places its downloaded data. | `<cwd>/download` |

## youtube-dl

Needs 2 arguments from the enqueued work item:

- [x] Youtube URL
- [x] One of two options: [`bestaudio`, `bestvideo+bestaudio`]

After that, it

- [x] And starts to download to the staging folder using `youtubedl` on the PATH.
   - Outputs to `<videoId>.(webm/m4a/mp4/flv)`
   - Dumps `<videoId>.info.json` with metadata alongside.
- [x] Also downloads the highest def thumbnail image it can find, 
   - `<videoId>.jpg`
- [x] Uses `ffmpeg` on the path to convert `.webm` -> `.m4a`