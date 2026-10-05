# Rotation evidence notes

This file records sanitized observations used to challenge or corroborate provisional rotation profiles. Raw combat captures, player names, network addresses, packet bytes, and raw log lines must never be committed.

## Global capture observations

### Templar
- Confirmed Global captures observe Shield Smite followed by Judgment inside the existing short Judgment reaction window.
- Pummel, Vicious Strike, Shield Smite, Punishing Benediction and Judgment all occur in live Templar traffic.
- Filler ordering varies in captures, so Pummel/Vicious remain priority-style filler rather than a hard-coded combo.

### Ranger
- Confirmed Global captures observe Tempest Shot, Concentrated Fire, Drill Dart, Griffon Arrow and Hunter's Soul.
- This live naming/evidence set does not currently align cleanly with the provisional Ranger profile centered on Marking Shot, Deadshot, Snipe, Rupture Arrow and Destruction Trap.
- Until names/mechanics are reconciled against current Global references, do not promote the Ranger profile to validated and do not derive speculative chain signals from these captures.

### Cleric
- Confirmed Global captures directly observe Earth Punishment, Divine Aura, Chain of Torment, Condemnation, Judgment Thunder and Earth's Retribution.
- Observed sequences include Chain of Torment followed by Judgment Thunder and Earth's Retribution, plus Chain of Torment active immediately before Condemnation.
- These observations corroborate the current profile's skill vocabulary, but they do not prove hidden mark/debuff state. Keep Condemnation and mark-dependent decisions signal-gated until passive debuff state is decoded reliably.

### Spiritmaster
- Confirmed Global captures directly observe Combustion, Earth Tremor, Spirit's Descent, Corrode, Jointstrike: Curse and summon-owned attacks.
- Combustion is included as provisional sustained coverage; summon/buff state remains fail-closed where not directly observed.

### Other classes
- Confirmed Global identities for Chanter and Assassin are present in stored captures.
- Mine only sanitized skill names/timing relationships and require repeated or corroborated evidence before changing recommendation logic.

## Evidence policy
- Captures are observational evidence, not proof of optimal rotation.
- Prefer confirmed-class captures.
- Repeated observed transitions may support passive sequence detection when they agree with current Global mechanics/guidance.
- Unknown buffs, stacks, resources, proc states and cooldowns remain fail-closed.
