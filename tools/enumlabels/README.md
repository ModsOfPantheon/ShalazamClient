# enumlabels

Generates `enum-labels.json` — a map of **`EntityMultiplierType` enum name → human-readable display
value**, i.e. the strings the game shows for item multiplier bonuses like `CritDamagePhysical` →
"Physical Crit Damage".

## Usage

```
cd tools/enumlabels
dotnet run                    # writes ./enum-labels.json
dotnet run --out=path.json    # custom output path
```

Game path auto-detects like `tools/reflect`; override with `PANTHEON_DIR`. The metadata file
defaults to `<game>/Pantheon_Data/il2cpp_data/Metadata/global-metadata.dat`; override with
`PANTHEON_METADATA`.

## How it works (and when to update it)

`EntityMultiplierType`'s player-facing labels only exist as a curated UI string block inside
`global-metadata.dat` — there is no enum→string method in code. So:

- **Enum names/values** are read straight from the game assemblies, so new enum values are picked up
  automatically on a re-run.
- **Labels**: the per-school ones are derived by naming rule; the ~35 irregular ones come from the
  `Scalars` table in `Program.cs`. Every produced label is then **validated against the live
  metadata**, and any that no longer appears is printed as a warning — that's your signal a game patch
  changed the wording and the `Scalars` table needs editing. Values with no player-facing label
  (internal/unused ones) are emitted as `null`.

Other enums we ship (`StatType`, `WeaponType`, `DamageType`, `EntityRace`, …) were considered but
left out: their names are already display-ready (or differ only by cosmetic spacing) and, unlike
`EntityMultiplierType`, have no authoritative label block to validate against — so a lookup table
would just be unverifiable guesses.
