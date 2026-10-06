# October 5 full-log evidence review

## Scope and method

Read-only offline review of one user-provided validation log: 865,884 lines, October 6 00:07:50–02:36:23 UTC (October 5 8:07–10:36 p.m. Eastern). File size 3,139,518,957 bytes. Raw log, player identities, entity IDs, addresses and packet bytes stay private.

The log's originating application revision is not established. Findings are diagnostic observations, not a replay of the current Lab build.

Matched all 166,855 resolvedCombat records to preceding damage/dot records using identical timestamp, source entity and target entity; when a packet contains multiple matching records, use FIFO logging order. This is a log correlation assumption, not an independent parser validation. 131,471 resolved records report confirmed=True; 35,384 do not. Excluding 298 exact duplicate confirmed records leaves 131,173 records. Hits, DoTs and healing-like entries are not player cast counts.

## Findings

- All eight class labels occur in confirmed resolved records. Explicit known-class selfIdentity appears for Templar and Chanter only; this is not eight verified local-player trials.
- Ranger records include Marking Shot, Deadshot, Snipe, Rapid Fire and Spiral Arrow alongside Tempest Shot and Concentrated Fire. Skill names must coexist rather than be guessed aliases.
- Confirmed Chanter adjacent damage transitions within three seconds include Spinning Strike → Dark Crush (142) and Impactful Crush → Dark Crush (135). These broad counts do not collapse all multi-hit groups or prove activation. A stricter pass requiring preceding Chanter selfIdentity within ten minutes and collapsing same-skill hits at 0.4s yields only two Dark Crush groups, both after Impactful Crush; no clean local Spinning-only group is established by that pass.
- Current Global client-derived skill entries, version 2.0.5.0 checked October 5, independently establish that both named openers grant Dark Crush for **two seconds**. The Lab follows that explicit mechanic rather than estimating duration from damage intervals.
- 3,027 unresolved summon-candidate records and 42 missing-owner-evidence records warrant capture investigation. Neither count is a unique summon or missing-DPS measurement.
- Suspect confirmed class/skill combinations include Chanter/Tempest Shot (245), Chanter/Deadshot (7), Chanter/Griffon Arrow (3), Spiritmaster/Divine Aura (20), and Templar/water-spirit attacks (7). Attribution, entity reuse, summon mapping, and decode behavior remain candidate explanations, not established causes.
- No buff/debuff application/removal tags were observed. Absence of these tags does not prove those packet events cannot exist; this log does not validate that state.

## Bounded changes

Preserve known selfInfo class over later combat or non-local identity class labels. Permit fresh confirmed selfInfo to prove a changed class. This protects local rotation history; broader party/summon attribution remains unresolved.

Support both documented Chanter openers with the two-second window and enforce the existing five-second Dark Crush base cooldown. Do not infer specialty cooldown removal, other ranged triggers, or hidden stacks.

Keep Ranger profile provisional; update evidence breadth without inventing Precision or charge state.

Sources:
- https://aion2.gaming.tools/skills/18060000
- https://aion2.gaming.tools/skills/18290000
- https://aion2.gaming.tools/skills/18100000

No optimal-DPS percentage or coverage increase is measured by this review. Independent holdout gameplay and current Lab capture tests remain required.
