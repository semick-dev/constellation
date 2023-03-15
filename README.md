# constellation
A constellation of tools that work together to form the core of my home automation suite. Vaguely space-themed.

Pointless rehash of a bunch of other crap, but I wanted to do this for myself.

Mostly written in .NET. Forays into other languages as necessary.

## Usage

Requirements:

- [x] Install .NET 6.0 SDK (`drone/downloader`)
- [x] Install Node 19 (`watcher`)
- [x] `npm install -g typescript` (`watcher`)
- [x] `npm install -g webpack` (`watcher`)

Installation

- [x] Clone this repo.
- [x] On linux, use `install.sh`. On Windows, use `install.ps1`.

## Repo Directory

|Name|Purpose|
|---|---|
|[`drone/downloader`](drone/downloader/README.md)|.NET executable that pulls a unit of work off a download queue, downloads the item, and exits.|
|[`watcher`](watcher/README.md)| Static Website that offers interactions with `constellation` automation and services. Communicates using a storage account.|



