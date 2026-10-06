# GoatMeter Rotation Lab: closed beta test guide

This package is for manual, passive testing of the isolated Rotation Lab. All eight rotation profiles remain provisional and recommendations are informational. CI verifies software behavior; live recommendation accuracy and optimal DPS still need controlled gameplay testing.

## Start tonight's test

1. Download `GoatMeter-Rotation-Lab-win-x64` from a successful **Build Rotation Lab** run in this repository.
2. Extract the entire ZIP into a new folder named GoatMeter-Rotation-Lab. Run its `GoatMeter.exe`; the window is labeled **GoatMeter Rotation Lab — Beta**.
3. Keep the included Protocol folder beside the executable. Npcap must already be installed for live capture.
4. Open the overlay and enter a controlled fight. Recommendations require an exactly decoded local-player identity, known class, and matching observed target HP. A waiting panel means those facts are not yet proven.
5. Read recommendations as experimental advice and press skills manually. No game input is automated.

Settings, fight history, and validation logs are stored under `%LOCALAPPDATA%\GoatMeterRotationLab\`. The lab does not import or overwrite production's `Aion2DPSPro` data.

## Focused checks

- **Solo:** Confirm your class and entity remain the rotation actor. Use each supported opener and follow-up once. The used follow-up must stop being recommended from that same opener.
- **Chanter:** Test Impactful Crush and Spinning Strike separately. Each opens Dark Crush for two seconds, only when its observed base cooldown is ready. Generic ranged activity must not open the window.
- **Identity refresh:** Later conflicting combat class labels must not switch the local profile. Fresh confirmed selfInfo proving a new class must reset prior history.
- **Party:** A party member joining, casting, or changing targets must not replace your local rotation identity or inject their skill history.
- **Two targets:** Apply Chain of Torment / Earth Punishment / Corrode / Fire Mark to target A, then attack B. A's reconstructed effects must not enable B's recommendations. Player cooldowns should continue counting down.
- **Effects:** If an explicit effect-removal event is decoded, the predicted base duration must not resurrect that removed effect. A fresh proven application may re-enable it.
- **Reconnect/zone:** Recommendations should wait again until the new session's local identity, class, target, and observations are established.
- **Unknown state:** Missing cooldown observations, unproven stigmas, unavailable stacks, and unverified icons remain explicit unknown/waiting states.

For each issue record the build SHA from BUILD-INFO.txt, class, skill, approximate time, target-switch/reconnect context, expected recommendation, and actual recommendation. Keep raw captures and player-identifying logs private; report sanitized observations.

## Beta acceptance

A build is ready to start closed beta after regression checks, WPF smoke checks, Windows x64 publish, and artifact upload pass. It is not a claim that every class is optimized or that live protocol/effect coverage is complete. Incorrect recommendations, actor/target mixing, crashes, and data-isolation failures block wider beta.

See BETA-STATUS.md for class-by-class scope and percentage limitations.
