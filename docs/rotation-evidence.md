# Rotation evidence notes

This file records sanitized observations used to challenge or corroborate provisional rotation profiles. Raw combat captures, player names, network addresses, packet bytes, and raw log lines must never be committed.

## Global capture observations

### Templar
- Confirmed Global captures observe Shield Smite followed by Judgment inside the existing short Judgment reaction window.
- Current Global Judgment activates after shield attacks; the lab preserves only the observed 2–3s trigger windows. Annihilate remains separately gated on a directly observed target Stun or Knockdown and uses its 20s base cooldown; the low-chance Incapacitated-Immunity activation and -10s specialization are not inferred.
- Pummel has a guaranteed Punishing Strike chain activation for 3s, so an observed Pummel opens that short continuation and it is consumed before longer cooldown actions.
- Punishment uses its validated 30s base cooldown during observed combat and its directly observed cast reconstructs the 20s Executor base window.
- Empyrean Lord's Punishment is a loadout-dependent stigma with a 60s base cooldown. It is not proactively recommended until the current session has passively observed it at least once.
- Vicious Strike remains a passive-observation chain starter; filler ordering varies in captures, so unsupported specialization routing is not invented.

### Ranger
- Confirmed Global captures observe Tempest Shot, Concentrated Fire, Drill Dart, Griffon Arrow and Hunter's Soul.
- The October 5 full-log review also observes Marking Shot, Deadshot, Snipe, Rapid Fire and Spiral Arrow in confirmed-class Ranger records. Both vocabulary sets coexist; do not alias Tempest Shot or Concentrated Fire to a different skill.
- Skill occurrence does not prove Precision state, charge level, priority quality or readiness. Ranger remains provisional; no new inferred Precision/charge signal is added.

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
- Current Global Ruinous Blow has a 45s base cooldown and grants Prepare for Battle for 20s (+20% PvE damage plus offensive stats). The lab recommends it during proven combat on its base cooldown and reconstructs only the observed 20s base buff window.
- Rage Burst is a loadout-dependent stigma with a 45s base cooldown and a 10s Overhead Slam activation window. The assistant recommends later uses only after the current session has directly observed Rage Burst; the 30s specialization cooldown is excluded.
- Brief observed chain continuations (Smashing, Rupture/Wrathful, Frenzied Wave, Upward Strike) outrank longer cooldown actions so the passive chain window is not dropped.
- Unreconciled Seismic Crash vocabulary is excluded from the current-Global Gladiator profile rather than treated as a hidden finisher.

### Assassin
- Confirmed Global captures directly observe Heart Gore, Insignia Explosion, Exploit Weakness, Savage Fang, Ambush Stance and Doppelganger Attack.
- Current Global Heart Gore remains critical-gated. Outside Illusive Clone the lab uses its validated 5s base cooldown; during the directly observed 20s Illusive Clone window, an observed critical may recommend Heart Gore without that normal cooldown because Clone removes it. Illusive Clone itself retains the current-Global 90s base cooldown.
- Savage Roar and Exploit Weakness can contribute Insignias, but passive action ordering does not prove the target has the full stack state needed for an optimal Insignia Explosion. Keep InsigniaReady fail-closed until stack count/readiness is decoded reliably.
- Ambush Stance and Doppelganger Attack are passive/proc behavior rather than player-pressed rotation actions, so they are observed for damage accounting but are not emitted as recommendations.

### Sorcerer
- Confirmed Global captures directly observe Blaze, Ice Chain, Cold Wave, Cold Snap, Burst and Cold Storm.
- The Sep-16 current-Global change makes Fire Mark a 100% proc on landed Fire attacks and Blaze no longer consumes it. The lab reconstructs only the 5s base mark window from named observed Fire actions or a directly decoded Fire Mark debuff.
- Ice Chain and Cold Wave capture vocabulary agrees with the current Global chain definition, supporting a short passive Cold Wave opportunity after an observed Ice Chain.
- Current Global base stigma cooldowns used by the lab are Element Enhancement 60s, Delayed Explosion 30s, Fire Wall 60s and Cold Storm 60s. Because those skills are loadout-dependent, the assistant does not recommend them until that stigma has been observed in the current session.
- Element Enhancement and Wish of Concentration provide observed 10s burst windows; Delayed Explosion provides a bounded 4s damage-amplification window. Specialty cooldown reductions and unreconciled Flame Cage/Flame Harpoon routing remain excluded.

### Chanter
- Confirmed Global captures directly observe Rushing Smash, Bursting Blow, Fracturing Blow, Incandescent Blow, Onslaught and Impactful Crush.
- Repeated sequences include Incandescent Blow followed by Onslaught and Bursting Blow, but repetition alone does not prove a mandatory chain. Keep these as sustained priority skills unless current Global mechanics corroborate a deterministic transition.
- Current Global client-derived skill data version 2.0.5.0 (checked October 5) explicitly grants Dark Crush for **2s** after Impactful Crush (18060000) or Spinning Strike (18290000). This supersedes the older three-second estimate. Dark Crush retains its 5s base cooldown; cooldown-removal specialty is excluded.
- Sources: https://aion2.gaming.tools/skills/18060000 and https://aion2.gaming.tools/skills/18290000 ; cooldown https://aion2.gaming.tools/skills/18100000 .
- October 5 confirmed-class damage records include both setup/follow-up pairs, but they are observational corroboration, not direct proc decoding or proof of optimal ordering. Other ranged triggers remain excluded.

## Evidence policy
- Captures are observational evidence, not proof of optimal rotation.
- Prefer confirmed-class captures.
- Repeated observed transitions may support passive sequence detection when they agree with current Global mechanics/guidance.
- Unknown buffs, stacks, resources, proc states and cooldowns remain fail-closed.
