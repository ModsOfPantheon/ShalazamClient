# ShalazamPlugin

## Purpose
This client listens for and uploads data to the Shalazam API to assist with data gathering of the world of Terminus.

## Functions
The client listens for the following operations
* A new NPC has been added to the scene
* A new World Item has been added to the scene (gatherables, crafting stations, etc)
* A new Player has entered the scene

## Disclaimer and anticheat
I believe that this client does not violate the terms and conditions of Pantheon: Rise of the Fallen.
From the current EULA (as of 7th Jan 2025):
```
(b) use cheats, exploits, automation software, bots, hacks, mods or any unauthorized third-party software,
code or other device designed to modify or interfere with the Game or Service, or without Visionary Realms’
express written consent, modify or cause to be modified any files that are a part of the Game or Service;
```
This plugin does not
* Automate any gameplay actions
* Interfere or change any gameplay
* Provide any additional information to the user
  * It does not draw anything on screen
  * It does not write anything besides error messages regarding the Shalazam API
* Modify any files, everything is applied at runtime

However, this plugin is to be used **at your own risk.** I do not accept any liability or reponsibility for any actions taken against your account for using this plugin.

## Setup

First, the prerequisites:
* Download and install the [.NET 6.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0). Choose the x64 version.
* Download the latest [Melonloader](https://melonwiki.xyz/#/)
* Run the installer, selecting Pantheon either from the list or by manually locating the game

Then install the plugin. Close Pantheon first — the mod file is locked while the game runs.

### Using the installer (recommended)
* Download the latest release zip and extract all of it somewhere
* Double-click `Install.cmd`

It finds your Pantheon installations (Steam and standalone launcher, live and PTR), shows
you exactly what it's going to change, and waits for you to confirm before touching
anything. If you have more than one installation it can do them all at once. It also
writes your API key into `MelonPreferences.cfg` for you.

Running it again lets you update or uninstall. To remove the mod, choose option 2.

If your game lives under `C:\Program Files`, right-click `Install.cmd` and choose
*Run as administrator*.

### By hand
* Extract the folder from releases in to the game directory
* In the UserData folder, edit `MelonPreferences.cfg` to contain your API key. A blank placeholder is present for you in the file.
* Run the game as normal
