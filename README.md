# This is an unstable fix, I'm currently rewriting most of the existing code, please be patient.

# This is a beta version. Expect bugs and missing features that will be added soon. Please report any [bugs here](https://github.com/Alleexx129/TrackTrek-CS/issues). Download Here [(Latest beta)](https://github.com/Alleexx129/TrackTrek-CS/releases/tag/v1.2.5-beta1)

![TrackTrek logo](https://raw.githubusercontent.com/Alleexx129/Nyanko/refs/heads/main/logo.png "TrackTrek")

# TrackTrek

## What is this?
**TrackTrek** is a C# utility designed to extract audio streams and media metadata from YouTube. Built as a front-end wrapper for `YoutubeExplode`, it allows you to archive your own videos, save royalty-free audio, and study stream data. It pulls the audio and automatically tags it with the correct metadata, including the title, artist, album info, and artwork.

## Key Features:
- **Stream Extraction**: Save media locally in MP3 format for personal backups or offline use.
- **Automated Tagging**: Automatically writes the title, artist, album, and genre tags directly into your files.
- **Artwork Archival**: Fetches and embeds the video thumbnail or album art.
- **Built-in Search**: Easily search for content using keywords, or paste a link/playlist directly.

### Install dependencies
[.Net Desktop Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.14-windows-x64-installer)

## How to use?
- Enter a YouTube link, keyword, or a YouTube playlist.
- If you search with keywords, you can choose from the top 5 results (max results are customizable in settings).

## Dependencies
- YoutubeExplode
- TagLib
- FFmpeg (TrackTrek uses this to process the audio streams)

## Legal & Disclaimer
TrackTrek is an open-source tool intended purely for educational use, personal archiving, and working with Creative Commons/Royalty-Free media. 

By using TrackTrek, you agree that:
- You will only use this tool for lawful purposes (like backing up your own created content).
- You are strictly responsible for your own actions and how you use the software.
- You must comply with applicable laws and platform terms of service.
- The developer is not responsible for any user misuse or damages.

**For Rights Holders:** This project is a neutral technical utility. If you have concerns about the repository, please open an issue.

For more information, read the full [License](https://raw.githubusercontent.com/Alleexx129/TrackTrek-CS/refs/heads/master/LICENSE.txt).
