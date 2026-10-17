"""GoatSim experimental deterministic event-based simulator.

Standalone Python reference prototype; NOT wired into GoatMeter UI.
Damage coefficients and sample stats are synthetic test fixtures, NOT AION 2 values.
"""
from dataclasses import dataclass
from typing import Optional
from urllib.parse import urlparse
import math
import random
import re


def parse_shugo_character_url(url: str) -> dict:
    if not isinstance(url, str):
        raise ValueError("Expected a character URL string")
    try:
        parsed = urlparse(url)
        port = parsed.port
    except ValueError as exc:
        raise ValueError("Malformed character URL") from exc
    if (parsed.scheme != "https" or parsed.hostname != "shugo.gg" or port is not None
            or parsed.username is not None or parsed.password is not None):
        raise ValueError("Expected an HTTPS shugo.gg character URL")
    parts = parsed.path.strip("/").split("/")
    if len(parts) != 4 or parts[0] != "character" or parts[1] != "global":
        raise ValueError("Unsupported Shugo character path")
    server, character = parts[2:]
    if not all(re.fullmatch(r"[A-Za-z0-9_-]{1,64}", p) for p in (server, character)):
        raise ValueError("Invalid server or character name")
    return {"region": "global", "server": server, "characterName": character, "sourceUrl": url}


def _finite_number(value: object) -> bool:
    return (not isinstance(value, bool) and isinstance(value, (int, float))
            and math.isfinite(value))


def _nonnegative_int(value: object) -> bool:
    return not isinstance(value, bool) and isinstance(value, int) and value >= 0


@dataclass(frozen=True)
class Skill:
    name: str
    potency: float
    cooldown_ms: int
    cast_ms: int
    resource_cost: int = 0

    def __post_init__(self):
        if (not isinstance(self.name, str) or not 1 <= len(self.name.strip()) <= 128
                or not _finite_number(self.potency) or self.potency < 0
                or not _nonnegative_int(self.cooldown_ms)
                or not _nonnegative_int(self.cast_ms) or self.cast_ms == 0
                or not _nonnegative_int(self.resource_cost)):
            raise ValueError("Invalid skill parameters")


@dataclass(frozen=True)
class Profile:
    attack: Optional[float]
    crit_chance: Optional[float]
    crit_multiplier: Optional[float]
    resource: int = 0

    def validate(self):
        if (not _finite_number(self.attack) or not _finite_number(self.crit_chance)
                or not _finite_number(self.crit_multiplier)
                or not _nonnegative_int(self.resource)
                or self.attack < 0 or not 0 <= self.crit_chance <= 1
                or self.crit_multiplier < 1):
            raise ValueError("Invalid or incomplete profile stats")


def simulate(profile: Profile, skills: list[Skill], duration_ms: int, seed: int = 0) -> dict:
    """Fixed-priority rotation, one action at a time, damage on cast completion.

    No regen, buffs, target defense, latency, animation locks or AION-specific rules.
    These must be modeled and validated before claiming real DPS accuracy.
    """
    if not isinstance(profile, Profile):
        raise ValueError("profile must be a Profile")
    profile.validate()
    if (not isinstance(duration_ms, int) or isinstance(duration_ms, bool)
            or not 0 < duration_ms <= 3_600_000):
        raise ValueError("duration_ms must be an integer in (0, 3600000]")
    if not isinstance(seed, int) or isinstance(seed, bool):
        raise ValueError("seed must be an integer")
    if not isinstance(skills, list) or not skills or not all(isinstance(s, Skill) for s in skills):
        raise ValueError("skills must be a nonempty list of Skill")
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
        # Choose the highest-priority available skill that fits the remaining time.
        skill = next((s for s in available if time_ms + s.cast_ms <= duration_ms), None)
        if skill is None:
            break
        completion = time_ms + skill.cast_ms
        resource -= skill.resource_cost
        ready[skill.name] = time_ms + skill.cooldown_ms
        damage = profile.attack * skill.potency * (profile.crit_multiplier if rng.random() < profile.crit_chance else 1.0)
        if not math.isfinite(damage) or not math.isfinite(total + damage):
            raise ValueError("Damage overflow; check synthetic stat magnitudes")
        if len(timeline) >= 100_000:
            raise ValueError("Simulation exceeded 100000 events; shorten the scenario")
        total += damage
        breakdown[skill.name] += damage
        timeline.append({"atMs": completion, "skill": skill.name, "damage": damage})
        time_ms = completion
    return {"totalDamage": total, "dps": total / (duration_ms / 1000), "bySkill": breakdown,
            "timeline": timeline, "remainingResource": resource, "seed": seed,
            "modelStatus": "synthetic-unvalidated"}
