# Rotation Lab beta status

## Healing Touch reconciliation

- 80 embedded icons now cover all eight classes. Healing Touch is the verified current Global Chanter stigma (18170000); the unsupported Healing Burst placeholder is retired rather than aliased as a historical rename.
- Healing Touch requires explicit healing need, proven current-session local stigma use and an observed 30s base cooldown. Party HP/healing need decoding is unavailable, so the assistant cannot manufacture an automatic healing recommendation from a prior cast.
- Only Disenchant remains an unresolved recommendation name. Global Aion2 PB searches returned no exact entry; original AION artwork is not substituted. All profiles remain provisional.

## Aion2 PB source reconciliation

- 79 icons now embed exact mapped artwork across all eight classes. Threatening Blow (12440000) and Upward Strike (11440000) were verified on Aion2 PB with the visible Global selector, including the base/variant Upward identity. Upward uses ICON_TE_SKILL_016.webp as actually linked by the source; icon filenames cannot be inferred from class or ID.
- This batch changes display mappings only. No KR/TW mechanics, skill level, specialization, cooldown or loadout eligibility is inferred from artwork.
- Healing Burst and Disenchant remain unresolved Global names with explicit fallbacks. They are not given original AION/Classic artwork as substitutes. Full icon coverage remains incomplete.

## All-class icon batch

- 77 embedded display mappings now cover all eight classes. Every primary and alternative suggestion uses the existing local resource renderer; runtime icon networking remains absent.
- Upward Strike and Threatening Blow retain explicit fallbacks: their identity is corroborated, but their exact source artwork was unavailable (404). Healing Burst and Disenchant still require current Global identity reconciliation. Icon coverage is not complete and does not measure optimal DPS.
- Every new asset is decoded by the existing WPF resource smoke check; catalog identity regressions apply to all mappings. All mechanics remain provisional and live gameplay validation is required.

## Assassin follow-up

- Ten Assassin icons bring the embedded total to 30 across Templar, Chanter and Assassin.
- Exploit Weakness is a Global passive effect, so it is no longer suggested as a skill to press.
- Savage Fang requires observed local use proving the stigma loadout and its 60s base cooldown. Party casts and zone-reset history cannot grant readiness.
- WPF checks exercise long skill names, both option icons and first-use guidance in every preset. The package includes ASSISTANT-PREVIEW.png.

## Live usability follow-up

- Twenty embedded Templar/Chanter skill icons now appear on the next suggestion and each individual alternative. Other classes and unresolved names retain explicit fallbacks.
- The next-skill tile is larger and long names wrap. Observation confidence is no longer shown as a prominent percentage that could be confused with DPS optimization.
- Unknown first-use cooldowns are listed with manual observation guidance. A filler-only Templar cold start explicitly explains why Pummel alone cannot open Judgment.
- Chanter Bursting Blow now requires an unconsumed 3s observed Incandescent Blow chain; it cannot appear as generic filler.
- Chanter Fracturing Blow requires proven current-session stigma use and its observed 45s base cooldown; specialization reductions are excluded.
- Icon identity and learning guidance cannot grant skill eligibility. All profiles remain provisional. Templar first-use readiness remains unknown until a manual observation; this batch explains that limitation rather than manufacturing readiness.

## Improvements in this development run

