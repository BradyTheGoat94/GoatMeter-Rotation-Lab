# Rotation Lab safety comparison

## Audit scope

Source audit of Lab head e72bb806fdf1a7bce55a20dbb3bd5529708d18fb against the inherited bootstrap baseline 899a1a161b78d8a5487e76930d82fcfdaa4a7219. Both revisions were inspected inside this repository; current installed production binary was not inspected.

Reviewed all 30 src C#/project files, direct native API calls, capture lifecycle, HTTP usage, package references and changed source paths. Static inspection is not an anti-cheat certification, binary audit, dependency security audit or runtime guarantee.

## Verified behavior

- SharpPcap 6.2.5 project dependency and project configuration unchanged.
- Capture still opens Npcap adapters in promiscuous mode with tcp port 13328 filter and reads packet arrival callbacks. No new packet-send path.
- No input synthesis, game process access, process memory reads/writes, DLL injection or remote-thread API use found in application source.
- Native user32 GetWindowLong/SetWindowLong calls are inherited and operate on the Lab overlay's own window for click-through.
- Two inherited HTTP GETs retrieve public skill/NPC labels. They do not send captured logs or packets in the inspected code. Downloaded content is parsed as JSON, not executed.
- Rotation code derives observations and renders informational suggestions; the player executes skills manually.
- A shared public-data cache path was discovered under production's Aion2DPSPro directory. It is now moved to GoatMeterRotationLab. The fix does not migrate, delete or modify the existing production cache. Older Lab packages retain the old cache path.

## Unverified risk

No evidence establishes equal account-ban or anti-cheat risk between the recommendation Lab and the inherited meter. Similar capture behavior does not establish publisher authorization for combat recommendations. Current production releases/installed files may differ from the copied baseline.

Official Global operation policy: https://www.plaync.com/policy/operation/aion2global/en . Review the current policy separately; this code audit cannot determine publisher enforcement or certify authorization.

Profiles and live capture remain beta software. Locally stored validation logs can contain player names, addresses and raw packet data; keep them private.
