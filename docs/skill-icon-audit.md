# Skill icon audit

## October 5 live-beta follow-up

The user prioritized recognizable skill icons after the level-45 Templar test. This lab batch embeds 77 explicitly mapped display icons across all eight classes. Global skill IDs and image links were checked against gaming.tools Global client data 2.0.5.0 (October 5). Existing sanitized passive observations corroborate the recommended English skill vocabulary; they do not grant cooldown or loadout eligibility.

Icons are local 64x64 PNG resources converted from the exact database image links. The application makes no runtime icon downloads and does not read game files, inject input, or change capture behavior. Every mapping has class/skill identity regressions and every packaged asset is decoded in the WPF smoke check. Unknown class, unsupported names, missing assets and class changes use the explicit question-mark fallback.

## Artwork provenance

Original AION 2 artwork remains NCSOFT intellectual property. Source URLs and converted-file hashes are in `skill-icon-manifest.json`. Inclusion is scoped to the user-requested isolated beta display; no publisher endorsement or blanket redistribution license is asserted. These are display identity mappings, not a claim of licensed artwork. The previous zero-asset audit is superseded for this explicitly requested beta integration.

## Coverage

| Class | Embedded mappings |
|---|---:|
| Templar | 9 |
| Chanter | 11 |
| Assassin | 10 |
| Gladiator | 10 |
| Ranger | 9 |
| Sorcerer | 12 |
| Cleric | 7 |
| Spiritmaster | 9 |

Templar Threatening Blow and Gladiator Upward Strike have corroborated identities but their exact source artwork URLs returned 404; they retain fallbacks. Chanter Healing Burst and Spiritmaster Disenchant still lack reconciled Global identities. No guessed aliases or unrelated icons are supplied. Exploit Weakness is a passive and is removed from pressable recommendations. All eight classes now have embedded mappings; full recommendation coverage remains incomplete.

The next skill uses a 48px tile. Each alternative has its own 32px tile and name; alternatives are current choices, not a forecast sequence. Icons do not establish learned level, equipped stigma, cooldown readiness or optimal DPS.
