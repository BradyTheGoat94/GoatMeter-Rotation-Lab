# Skill icon audit

Rotation Lab skill icons fail closed: an unverified skill icon must render as an explicit fallback rather than a guessed image.

Target classes: Gladiator, Templar, Assassin, Ranger, Sorcerer, Spiritmaster, Cleric, Chanter.

## Promotion rule

Promote an icon mapping only after the Global skill identity is corroborated and the local asset has acceptable reuse provenance. Add a regression assertion for every promoted mapping. External database images are references only and are not copied into this repository without permission.

A mapping needs all of:
1. Global English skill identity corroborated by at least two current Global-oriented data sources, with client/build scope recorded when available.
2. Skill ID corroborated when a source exposes one.
3. A local icon asset whose license/provenance permits inclusion in GoatMeter.
4. A regression assertion covering skill -> icon and an overlay assertion covering Next Skill / planned queue rendering.

If any item is missing, the UI must label the icon as unverified and use the GoatMeter fallback rather than an unrelated or guessed skill image.

## Current corroboration sources (2026-10-05)

- A2Tools / gaming.tools Global database: reports Global client version 1.0.21.0, updated 2026-09-30. Useful as the primary current Global identity/build cross-check.
- MetaBot Global skills/classes: reports data extracted from the Global client and exposes the eight launch classes and Global skill names/cooldowns.
- Wikily skill index: exposes 288 entries (36 per launch class) and is useful as a second name/class cross-check.
- AtreiaKompass Global skill table: dated 2026-10-03 with Global Early Access data and exposes English names plus many skill IDs.
- DBAion2 Global class pages: game-client-derived skill lists useful as an additional corroboration source.

These sites are corroboration only. Their image assets are not imported unless reuse permission is independently established.

## Coverage

| Class | Global identity source coverage | Permitted local skill-icon assets | Promoted mappings |
|---|---|---:|---:|
| Gladiator | >=2 current sources | none established | 0 |
| Templar | >=2 current sources | none established | 0 |
| Assassin | >=2 current sources | none established | 0 |
| Ranger | >=2 current sources | none established | 0 |
| Sorcerer | >=2 current sources | none established | 0 |
| Spiritmaster | >=2 current sources | none established | 0 |
| Cleric | >=2 current sources | none established | 0 |
| Chanter | >=2 current sources | none established | 0 |

The zero promoted count is intentional: correct identity is not sufficient to copy third-party artwork. Accuracy and provenance both have to pass before an icon enters the executable.

## Next implementation step

Add a lab-owned skill icon resolver whose default result is explicitly unverified, wire that result into both Next Skill and each planned-skill entry, then add smoke tests proving unknown skills cannot display a verified icon. Populate the registry only as permitted local assets become available.
