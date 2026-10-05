# GoatMeter Global PvE APL reference

Status: **provisional evidence map**, not automation. GoatMeter remains passive and informational.
These are normalized priority skeletons for Global Season 1. Conditional actions fail closed unless prerequisite state is passively observed.

| Class | Normalized single-target priority skeleton | State still requiring proof |
|---|---|---|
| Templar | Judgment reaction > Punishment > Empyrean Lord's Punishment > sustained filler | verified Judgment trigger; specialization cooldown changes excluded |
| Gladiator | Rage Burst window > Overhead Slam > Upward Strike > Rending/Smashing > Keen/Rupture/Wrathful > filler | observed Rage/Overhead and chain casts |
| Assassin | crit Heart Gore > proven Insignia Explosion > Illusive Clone burst/Shadowstrike > Quick Slice chain > Savage chain > filler | exact insignia/build state and specialty resets |
| Ranger | Precision Deadshot > high-value cooldowns > Snipe/Rapid Fire/Spiral Arrow > filler | observed Precision; full charge and mark refresh remain provisional |
| Sorcerer | opener > situational Hellfire burst > Fire Mark/Blaze > Ice Chain/Cold Wave > filler | charge level and specialty cooldown reductions are not inferred |
| Spiritmaster | pre-summon buffs > Ancient Spirit > Corrode > Four Elements/Fusion > post-summon Dimensional Control > filler | observed element/summon state; specialty snapshot details excluded |
| Cleric | Earth Punishment + Chain of Torment > ready Condemnation > damage window > filler; heal when required | reset/guaranteed-crit specialty is build-dependent |
| Chanter | ranged setup > Dark Crush > major burst > Impactful Crush > Onslaught/Resonance/Bolt > filler; support/heal when required | vulnerability and support/heal need must be observed |

## Evidence policy

1. Executable changes require evidence that applies to the **current Global release build**: release-client extraction/database, Global patch notes, or corroborated live Global behavior.
2. Global playtest/client evidence may support investigation, but does not override a differing current release build.
3. Use current Global guides as corroboration for ordering, not proof of hidden state.
4. KR/TW evidence is research/corroboration only and cannot by itself create executable Global logic.
5. Never infer a proc, charge completion, stack count, specialization, target debuff, or cooldown modifier from generic activity.
6. Every executable conditional priority needs present/absent regression coverage.
7. If current Global compatibility cannot be established, keep the rule provisional/fail-closed or remove it; coverage score must not be used to justify uncertain behavior.

## Coverage meaning

Optimization coverage estimates evidence-backed decision coverage, not percent of theoretical maximum DPS. A class approaches 90% only when major priorities, conditional chains, readiness, important state, build gates, and regressions are represented.
