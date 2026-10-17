"""GoatSim experimental, deterministic CLI prototype.

Requires local, opt-in goatsim-export-v1 evidence and an explicit priority file.
No game memory, network access, game input, or inferred AION 2 mechanics.
"""
from __future__ import annotations

import argparse
import json
import math
import sys
from pathlib import Path

try:
    from tools.goatsim_observed_priority import simulate_from_observations
except ModuleNotFoundError:
    from goatsim_observed_priority import simulate_from_observations

SCHEMA = "goatsim-export-v1"
MAX_INPUT_BYTES = 12_000_000
MAX_DURATION_MS = 3_600_000


class PrototypeError(ValueError):
    pass


def read_json(path):
    path = Path(path)
    if path.stat().st_size > MAX_INPUT_BYTES:
        raise PrototypeError("input exceeds local safety limit")
    return json.loads(path.read_text(encoding="utf-8"))


def validate_priority(raw):
    """Require user-provided cooldowns; never infer game mechanics."""
    if not isinstance(raw, list) or not 1 <= len(raw) <= 64:
        raise PrototypeError("priority must be a nonempty list of at most 64 skills")
    seen = set()
    clean = []
    for row in raw:
        if not isinstance(row, dict):
            raise PrototypeError("priority rows must be objects")
        name = row.get("name")
        cooldown = row.get("cooldown_ms")
        if not isinstance(name, str) or not 1 <= len(name) <= 120 or name in seen:
            raise PrototypeError("invalid or duplicate skill name")
        if type(cooldown) is not int or not 0 <= cooldown <= MAX_DURATION_MS:
            raise PrototypeError("cooldown_ms must be a nonnegative integer <= 3600000")
        if row.get("requires") is not None:
            raise PrototypeError("conditional priority skills not supported in this CLI")
        seen.add(name)
        clean.append({"name": name, "cooldown_ms": cooldown})
    return clean


def validate_export(raw):
    if not isinstance(raw, dict) or raw.get("schema") != SCHEMA:
        raise PrototypeError("expected a local goatsim-export-v1 JSON file")
    character = raw.get("character")
    encounters = raw.get("encounters")
    if not isinstance(character, dict) or not isinstance(encounters, list):
        raise PrototypeError("missing character or encounters")
    if not isinstance(character.get("class"), str):
        raise PrototypeError("character class missing")
    if not encounters:
        raise PrototypeError("no observed encounters")
    eligible = []
    rejected = 0
    for encounter in encounters:
        if not isinstance(encounter, dict) or not isinstance(encounter.get("events"), list):
            raise PrototypeError("malformed encounter")
        for event in encounter["events"]:
            if not isinstance(event, dict):
                raise PrototypeError("malformed event")
            value = event.get("damage")
            if (event.get("source_role") == "self" and event.get("damage_kind") == "direct"
                    and isinstance(event.get("skill"), str) and event["skill"]
                    and type(value) in (int, float) and math.isfinite(value)
                    and 0 <= value < 1e12):
                eligible.append(event)
            else:
                rejected += 1
    if not eligible:
        raise PrototypeError("no explicit self direct-hit evidence; no damage inferred")
    return character, eligible, rejected


def run_prototype(export, priority, *, duration_ms=60000, delay_ms=10,
                  action_lock_ms=1000):
    if type(duration_ms) is not int or not 1 <= duration_ms <= MAX_DURATION_MS:
        raise PrototypeError("duration_ms must be between 1 and 3600000")
    if type(action_lock_ms) is not int or not 1 <= action_lock_ms <= 60000:
        raise PrototypeError("action_lock_ms must be between 1 and 60000")
    if delay_ms not in (10, 20):
        raise PrototypeError("delay_ms must be 10 or 20")
    skills = validate_priority(priority)
    character, eligible, rejected = validate_export(export)
    try:
        result = simulate_from_observations(skills, eligible, duration_ms=duration_ms,
                                            delay_ms=delay_ms, action_lock_ms=action_lock_ms)
    except ValueError as exc:
        raise PrototypeError(str(exc)) from exc
    result["estimated_dps"] = result["estimated_damage"] / (duration_ms / 1000)
    result["character"] = {"class": character["class"], "level": character.get("level")}
    result["evidence"] = {"eligible_self_direct_hits": len(eligible),
                          "excluded_or_untyped_hits": rejected}
    result["status"] = "UNVALIDATED EXPERIMENTAL PROTOTYPE"
    result["limitations"] = [
        "Damage uses observed median per direct hit, not a verified per-cast coefficient.",
        "No proc, DoT, resource, animation, buff, target-defense, or server model.",
        "Cooldowns and action lock are caller-supplied assumptions.",
        "Not validated against the 17.1k DPS dummy encounter; not a DPS prediction.",
    ]
    return result


def main(argv=None):
    parser = argparse.ArgumentParser(description="Experimental offline GoatSim priority prototype")
    parser.add_argument("--export", required=True, type=Path, help="Local goatsim-export-v1 JSON")
    parser.add_argument("--priority", required=True, type=Path, help="JSON list of ordered skills with cooldown_ms")
    parser.add_argument("--duration-ms", type=int, default=60000)
    parser.add_argument("--action-lock-ms", type=int, default=1000)
    parser.add_argument("--delay-ms", type=int, choices=(10, 20), default=10)
    args = parser.parse_args(argv)
    try:
        result = run_prototype(read_json(args.export), read_json(args.priority),
                               duration_ms=args.duration_ms,
                               delay_ms=args.delay_ms, action_lock_ms=args.action_lock_ms)
        print(json.dumps(result, indent=2, allow_nan=False))
        return 0
    except (OSError, UnicodeError, ValueError, TypeError, KeyError) as exc:
        print("GoatSim prototype failed: " + str(exc), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
