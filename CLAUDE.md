# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A [MelonLoader](https://melonwiki.xyz/) mod (`ShalazamPlugin`) for the Unity/Il2Cpp game *Pantheon: Rise of the Fallen*. It observes game state at runtime (NPCs, world items, loot drops, abilities, masteries) and uploads it over a websocket to the [Shalazam](https://shalazam.info) community database. It does not automate gameplay, draw overlays, or modify game files — see `README.md` for the full anti-cheat/EULA rationale, which should stay true of any change made here.

## Build

Requires .NET 6.0 SDK and a local install of the game (the build references the game's MelonLoader/Il2Cpp interop DLLs directly, not NuGet packages).

```
dotnet build ShalazamPlugin.sln
```

- `ShalazamPlugin.csproj` auto-detects the Steam install path (checks `C:\Program Files (x86)\Steam...`, `C:\SteamLibrary...`, `D:\SteamLibrary...`). If your install is elsewhere, add a `Directory.Build.props` next to the `.sln` setting `<GamePath>`.
- Post-build copies the built DLL into `$(GamePath)\Mods`, so a successful local build drops it straight into the game's mod folder.
- Bump `ModMain.PluginVersion` (`ShalazamPlugin/ModMain.cs`) when cutting a release; it's sent to the server as `X-Plugin-Version`.

There is no test suite and no lint/CI config in this repo currently — verify changes by building and running the mod in-game.

### Exploring game types (`tools/reflect`)

The game's Il2Cpp interop types (`Il2Cpp`, `Il2CppPantheonPersist`, `Il2CppViNL`, etc.) aren't documented anywhere; `tools/reflect` is a standalone console app for inspecting them directly from the installed game's assemblies:

```
cd tools/reflect
dotnet run EntityNpcGameObject
dotnet run EntityNpcGameObject --depth=3
dotnet run EntityNpcGameObject --refs      # find types that reference a given type
dotnet run --html                          # browsable HTML dump of everything
```

Game path auto-detects the same way as the main project; override with the `PANTHEON_DIR` env var. Use this before writing new hooks/extensions against an unfamiliar Il2Cpp type instead of guessing at field/method names.

## Architecture

Data flows in one direction: **game event → Harmony hook → `EntityManager`/cache → `Extensions` mapper → `SDK` client → websocket**.

- **`Hooks/`** — Harmony patches (`[HarmonyPatch]` classes with `Prefix`/`Postfix` methods) on game methods (`NetworkStart`/`NetworkStop`, loot generation, chat, vendor windows, etc.). Hooks should stay thin: pull data off the `__instance`/args and hand off to `EntityManager` or a cache class. Don't put filtering/business logic in hook classes.
- **`EntityManager`** — central dispatcher for entity lifecycle (players, NPCs, world items). Owns dedup (`NetworkStart` fires twice for some objects), filtering (name blacklist, distance thresholds, profession checks, "is this actually a player summon" heuristics), and deciding which `ShalazamClient.Post*` call to make.
- **`AbilityCache` / `ItemCache` / `LootCache`** — dedup/track state so the same ability, item, or loot drop isn't reported to the server repeatedly (e.g. `LootCache` tracks seen `NetworkId`s per skinned/non-skinned kill).
- **`Globals`** — static mutable state shared across hooks and the update loop (local player reference, currently tracked monster, PTR flag, UI-init flag). Kept intentionally minimal; add to it only for state that genuinely needs to be read from multiple unrelated hook sites.
- **`Extensions/`** — `ToXPayload()`-style extension methods that map raw Il2Cpp game objects (`EntityNpcGameObject`, `NetworkWorldItem`, `AbilityData`, etc.) into the SDK's plain-C# payload models. This is where game-type knowledge lives; keep it out of `SDK/`.
- **`SDK/`** — transport layer, decoupled from game types:
  - `IShalazamClient` / `ShalazamWebsocketClient` — single persistent websocket to `wss://shalazam.info/api/v1/client`, with auto-reconnect and a `ConcurrentDictionary` tracking in-flight requests by id.
  - Every outgoing `Post*` method checks the user's granted roles (`Permissions.cs`, populated from the server's `"me"` message) before sending — a missing permission is a silent no-op, not an error.
  - `SDK/Models/` — plain payload/body records; `SDK/Models/Websockets/` — the envelope types actually sent/received over the socket. Outgoing JSON uses snake_case (`JsonSnakeCaseNamingPolicy`) and omits nulls.
- **`ModMain`** (`MelonMod` entry point) — reads MelonPreferences (API key, `MinimumTrackingDistance`), constructs the `ShalazamClient`, and runs the per-frame `OnUpdate` poll used for continuous monster-position tracking (`TrackOffensiveTarget`/`StopTrackingOffensiveTarget`, bound to in-game commands elsewhere).

### Conventions to follow

- New game-observation logic: add a Harmony patch in `Hooks/`, keep it thin, push the actual logic into `EntityManager` or a dedicated cache.
- New outgoing data: add a payload/body pair under `SDK/Models/` (+ `Websockets/` envelope if new), a mapping extension in `Extensions/`, a permission-gated `Post*` method on `IShalazamClient`/`ShalazamWebsocketClient`, and a `Permissions` constant if it's a new permission.
- Use `MelonLogger` (`.Msg`/`.Warning`/`.Error`), never `Console`/`Debug`, so output shows up in the MelonLoader console/log.
- Nullable reference types are enabled solution-wide — respect existing `?`/null-check patterns rather than suppressing warnings.
