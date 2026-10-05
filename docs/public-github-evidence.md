# Public GitHub Evidence Sources

This document tracks public AION 2 sources that may help corroborate Rotation Lab decisions. Public availability does **not** mean raw code or datasets should be copied into GoatMeter. The lab uses these sources as external evidence and independently implements its own logic.

## Raw public combat-log availability

A GitHub-wide search found very few actual AION 2 raw combat logs committed to public repositories.

A2Tools-DPS-Meter is particularly informative: its test suite references real replay captures such as `packets_20260815_183732.txt`, `packets_20261001_040431.txt`, `packets_20261001_041517.txt`, and `packets_20261003_005123.txt`, but the repository's `.gitignore` explicitly excludes `packets_*.txt` and `*.log`. The tests load those captures from local paths supplied through environment variables. Therefore the underlying captures are not available from the public repository.

This is good privacy practice and means Rotation Lab should not assume public GitHub will provide a large raw-log corpus.

## Useful public evidence discovered

### taengu/A2Tools-DPS-Meter

Public, GPL-3.0. Provides independent AION 2 packet parsing, replay-test architecture, skill localization data, class/identity replay tests, summon handling, DOT data, and fight-analysis behavior.

Use in Rotation Lab:
- corroborate protocol/skill identity assumptions;
- compare conceptual replay-validation methods;
- validate that certain event/state types are observable;
- do not copy GPL source into GoatMeter.

### nousx/aion2-dps-meter

Public AION 2 DPS meter with independent skill-code mappings, parser logic, class detection, and skill analysis.

Use in Rotation Lab:
- cross-check skill IDs/names and class attribution;
- compare observed skill families against our own logs;
- use only factual corroboration unless licensing permits direct reuse.

### Kuroukihime/AIon2-Dps-Meter

Public GPL-3.0 network-based Global AION 2 meter. Contains skill data, buff processing, cooldown processing, packet parsing, history/session support, and target tracking.

Use in Rotation Lab:
- corroborate which cooldown/buff/state concepts are technically observable;
- cross-check skill identifiers and cooldown/event semantics;
- do not copy GPL implementation into GoatMeter.

### jonboy648/aion2-companion

Public class-research repository with mechanics, skill data, chains, specialties, and community rotation research for all eight classes.

Especially useful for our thinner log classes:
- Gladiator: multiple boss/AoE priorities involving Lunge Stance, Zikel's Blessing, Ruinous Blow, Rage Burst, Overhead Slam, Rending Blow.
- Ranger: multiple sources converge around Marking Shot, Deadshot, Snipe-related chains, Gale Arrow, Drill Dart, Tempest Shot and burst-window buffs.
- Chanter: multiple sources converge around Dark Crush, Spinning Strike, Incandescent/Bursting Blow, Impactful Crush and Onslaught.
- Templar: multiple sources emphasize Punishment, Judgment/Shield Smite lines, Pummel/Vicious Strike lines, and build-dependent hand-cast cooldowns.

Use in Rotation Lab:
- treat these as opinion/research evidence, not ground truth;
- cross-check every proposed priority against our historical combat observations;
- favor rules that survive disagreement between multiple public sources and our own holdout data.

## Evidence hierarchy

Rotation Lab should rank evidence in this order:

1. Clean, recent, confirmed-class GoatMeter observations from the user's own logs.
2. Repeated outcome advantages across multiple players/encounters in those logs.
3. Current official skill/balance information.
4. Agreement among multiple independent public AION 2 projects/guides.
5. Single community rotation or guide.
6. Unverified inference.

No item at levels 4-6 should make a profile actionable by itself.

## Privacy / licensing rules

- Never commit third-party raw player captures or personal identifiers.
- Never copy third-party GPL code into GoatMeter.
- Do not republish unlicensed third-party datasets.
- Record only independently derived aggregate findings and source provenance.
- Keep uncertain evidence marked provisional.
