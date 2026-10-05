# Rotation Research Corpus

Last updated: 2026-10-05

This document is the source-ranked research layer for GoatMeter Rotation Lab. It is not a finished rotation list and it is not permission to automate the game. The assistant remains recommendation-only.

## Evidence hierarchy

1. Clean recent GoatMeter combat observations with confirmed class identity.
2. Repeated outcome advantages across multiple players/encounters in those logs.
3. External packet-derived combat data and leaderboards.
4. Current official balance/patch information.
5. Agreement among multiple current Global guides.
6. KR/TW high-end guides that still match the Global Season 1 ruleset.
7. Single community guide/opinion.
8. Inference.

A rule should not become `Validated` because one guide says so.

## Datasets currently available

### GoatMeter historical captures

- 61 stored `combat-*.log` files
- about 66.9 MB total
- October 2–4, 2026
- first pass prioritized the 10 newest logs
- raw logs stay private and are never committed

### A2Tools public combat database

The public A2Tools site exposes packet-recomputed boss logs, class statistics and leaderboards.

Current snapshot observed on 2026-10-05:

- 1,358 public shared logs
- class-stat page currently summarizes 473 de-duplicated logs
- 2,237 player samples in those class stats
- the stats page excludes players who effectively did not participate

Per-class sample counts in the current class-stat view:

| Class | Samples |
|---|---:|
| Chanter | 328 |
| Assassin | 305 |
| Sorcerer | 163 |
| Templar | 310 |
| Spiritmaster | 198 |
| Ranger | 348 |
| Gladiator | 243 |
| Cleric | 342 |

This dataset is especially useful because its fight numbers are re-derived from uploaded packet evidence rather than typed in by players.

## Current candidate PvE priorities

These are research hypotheses to test against our own logs and holdout fights. They are not yet executable profiles.

### Gladiator

Strong external agreement:
- Rage Burst is used to unlock/enable the high-value Overhead Slam window.
- Ruinous Blow is a major recurring cooldown and combat-state setup.
- Rending Blow and its follow-up/related heavy strikes form the sustained core.
- Buff maintenance such as Lunge Stance / Zikel's Blessing belongs outside a simple filler loop.

External packet-derived class stats strongly feature:
- Smashing Blow
- Rending Blow
- Ruinous Blow
- Overhead Slam
- Upward Strike

Research direction:
- model Overhead Slam as a conditional high-priority action rather than a fixed sequence step;
- measure whether delaying lower-value filler for Rage Burst / Overhead windows improves normalized damage.

### Templar

Strong external agreement:
- Punishment is a central damage cooldown and should interact with cooldown-reduction mechanics.
- Judgment has very high priority when its shield-skill relationship is active.
- Shield Smite / Warding Strike / Doom Shield form a state-dependent shield line.
- Annihilate / Debilitating Smash / Pummel form the other sustained line.
- Vicious Strike/Pummel behavior can affect cooldown availability and resource flow.

External packet-derived stats strongly feature:
- Punishment
- Punishing Strike
- Pummel
- Judgment
- Desperate Strike
- Annihilate
- Decisive Strike
- Vicious Strike
- Shield Smite
- Punishing Benediction

Research direction:
- infer Punishment availability from observed cooldown-reset events;
- prioritize Judgment based on actual proc/state evidence instead of blindly alternating attack lines.

### Assassin

Strong external agreement:
- stay behind the boss whenever possible;
- Insignia Explosion is a primary spender and should fire when the state is favorable;
- Heart Gore is highly reactive to crit/proc state;
- Quick Slice is a sustained core action and interacts with cooldown/resource flow;
- Illusive Clone / Swift Contract / Savage Fang create a major burst window.

External packet-derived stats show near-universal use of:
- Heart Gore
- Insignia Explosion
- Quick Slice / Breaking Slice / Swift Slice
- Savage Roar / Savage Back Kick / Savage Smash
- Ambush Stance

Research direction:
- make rear-position confidence part of the recommendation score;
- recognize burst windows and heavily raise Heart Gore/Insignia priorities inside them;
- compare early vs delayed Insignia Explosion timing against normalized follow-up damage.

### Ranger

Strong external agreement:
- maintain Marking Shot / Precision rather than mindlessly spamming it;
- fully charged Deadshot is an important manual burst action;
- Gale Arrow -> Drill Dart -> Tempest Shot is a recurring sustained package;
- Snare/Griffon/Burst interactions depend on Slow/Root state;
- Snipe/basic sustained firing fills otherwise empty time;
- Vaizel's Authority and Bow of Blessing define burst windows.

External packet-derived stats strongly feature:
- Tempest Shot
- Deadshot
- Drill Dart
- Burst Arrow
- Marking Shot
- Spiral Arrow
- Rapid Fire

