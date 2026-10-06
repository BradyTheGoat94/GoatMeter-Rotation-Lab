# Rotation Lab beta status

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
