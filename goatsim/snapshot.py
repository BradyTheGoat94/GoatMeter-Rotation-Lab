"""Strict, manual GoatSim character snapshot importer.

No scraping or network access. Values must be provided by user or approved data feed.
All combat stats are explicit; absent values never become zero.
"""
from dataclasses import dataclass
from typing import Any
from .prototype import Profile, parse_shugo_character_url


@dataclass(frozen=True)
class CharacterSnapshot:
    source_url: str
    region: str
    server: str
    character_name: str
    class_name: str
    profile: Profile
    provenance: str = "manual-unverified"


def import_snapshot(payload: dict[str, Any]) -> CharacterSnapshot:
    if not isinstance(payload, dict):
        raise ValueError("Character snapshot must be an object")
    source_url = payload.get("sourceUrl")
    if not isinstance(source_url, str):
        raise ValueError("sourceUrl is required")
    identity = parse_shugo_character_url(source_url)
    class_name = payload.get("className")
    if not isinstance(class_name, str) or not class_name.strip():
        raise ValueError("className is required")
    stats = payload.get("stats")
    if not isinstance(stats, dict):
        raise ValueError("stats object is required")
    required = ("attack", "critChance", "critMultiplier")
    if any(key not in stats or isinstance(stats[key], bool) or
           not isinstance(stats[key], (int, float)) for key in required):
        raise ValueError("attack, critChance and critMultiplier must be numeric")
    import math
    if any(not math.isfinite(stats[key]) for key in required):
        raise ValueError("Non-finite stats are forbidden")
    resource = stats.get("resource", 0)
    if isinstance(resource, bool) or not isinstance(resource, int):
        raise ValueError("resource must be an integer")
    profile = Profile(float(stats["attack"]), float(stats["critChance"]),
                      float(stats["critMultiplier"]), resource)
    profile.validate()
    return CharacterSnapshot(source_url, identity["region"], identity["server"],
                             identity["characterName"], class_name.strip(), profile)
