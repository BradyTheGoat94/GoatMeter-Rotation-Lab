"""GoatSim experimental deterministic event-based simulator.

Standalone Python reference prototype; NOT wired into GoatMeter UI.
Damage coefficients and sample stats are synthetic test fixtures, NOT AION 2 values.
"""
from dataclasses import dataclass, field
from typing import Optional
from urllib.parse import urlparse
import heapq
import random
import re


def parse_shugo_character_url(url: str) -> dict:
    parsed = urlparse(url)
    if parsed.scheme != "https" or parsed.hostname != "shugo.gg" or parsed.port is not None:
        raise ValueError("Expected an HTTPS shugo.gg character URL")
    parts = parsed.path.strip("/").split("/")
    if len(parts) != 4 or parts[0] != "character" or parts[1] != "global":
        raise ValueError("Unsupported Shugo character path")
    server, character = parts[2:]
    if not all(re.fullmatch(r"[A-Za-z0-9_-]{1,64}", p) for p in (server, character)):
        raise ValueError("Invalid server or character name")
    return {"region": "global", "server": server, "characterName": character, "sourceUrl": url}


@dataclass(frozen=True)
class Skill:
    name: str
    potency: float
    cooldown_ms: int
    cast_ms: int
    resource_cost: int = 0

    def __post_init__(self):
        if not self.name or self.potency < 0 or self.cooldown_ms < 0 or self.cast_ms <= 0 or self.resource_cost < 0:
            raise ValueError("Invalid skill parameters")


@dataclass(frozen=True)
class Profile:
    attack: Optional[float]
    crit_chance: Optional[float]
    crit_multiplier: Optional[float]
    resource: int = 0

    def validate(self):
        if self.attack is None or self.crit_chance is None or self.crit_multiplier is None:
            raise ValueError("Incomplete profile: missing offensive stat")
        if self.attack < 0 or not 0 <= self.crit_chance <= 1 or self.crit_multiplier < 1 or self.resource < 0:
            raise ValueError("Invalid profile stat")


def simulate(profile: Profile, skills: list[Skill], duration_ms: int, seed: int = 0) -> dict:
    """Fixed-priority rotation, one action at a time, damage on cast completion.

    No regen, buffs, target defense, latency, animation locks or AION-specific rules.
    These must be modeled and validated before claiming real DPS accuracy.
    """
    profile.validate()
    if duration_ms <= 0 or not skills:
        raise ValueError("Duration and skill list must be positive/nonempty")
    if len({s.name for s in skills}) != len(skills):
        raise ValueError("Skill names must be unique")
    rng = random.Random(seed)
    ready = {s.name: 0 for s in skills}
    resource = profile.resource
    time_ms = 0
    total = 0.0
    breakdown = {s.name: 0.0 for s in skills}
    timeline = []
    while time_ms < duration_ms:
        available = [s for s in skills if ready[s.name] <= time_ms and s.resource_cost <= resource]
        if not available:
            next_ready = min((ready[s.name] for s in skills if s.resource_cost <= resource and ready[s.name] > time_ms), default=None)
            if next_ready is None:
                break
            time_ms = next_ready
            continue
        skill = available[0]
        completion = time_ms + skill.cast_ms
        if completion > duration_ms:
            break
        resource -= skill.resource_cost
        ready[skill.name] = time_ms + skill.cooldown_ms
        damage = profile.attack * skill.potency * (profile.crit_multiplier if rng.random() < profile.crit_chance else 1.0)
        total += damage
        breakdown[skill.name] += damage
        timeline.append({"atMs": completion, "skill": skill.name, "damage": damage})
        time_ms = completion
    return {"totalDamage": total, "dps": total / (duration_ms / 1000), "bySkill": breakdown,
            "timeline": timeline, "remainingResource": resource, "seed": seed,
            "modelStatus": "synthetic-unvalidated"}
