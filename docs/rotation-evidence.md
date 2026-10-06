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
- Current Global base data used by the lab: Earth Punishment 30s cooldown / 10s base effect, Chain of Torment 20s cooldown / 10s base effect, Condemnation 3s, Divine Aura 30s and Bolt 45s. Specialty cooldown resets/reductions are excluded unless directly observed.
- Chain of Torment and Earth Punishment may preserve their base active windows from observed local casts when a separate debuff event is unavailable; a directly observed debuff remains authoritative beyond the base duration without assuming the +3s/+10s specialties.
- Earth Punishment is a stigma and is not proactively recommended until the current session has passively observed the player use it at least once. The single-target damage profile does not emit healing recommendations because party-health state is not decoded by the passive rotation tracker.

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
- The Sep-16 current-Global change makes Fire Mark a 100% proc on landed Fire attacks and Blaze no longer consumes it. The lab reconstructs only the 5s base mark window from named observed Fire actions or a directly decoded Fire Mark debuff.
- Ice Chain and Cold Wave capture vocabulary agrees with the current Global chain definition, supporting a short passive Cold Wave opportunity after an observed Ice Chain.
- Current Global base stigma cooldowns used by the lab are Element Enhancement 60s, Delayed Explosion 30s, Fire Wall 60s and Cold Storm 60s. Because those skills are loadout-dependent, the assistant does not recommend them until that stigma has been observed in the current session.
- Element Enhancement and Wish of Concentration provide observed 10s burst windows; Delayed Explosion provides a bounded 4s damage-amplification window. Specialty cooldown reductions and unreconciled Flame Cage/Flame Harpoon routing remain excluded.

### Chanter
- Confirmed Global captures directly observe Rushing Smash, Bursting Blow, Fracturing Blow, Incandescent Blow, Onslaught and Impactful Crush.
- Repeated sequences include Incandescent Blow followed by Onslaught and Bursting Blow, but repetition alone does not prove a mandatory chain. Keep these as sustained priority skills unless current Global mechanics corroborate a deterministic transition.
- Impactful Crush remains the evidence-backed passive trigger for the short Dark Crush opportunity.

## Evidence policy
- Captures are observational evidence, not proof of optimal rotation.
- Prefer confirmed-class captures.
- Repeated observed transitions may support passive sequence detection when they agree with current Global mechanics/guidance.
- Unknown buffs, stacks, resources, proc states and cooldowns remain fail-closed.
