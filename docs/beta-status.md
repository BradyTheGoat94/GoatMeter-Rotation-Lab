# Rotation Lab beta status

## Improvements in this development run

- One observed opener now grants one pending continuation across 18 existing short chains in six class profiles; an observed Judgment consumes its shield opportunity.
- All eight classes bind rotation state to explicit selfInfo local-player proof. Confirmed party names do not prove local identity.
- Capture reconnect, zone reset, self despawn, and local-entity changes invalidate previous rotation state.
- Target-scoped Cleric marks, Spiritmaster Corrode, and Sorcerer Fire Mark stop carrying to a new target. Explicit removal overrides cast-derived effect duration.
- Self buffs use their recipient; party buffs on self are accepted, while buffs cast on allies do not become self buffs.
- Late skill events cannot roll back player cooldown timestamps. Target switches preserve player cooldown/loadout history.
- Lab settings, history, validation files, window title, and installer identity are separate from production.

## Class percentages

**Optimized DPS % is not measured for any class.** No validated theoretical-DPS reference, fixed build/gear scenario, or controlled DPS comparison exists in the current project. Test counts and modeled priority rules cannot be converted into percentage of optimal DPS.

The prior project's planning estimates are retained only for checkpoint continuity. They estimate evidence-backed decision coverage, were not measured, and have not been increased because of these correctness fixes. They must not appear as optimized-DPS numbers in the overlay.

| Class | Prior provisional decision-coverage estimate | Optimized DPS % | New validation focus |
|---|---:|---|---|
| Templar | ~93% | Not measured | Four short chain continuations; Judgment consumption; self and target lifecycle |
| Gladiator | ~81% | Not measured | Five short chain continuations; identity and cooldown preservation |
| Assassin | ~87% | Not measured | Four short chain continuations; party exclusion; hidden Insignia/position state still unavailable |
| Ranger | ~84% | Not measured | Two short chain continuations; party exclusion; vocabulary/Precision coverage remains provisional |
| Sorcerer | ~84% | Not measured | Cold Wave consumption; Fire Mark target scope and explicit removal |
| Spiritmaster | ~83% | Not measured | Corrode target scope/removal; player cooldown history survives switches |
| Cleric | ~80% | Not measured | Chain of Torment and Earth Punishment target scope/removal; recipient-aware buffs |
| Chanter | ~82% | Not measured | Two short chain continuations; uncorroborated Dark Crush triggers remain excluded |

## Outstanding gameplay validation

All eight profiles remain provisional and informational. Buff/debuff/proc opcodes, class identity availability, summon behavior, target HP matching, and actual gameplay recommendations need controlled testing. Stigma knowledge is session-scoped; hidden equipment/build changes are not decoded. Insignia stacks, charge levels, positional safety, movement, and specialization-dependent resets are not inferred.

## Future DPS measurement

Fix client version, class build, gear, encounter, target count, and duration. Establish a reviewed reference rotation using the same configuration and passive evidence. Compare repeated controlled trials or a verified damage simulator. Only then report `observed DPS / reference DPS * 100`, its sample count, and uncertainty. Synthetic lifecycle regressions do not provide that baseline.
