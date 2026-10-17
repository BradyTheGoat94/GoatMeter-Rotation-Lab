"""Repeatable Monte Carlo DPS estimates for GoatSim's synthetic reference model.

This is not calibrated to AION 2 damage formulas.
"""
from dataclasses import dataclass
import math
from statistics import mean
from .prototype import simulate, Profile, Skill


def estimate_dps(profile: Profile, skills: list[Skill], duration_ms: int,
                 trials: int = 200, seed: int = 0) -> dict:
    if isinstance(trials, bool) or not isinstance(trials, int) or not 2 <= trials <= 10000:
        raise ValueError("trials must be an integer between 2 and 10000")
    if isinstance(seed, bool) or not isinstance(seed, int):
        raise ValueError("seed must be an integer")
    # Use separate reproducible random streams per trial.
    values = sorted(simulate(profile, skills, duration_ms, seed + i)["dps"]
                    for i in range(trials))
    def percentile(p: float) -> float:
        pos = (len(values) - 1) * p
        lower = math.floor(pos)
        upper = math.ceil(pos)
        return values[lower] + (values[upper] - values[lower]) * (pos - lower)
    return {
        "meanDps": mean(values),
        "medianDps": percentile(0.5),
        "p05Dps": percentile(0.05),
        "p95Dps": percentile(0.95),
        "minDps": values[0],
        "maxDps": values[-1],
        "trials": trials,
        "seed": seed,
        "modelStatus": "synthetic-unvalidated",
        "warning": "Statistical variation only; does not quantify model accuracy."
    }
