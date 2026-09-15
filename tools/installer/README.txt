ShalazamPlugin - installer
==========================

What this is
------------
ShalazamPlugin is a MelonLoader mod for Pantheon: Rise of the Fallen. It watches the
world as you play and uploads what it sees (NPCs, gatherables, loot, abilities) to the
community database at https://shalazam.info.

It does not automate anything, draw anything on screen, or modify any game file.


Before you start
----------------
1. Install the .NET 6.0 x64 runtime:
   https://dotnet.microsoft.com/en-us/download/dotnet/6.0
2. Install MelonLoader and point it at Pantheon.exe:
   https://melonwiki.xyz/
3. Get your API key from your profile on https://shalazam.info.

The installer will tell you if MelonLoader is missing.


Installing
----------
Extract this whole zip somewhere, then double-click Install.cmd.

All the installer needs is ShalazamPlugin.dll sitting in the same folder as install.ps1,
so it works just as well on a folder you put together yourself.

It will:
  - find your Pantheon installation(s), including PTR
  - show you exactly what it plans to change, and wait for you to confirm
  - copy ShalazamPlugin.dll into <game>\Mods\
  - write your API key into <game>\UserData\MelonPreferences.cfg

If you have more than one installation (for example live and PTR) it lists them all and
can apply to all of them at once, or you can pick individually.

Nothing outside Mods\ and UserData\ is written to. No game file is modified.

Close Pantheon before running the installer - the mod file is locked while the game runs.

If your game is installed under C:\Program Files, right-click Install.cmd and choose
"Run as administrator".


Uninstalling
------------
Run Install.cmd again and choose option 2. It deletes the mod and, if you ask it to,
removes your Shalazam settings from MelonPreferences.cfg. MelonLoader and any other mods
you have are left alone.


Command line
------------
For scripted use:

  powershell -ExecutionPolicy Bypass -File install.ps1 -GamePath "D:\Pantheon\App" -ApiKey <key> -Quiet
  powershell -ExecutionPolicy Bypass -File install.ps1 -Uninstall

  -GamePath   skip detection and use this folder (the one containing Pantheon.exe)
  -ApiKey     API key to write into MelonPreferences.cfg
  -Uninstall  remove the mod
  -Quiet      no prompts; assumes yes to the confirmation


Troubleshooting
---------------
"No Pantheon installation found" - enter the path by hand when asked. It's the folder
containing Pantheon.exe; for the standalone launcher that's usually <install>\App.

The mod isn't loading - check the MelonLoader console window for a line mentioning
ShalazamPlugin, and confirm the .NET 6.0 x64 runtime is installed.

After a game patch - the launcher may remove unknown files, so if the mod stops loading
after an update, just run the installer again.


Source and issues: https://github.com/ModsOfPantheon/ShalazamClient
