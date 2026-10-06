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
- Ancient Spirit may react to directly observed Flame Blessing or Spirit's Benediction casts inside the bounded passive window, but the Rotation Lab does not proactively recommend those buffs until a passive opener trigger is independently proven.
- Base Corrode state is preserved for 20s from a directly observed local cast when a separate debuff event is unavailable. The 30s specialization extension is intentionally excluded unless it becomes directly observable.

### Gladiator
- Confirmed Global captures directly observe Rending Blow, Keen Strike, Wrathful Strike, Smashing Blow, Rupture Strike and Murderous Burst.
- Repeated observations corroborate the sustained chain vocabulary. Passive timing windows are used only where current Global mechanics also support the transition; no hidden proc state is inferred.

### Assassin
- Confirmed Global captures directly observe Heart Gore, Insignia Explosion, Exploit Weakness, Savage Fang, Ambush Stance and Doppelganger Attack.
- Heart Gore is repeatedly observed with critical/back-hit context, but capture ordering does not prove Insignia stack count. Keep InsigniaReady fail-closed until stack state is decoded reliably.

### Sorcerer
- Confirmed Global captures directly observe Blaze, Ice Chain, Cold Wave, Cold Snap, Burst and Cold Storm.
- Ice Chain and Cold Wave capture vocabulary agrees with the current Global chain definition, supporting a short passive Cold Wave opportunity after an observed Ice Chain.
- Do not infer Fire Mark, Element Enhancement, DoT refresh state or other hidden buff/debuff state from damage ordering alone.

### Chanter
- Confirmed Global captures directly observe Rushing Smash, Bursting Blow, Fracturing Blow, Incandescent Blow, Onslaught and Impactful Crush.
- Repeated sequences include Incandescent Blow followed by Onslaught and Bursting Blow, but repetition alone does not prove a mandatory chain. Keep these as sustained priority skills unless current Global mechanics corroborate a deterministic transition.
- Impactful Crush remains the evidence-backed passive trigger for the short Dark Crush opportunity.

## Evidence policy
- Captures are observational evidence, not proof of optimal rotation.
- Prefer confirmed-class captures.
- Repeated observed transitions may support passive sequence detection when they agree with current Global mechanics/guidance.
- Unknown buffs, stacks, resources, proc states and cooldowns remain fail-closed.