- One observed opener now grants one pending continuation across 18 existing short chains in six class profiles; an observed Judgment consumes its shield opportunity.
- Exact known selfInfo class takes precedence over conflicting later combat/non-local identity class labels; fresh confirmed selfInfo may prove a class change and reset history.
- Chanter Dark Crush now supports observed Spinning Strike and Impactful Crush using the current Global two-second availability window; base cooldown remains enforced.
- All eight classes bind rotation state to explicit selfInfo local-player proof. Confirmed party names do not prove local identity.
- Capture reconnect, zone reset, self despawn, and local-entity changes invalidate previous rotation state.
- Sorcerer Fire Mark reconstruction requires observed positive target damage; cast attempts cannot create or extend a landed-hit mark.
- Target-scoped Cleric marks, Spiritmaster Corrode, and Sorcerer Fire Mark stop carrying to a new target. Explicit removal overrides cast-derived effect duration.
- Explicit self-buff removals override cast-derived burst estimates, including Illusive Clone's Heart Gore cooldown bypass. Late and equal-timestamp effect applications cannot revive removed self/target state.
- Mismatched class or unbound actor observations cannot generate any recommendation signals.
- Self buffs use their recipient; party buffs on self are accepted, while buffs cast on allies do not become self buffs.
- Late different-skill critical and shield events cannot replace newer Assassin/Templar proc windows or revive consumed Judgment.
- Target despawn retains action ordering: stale or same-time hits cannot resurrect the cleared target, and stale despawns cannot clear newer target evidence.
- Late skill events cannot roll back player cooldown timestamps. Target switches preserve player cooldown/loadout history.
- Lab settings, history, validation files, public skill-data cache, window title, and installer identity are separate from production. Earlier beta builds through e72bb806 retained a shared meter-bootstrap.json cache; the safety audit isolated it without modifying the existing production cache.

- Duplicate conditional rules retain one strongest candidate per skill, preserving distinct alternatives. The overlay labels these as other current choices rather than a predicted future sequence.

## First live beta issue

A level-14 Chanter test reports no local damage row or recommendations. The screenshot shows other captured rows and unknown target HP (0/0); the local capture/identity cause remains unconfirmed pending the current test log. Missing skill icons do not gate recommendation text.

Overlay preset code still addressed the old footer row after the assistant was inserted, clipping or hiding the assistant diagnostic. Presets now size the assistant automatically and apply footer sizes to the actual footer. Waiting diagnostics separately identify missing selfInfo/class, missing local damage row, missing offensive target, and unknown target HP. These changes reveal the gate; they do not claim to fix missing local capture.

## Class percentages

**Optimized DPS % is not measured for any class.** No validated theoretical-DPS reference, fixed build/gear scenario, or controlled DPS comparison exists in the current project. Test counts and modeled priority rules cannot be converted into percentage of optimal DPS.

The prior project's planning estimates are retained only for checkpoint continuity. They estimate evidence-backed decision coverage, were not measured, and have not been increased because of these correctness fixes. They must not appear as optimized-DPS numbers in the overlay.

| Class | Prior provisional decision-coverage estimate | Optimized DPS % | New validation focus |
|---|---:|---|---|
| Templar | ~93% | Not measured | Four short chain continuations; Judgment consumption; self and target lifecycle |
| Gladiator | ~81% | Not measured | Five short chain continuations; identity and cooldown preservation |
| Assassin | ~87% | Not measured | Four short chain continuations; party exclusion; hidden Insignia/position state still unavailable |
| Ranger | ~84% | Not measured | Two short chain continuations; party exclusion; both Ranger vocabulary sets observed, Precision/charge state still unproven |
| Sorcerer | ~84% | Not measured | Cold Wave consumption; Fire Mark target scope and explicit removal |
| Spiritmaster | ~83% | Not measured | Corrode target scope/removal; player cooldown history survives switches |
| Cleric | ~80% | Not measured | Chain of Torment and Earth Punishment target scope/removal; recipient-aware buffs |
| Chanter | ~82% | Not measured | Two short chain continuations; two documented Dark Crush openers and two-second expiry |

The October 5 full-log review improves evidence breadth but contains class-attribution anomalies. SelfInfo precedence is a bounded software fix; it does not establish the cause of every party/summon class mismatch or validate all eight profiles.

## Outstanding gameplay validation

All eight profiles remain provisional and informational. Buff/debuff/proc opcodes, class identity availability, summon behavior, target HP matching, and actual gameplay recommendations need controlled testing. Stigma knowledge is session-scoped; hidden equipment/build changes are not decoded. Insignia stacks, charge levels, positional safety, movement, and specialization-dependent resets are not inferred.

## Future DPS measurement

Fix client version, class build, gear, encounter, target count, and duration. Establish a reviewed reference rotation using the same configuration and passive evidence. Compare repeated controlled trials or a verified damage simulator. Only then report `observed DPS / reference DPS * 100`, its sample count, and uncertainty. Synthetic lifecycle regressions do not provide that baseline.
