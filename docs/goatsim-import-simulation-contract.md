# GoatSim character import and simulation contract (experimental)

## Import source
Example public profile: `https://shugo.gg/character/global/nae-zikel/Bradyboi?tab=stats`.

The user may paste a public character URL. Treat website HTML and profile data as untrusted. Do not assume a documented Shugo API exists. Prefer a documented, permitted endpoint if available; otherwise implement a user-reviewed manual import fallback. Never request account credentials or bypass bot protections.

## Normalized character snapshot
- source, sourceUrl, capturedAtUtc, region, server, characterName, className
- stats: map of named numeric stats with units and original display labels
- equipment: optional items with slot, item name, enhancement, and relevant modifiers
- skills: optional skill levels, specialization, and selected build choices
- provenance: per-field source and confidence (verified / inferred / missing)
- simulatorVersion and combatRulesVersion

Unknown or missing stats must remain null, never silently default to zero. Preserve original values and units for audit.

## Simulator boundary
1. Character snapshot -> normalized, validated inputs.
2. Combat rules -> versioned, class-specific skill definitions.
3. Rotation strategy -> recommended next legal action, not game input.
4. Deterministic simulation -> event queue of casts, hits, cooldowns, buffs, resources, target state.
5. Output -> DPS, total damage, skill breakdown, timeline, assumptions, uncertainty and excluded mechanics.

Use seeded RNG for crit/proc Monte Carlo; provide mean and percentile ranges rather than a made-up accuracy percentage. Record boss defense, resistance, level and encounter duration assumptions.

## Acceptance checks
- Invalid Shugo URLs rejected without network requests.
- Missing class or essential offensive stat yields incomplete-profile status, not a fabricated DPS result.
- Stat units are parsed consistently (percentage versus flat ratings).
- Skill cannot be cast during cooldown, without resource, or outside legal conditions.
- A 20-second Corrode baseline and an optional 30-second specialization are separate rule configurations pending current Global verification.
- Changing only the encounter duration reproduces deterministic results when RNG seed and inputs match.
- Character name / server in URL is never taken as proof of identity or live ownership.
- No automation, injection, packet modification, or gameplay input.

## Next implementation step
Inspect the actual lab solution and existing class/skill model; integrate the normalized snapshot and deterministic engine with unit tests on this branch. Verify live Shugo data access and terms before wiring a fetcher. Do not merge to production without tests and validation.
