"""Command-line entry point for the experimental GoatSim reference simulator.

Run: python -m goatsim.cli --input path/to/fixture.json --trials 200
The input is manually provided; no Shugo scraping or live game connection.
"""
import argparse
import json
from pathlib import Path
from .prototype import Skill
from .snapshot import import_snapshot
from .monte_carlo import estimate_dps


def run_payload(payload: dict, trials: int = 200, seed: int = 0) -> dict:
    snapshot = import_snapshot(payload["character"])
    raw_skills = payload["skills"]
    if not isinstance(raw_skills, list) or not raw_skills:
        raise ValueError("skills must be a nonempty array")
    skills = []
    for item in raw_skills:
        if not isinstance(item, dict):
            raise ValueError("skill must be an object")
        skills.append(Skill(name=item["name"], potency=item["potency"],
                            cooldown_ms=item["cooldownMs"], cast_ms=item["castMs"],
                            resource_cost=item.get("resourceCost", 0)))
    duration_ms = payload["durationMs"]
    if isinstance(duration_ms, bool) or not isinstance(duration_ms, int):
        raise ValueError("durationMs must be an integer")
    result = estimate_dps(snapshot.profile, skills, duration_ms, trials, seed)
    return {"character": snapshot.character_name, "server": snapshot.server,
            "className": snapshot.class_name, "results": result}


def main():
    parser = argparse.ArgumentParser(description="Experimental GoatSim CLI (synthetic damage model)")
    parser.add_argument("--input", required=True, help="Manual JSON character and skill fixture")
    parser.add_argument("--trials", type=int, default=200)
    parser.add_argument("--seed", type=int, default=0)
    args = parser.parse_args()
    payload = json.loads(Path(args.input).read_text(encoding="utf-8"))
    print(json.dumps(run_payload(payload, args.trials, args.seed), indent=2))


if __name__ == "__main__":
    main()