Research direction:
- track Marking Shot remaining duration;
- do not recommend refresh until the buff is near expiry;
- score charged Deadshot around buff windows and encounter movement risk.

### Sorcerer

Strong external agreement:
- Element Enhancement and Wish of Concentration define important buff/cooldown windows.
- Bittercold Wind / Delayed Explosion and field skills create debuff/burst setup.
- Blaze should exploit Fire Mark state.
- Firestorm is a high-frequency cooldown.
- Hellfire is a high-value charged hit and should not blindly delay more important cooldowns.
- Winter's Shackles / other damage-amplification windows should influence charged-skill timing.

External packet-derived stats strongly feature:
- Hellfire
- Blaze
- Bittercold Wind
- Firestorm
- Fire Wall
- Pyroclasm
- Flame Arrow
- Burst

Research direction:
- explicitly model Fire Mark and damage-amplification debuff state;
- score Hellfire charge depth versus opportunity cost;
- learn whether field overlap and cooldown-reset timing produce measurable gains in real fights.

### Spiritmaster

Strong external agreement:
- spirit summons snapshot buffs at summon time;
- maintain the Earth Tremor/basic-attack attack buff at five stacks;
- delaying a summon briefly to restore the full stack can be a gain;
- Elemental Fusion is a major priority;
- Jointstrike: Corrode should maintain very high uptime;
- Fire Spirit generally benefits from being the last persistent base spirit in the rotation.

External packet-derived stats strongly feature:
- Combustion
- Elemental Fusion
- Summon: Fire Spirit
- Dimensional Control
- Earth/Water Spirit
- Corrode
- Cold Shock
- Spirit's Descent

Research direction:
- snapshot-aware recommendations are mandatory;
- never recommend a major summon when the required buff snapshot is missing unless the model proves the delay is worse;
- separate summon casts, summon damage, and player direct casts when learning transitions.

### Cleric

Strong external agreement:
- Earth Punishment can enable guaranteed-crit behavior for Condemnation in the relevant setup;
- Condemnation is a central damage action;
- Divine Aura and Debilitating Mark are high-value recurring actions;
- Judgment Thunder is a sustained filler;
- Bolt is a charged burst action;
- Prayer of Amplification defines a major damage window;
- stagger-only skills must be gated on boss state.

External packet-derived stats strongly feature:
- Condemnation
- Judgment Thunder
- Divine Punishment
- Divine Aura
- Empyrean Lord's Grace
- Bolt
- Noble Aura
- Debilitating Mark

Recent Korean high-end guidance also emphasizes reducing macro-line bloat because too many entries can lower effective hit frequency.

Research direction:
- test Condemnation frequency versus other filler under different specializations;
- model healing/support urgency as an override above personal DPS;
- keep stagger-only actions impossible outside valid boss state.

### Chanter

Strong external agreement:
- Dark Crush is the central conditional damage window;
- Spinning Strike and Impactful Crush can open that window;
- Incandescent Blow fills when the openers are unavailable;
- Onslaught should be continuously woven for sustain/resource flow;
- Marchutan's Wrath is best treated as a reserve/manual window opener rather than indiscriminate filler;
- support/defensive duties can override personal DPS.

External packet-derived stats strongly feature:
- Bursting Blow
- Incandescent Blow
- Spinning Strike
- Piercing Strike
- Dark Crush
- Bolt Crush
- Resonance Crush
- Onslaught
- Impactful Crush

Research direction:
- make Dark Crush eligibility a first-class state;
- model party support requirements separately from DPS priority;
- compare opener selection based on distance/movement and cooldown state.

## Sources being monitored

- A2Tools public logs, class stats and leaderboards: https://a2tools.app/
- Couga54 Global Season 1 class guides: https://couga54.github.io/aion2-guides/en/
- Spiritmaster Global guide: https://aion2sm.com/global/
- AION 2 Inven class boards and current Korean PvE optimization posts: https://www.inven.co.kr/board/aion2/
- DCInside AION 2 high-end class guides where corroborated by other evidence
- VortexGaming summaries of current Korean build/rotation videos
- questlog.gg / current Global client-derived skill data when available
- NCSOFT official balance announcements
- public GitHub AION 2 projects used only for factual corroboration and independent research

## Validation rule

For a class/build profile to move from `Unvalidated` -> `Provisional`:

- core mechanics must be confirmed from current data;
- at least two independent evidence families should agree;
- our own logs must contain enough clean examples to replay the rule.

For `Provisional` -> `Validated`:

- holdout fights must show the rule predicts strong choices without being trained on those fights;
- no known current patch contradiction;
- recommendation confidence must fail closed when required state is missing;
- regression fixtures must cover the important proc/cooldown/buff branches.

## Safety boundary

All of this research is used only to choose what GoatMeter displays as the next recommended action. The project must never use this information to send keyboard/mouse input, inject into the game, modify memory, modify packets, or bypass anti-cheat.
