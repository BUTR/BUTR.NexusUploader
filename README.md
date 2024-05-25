# BUTR's Nexus Mods File Uploader

> Because a real API will be ready ***Soon™***

## Introduction

This is a small command-line tool that can be used to upload mod files to an existing mod on Nexus Mods.

This is **very** unofficial and **very** unsupported. If you want to see an official and supported API for mod authors, open a topic on [the forums](https://forums.nexusmods.com/index.php?/forum/117-feedback-suggestions-and-questions/) or in `#site-help-feedback` on Discord to let Nexus know the demand.

This tool does not automate the web UI, as an [existing solution](https://github.com/BUTR/Bannerlord.NexusmodsUploader) already does a perfectly good job of this (👋 Aragas), but instead recreates the underlying requests to upload a file and add it to an existing mod. It also supports adding/updating changelogs, but this hasn't been tested much.

## Installation

You can either download the binary for your platform from the Releases section on the right, or install it using the `dotnet` CLI:

```bash
dotnet tool install -g BUTR.NexusUploader
# then run with
unex
```

If your project already has a local tools manifest, you can also install it locally:

```bash
dotnet tool install BUTR.NexusUploader
# then run with
dotnet unex
```

> By default, your mod's main version will be updated to your new file version, but you can skip this using the `--no-version-update` option. 

You can optionally upload your file as a replacement for an existing file, by providing the `PreviousFile` configuration key. Set it to a file ID to directly replace that file, or to `"auto"` to replace the highest-versioned Main File on your mod (this is both highly experimental and only available for published mods).

## Configuration and Usage

Since there's a lot of information required when updating mods, the recommended method is to create a configuration file for non-sensitive information and use environment variables just for the sensitive information. That being said, you can mix-and-match keys from the config file or environment variables (just prefix them with `UNEX_`) at will.

Sample `unex.yml`:

```yaml
Game: site
ModId: 163
FileName: Upload Test
FileDescription: |-
  Your file description should go here.

  You can include whatever you like in your description, it's added as-is.
```

> You can also use a JSON configuration file if you prefer, you heathen

The remaining two configuration keys are sensitive and should not be made public, but you need both of them (for hard-to-explain reasons): a valid API key and session cookies. Your API key should be in the `UNEX_APIKEY` environment variable. Cookies can be provided in two ways:

- If you have the raw Cookie header from a valid session, you can include the whole header in the `UNEX_COOKIES` variable
- If you have an exported `cookies.txt` file, you can include the relative path to the file in the `UNEX_COOKIES` variable (like `./cookies.txt`)
- If you have the `nexusmods_session` variable, include it in the `UNEX_COOKIES` variable
- The lifetime of the `nexusmods_session` value is a week, so you may need to update it regularly

> All relative paths will be parsed relative to the *current working directory*

Then, call the CLI to begin your upload:

```bash
#unex upload <mod-id> <path-to-file> -v <target-version>
unex upload 163 ./Your-Mod-File.zip -v 1.1.2
```

### Available Commands:

###### unex -h
```bat
USAGE:
    unex [OPTIONS] <COMMAND>

EXAMPLES:
    unex changelog <version> -c <changelog>
    unex upload <mod-id> <archive-file> -v <version>
    unex check -s <session-cookie> -k <api-key>
    unex refresh -s <session-cookie>

OPTIONS:
    -h, --help    Prints help information

COMMANDS:
    changelog <version>               Add a changelog entry for a specific mod version
    upload <mod-id> <archive-file>    Upload a mod
    check                             Check the validity on an API Key and/or Session Cookie
    refresh                           Refresh the session cookie
```
---

###### unex changelog -h
```bat
DESCRIPTION:
Add a changelog entry for a specific mod version

USAGE:
    unex changelog <version> [OPTIONS]

EXAMPLES:
    unex changelog <version> -c <changelog>

ARGUMENTS:
    <version>    The version of the mod to update the changelog for

OPTIONS:
    -h, --help                     Prints help information
    -k, --api-key                  The NexusMods API key. Available Environment Variable: UNEX_APIKEY
    -g, --game                     The NexusMods game name (domain) to upload the mod to. Can be found in the URL of the game page. Available Environment Variable: UNEX_GAME
    -m, --mod-id                   The NexusMods mod Id to update the changelog for. Available Environment Variable: UNEX_MODID
    -c, --changelog <CHANGELOG>    The changelog content to add. Available Environment Variable: UNEX_CHANGELOG
```
---

###### unex upload -h
```bat
DESCRIPTION:
Upload a mod

USAGE:
    unex upload <mod-id> <archive-file> [OPTIONS]

EXAMPLES:
    unex upload <mod-id> <archive-file> -v <version>

ARGUMENTS:
    <mod-id>          The NexusMods mod Id to upload the file to
    <archive-file>    Path to the mod archive file to upload

OPTIONS:
    -h, --help                                    Prints help information
    -k, --api-key                                 The NexusMods API key. Available Environment Variable: UNEX_APIKEY
    -g, --game                                    The NexusMods game name (domain) to upload the mod to. Can be found in the URL of the game page. Available Environment Variable: UNEX_GAME
    -f, --file-name                               Name for the file on NexusMods. Available Environment Variable: UNEX_FILENAME
    -v, --version <VALUE>                         Version for your uploaded file. May also update your main version. Available Environment Variable: UNEX_FILEVERSION
        --remove-download-with-manager [VALUE]    Removes the Download With Manager button. Available Environment Variable: UNEX_REMOVEDOWNLOADWITHMANAGER
        --no-version-update [VALUE]               Skips updating your mod's main version to match this file's version. Available Environment Variable: UNEX_SKIPMAINVERSIONUPDATE
        --set-main-vortex [VALUE]                 Sets this file as the main Vortex file (for the Download with Manager buttons). Available Environment Variable: UNEX_SETMAINVORTEXFILE
```
---

###### unex check -h
```bat
DESCRIPTION:
Check the validity on an API Key and/or Session Cookie

USAGE:
    unex check [OPTIONS]

EXAMPLES:
    unex check -s <session-cookie> -k <api-key>

OPTIONS:
    -h, --help              Prints help information
    -k, --api-key           The NexusMods API key. Available Environment Variable: UNEX_APIKEY
    -s, --session-cookie    Value of the 'nexusmods_session' cookie. Can be a file path or the raw cookie value. Available Environment Variable: UNEX_SESSION_COOKIE
```
---

###### unex refresh -h
```bat
DESCRIPTION:
Refresh the session cookie

USAGE:
    unex refresh [OPTIONS]

EXAMPLES:
    unex refresh -s <session-cookie>

OPTIONS:
    -h, --help                               Prints help information
    -s, --session-cookie <SESSION-COOKIE>    Value of the 'nexusmods_session' cookie. Can be a file path or the raw cookie value. Available Environment Variable: UNEX_SESSION_COOKIE
```
---
