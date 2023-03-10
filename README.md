[![Docker Image Version (tag latest semver)](https://img.shields.io/docker/v/mmuffins/nzbhydra-scheduler/latest)](https://hub.docker.com/r/mmuffins/nzbhydra-scheduler)

# NzbHydra Scheduler
A simple scheduler for nzbhydra.

## Usage

### docker-compose

```yaml
---
version: "3.8"
   
services:
  nzbhydra-schedule:
    image: nzbhydra-scheduler:latest
    container_name: nzbhydra-scheduler
    restart: unless-stopped
    environment:
      - PUID="1000"
      - PGID="1000"
      - TZ=Europe/Vienna
      - CRON=1 13 * * 1
      - SEARCHFREQUENCY=72
      - LOGLEVEL=debug
      - SEARCHTERMSFILE=/config/searchterms
      - GROUPSFILE=/config/groups
      - SHOWSFILE=/config/shows
      - NZBOUTPUTDIRECTORY=/output
      - NZBHYDRAURI=nzbhydra:5076
      - NZBHYDRAAPIKEY=supersecretapikey
      - MINSIZE=1
      - MAXSIZE=900000
      - MAXAGE=7
      - CATEGORY=Anime
      - INDEXERS=Animetosho
```
### Getting search terms
It is possible to override the entrypoint to generate search terms from the group and shows file:
```
docker-compose run --rm --entrypoint="./start-getsearchterms.sh" nzbhydrascheduler
```

## Parameters

| Parameter | Function |
| :----: | --- |
| `CRON=1 13 * * 1` | Run the download each Monday 13:01 |
| `SEARCHFREQUENCY=72` | The minimum delay between two search runs in hours. |
| `LOGLEVEL=debug` | Set log level for the application. Possible values are debug, info, warn, err |
| `SEARCHTERMSFILE=/config/searchterms` | Path to a file containing search terms to look for. Each line should contain a search term. |
| `SHOWSFILE=/config/shows` | Path to a file containing shows to generate search terms for. Only used when manually specifiying the container entry point. Each line should contain a search term. |
| `GROUPSFILE=/config/groups` | Path to a file containing release groups to generate search terms for. Only used when manually specifiying the container entry point. Each line should contain a search term. |
| `NZBOUTPUTDIRECTORY=/output` | Directory to save found nzbs to. |
| `NZBHYDRAURI=nzbhydra:5076` | The url for nzbhydra.  |
| `NZBHYDRAAPIKEY=supersecretapikey` | The api key for the nzbhydra api. |
| `MINSIZE=1` | The minimum file size for found results in MB. |
| `MAXSIZE=900000` | The maximum file size for found results in MB. |
| `MAXAGE=7` | The maximum age for found results in days. |
| `CATEGORY=Anime` | The nzbhydra category to search in. |
| `INDEXERS=Animetosho` | The indexers to include in the search. |
| `REQUESTCOOLDOWN=30` | Cooldown between searches in seconds. |

## Last search date and max age.
After each successful search run a timestamp file containing the current date and time is created. If a search run finds a timestamp file it will automatically limit searches to only include results between the current time and the time in the timestamp file.
If the timestamp file and max age parameter overlap the shortertime frame will be used. 