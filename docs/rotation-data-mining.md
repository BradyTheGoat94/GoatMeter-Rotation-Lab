# Rotation Log Mining Baseline

This document records the first historical-data pass for the passive Rotation Lab. It does **not** define an optimal rotation yet.

## Privacy and safety

Raw combat logs are not committed to this repository. Aggregate mining output excludes player names, IP addresses, raw packets, and raw log lines. The miner is an offline analysis tool only and has no game-control capability.

## Available historical data

The ChatGPT file library currently contains 61 `combat-*.log` captures totaling about 66.9 MB, spanning October 2 through October 4, 2026.

The first pass intentionally analyzed the 10 newest captures first:

- 46,211,738 bytes analyzed
- 19,047 direct-damage events parsed
- 12,190 direct-damage events had confirmed class evidence
- all eight classes were represented

This is deliberately a conservative first pass. Older captures were produced while identity/class/parser behavior was still being improved, so they should be incorporated only with quality weighting.

## Initial confirmed-class coverage

| Class | Confirmed damage events | Filtered observed action groups |
| --- | ---: | ---: |
| Gladiator | 188 | 137 |
| Templar | 3,109 | 2,066 |
| Assassin | 2,133 | 1,556 |
| Ranger | 255 | 185 |
| Sorcerer | 1,512 | 1,273 |
| Spiritmaster | 2,255 | 1,993 |
| Cleric | 2,236 | 1,758 |
| Chanter | 502 | 259 |

Coverage is not equal. Gladiator, Ranger, and Chanter should remain lower-confidence until more clean samples are incorporated.

## Example observed patterns

These are repeated **observations**, not hard-coded rotations.

- Templar strongly repeats `Pummel ↔ Punishing Benediction`, with `Punishing Strike`, `Vicious Strike`, `Decisive Strike`, and `Desperate Strike` frequently appearing around that core.
- Assassin repeatedly clusters `Ambush Stance`, `Savage Back Kick`, `Exploit Weakness`, `Determination`, and `Heart Gore`.
- Ranger observations frequently return to `Tempest Shot` from `Concentrated Fire`, `Support Fire`, `Melee Fire`, and other actions.
- Sorcerer repeatedly shows `Vitality Evaporation → Grace of Enhancement`, `Burst → Pyroclasm`, and `Cold Storm → Fire Wall` patterns.
- Spiritmaster repeatedly clusters `Combustion`, `Consecutive Countercurrent`, `Spirit's Descent`, and other spirit actions, but several unresolved/internal skill labels still need cleanup.
- Cleric frequently alternates offensive actions around `Empyrean Lord's Grace`, including `Judgment Thunder`, `Divine Punishment`, and `Earth's Retribution`.
- Chanter most clearly clusters `Incandescent Blow`, `Bursting Blow`, `Onslaught`, and `Resonance Crush`.
- Gladiator has a smaller sample, with `Rending Blow → Smashing Blow` currently the strongest repeated transition.

## Quality controls

The miner uses confirmed identity mappings for class-specific counts. To reduce cross-class contamination, a skill is used in class-specific transition analysis only when at least 90% of its confirmed observations map to one class and at least three observations exist.

Consecutive same-skill direct-damage hits within 0.4 seconds are collapsed into one **observed action group** to reduce multi-hit inflation. This still does not prove that every group equals one player keypress or cast.

## How this feeds the recommendation engine

Historical observations will be used to:

1. identify likely skill families and common follow-up choices;
2. estimate real-world timing/repeat behavior;
3. identify candidate priority rules;
4. build class-specific simulated regression fixtures;
5. compare public rotation theory against actual observed combat behavior.

No historical pattern becomes an actionable profile solely because it is common. The Rotation Engine remains fail-closed: profiles stay `Unvalidated` or `Provisional` until their rules are corroborated and tested.
