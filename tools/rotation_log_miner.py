#!/usr/bin/env python3
"""Sanitized offline miner for GoatMeter combat logs.

This tool reads local combat-*.log files and emits aggregate class/skill evidence.
It intentionally never emits player names, IP addresses, raw packet bytes, or raw lines.
Observed damage events are evidence for rotation research, not proof of casts or optimal play.
"""

from __future__ import annotations

import argparse
import json
import re
import statistics
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path

CLASSES = (
    "Gladiator", "Templar", "Assassin", "Ranger",
    "Sorcerer", "Spiritmaster", "Cleric", "Chanter",
)

DAMAGE_RE = re.compile(
    r"^(?P<ts>[^|]+)\|tag=damage\|src=(?P<src>\d+)\|tgt=(?P<tgt>\d+)"
    r"\|skill=(?P<skill>[^|]+)\|amount=(?P<amount>\d+)\|"
)
RESOLVED_RE = re.compile(
    r"^(?P<ts>[^|]+)\|tag=resolvedCombat\|entity=(?P<entity>\d+)"
    r"\|name=[^|]+\|class=(?P<class>[^|]+)\|confirmed=(?P<confirmed>True|False)\|"
)
IDENTITY_RE = re.compile(
    r"^(?P<ts>[^|]+)\|tag=identity(?:Map)?\|(?:packet=[^|]+\|)?"
    r"(?:id|entity)=(?P<entity>\d+)\|name=[^|]+\|class=(?P<class>[^|]+)"
)

def parse_time(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00"))

def class_maps(path: Path):
    observed = {}
    confirmed = {}
    with path.open("r", encoding="utf-8", errors="ignore") as fh:
        for line in fh:
            m = RESOLVED_RE.match(line)
            if m:
                entity = int(m.group("entity"))
                cls = m.group("class")
                if cls in CLASSES:
                    observed[entity] = cls
                    if m.group("confirmed") == "True":
                        confirmed[entity] = cls
                continue
            m = IDENTITY_RE.match(line)
            if m:
                entity = int(m.group("entity"))
                cls = m.group("class")
                if cls in CLASSES:
                    observed[entity] = cls
                    confirmed[entity] = cls
    return observed, confirmed

def damage_events(path: Path, confirmed):
    events = []
    with path.open("r", encoding="utf-8", errors="ignore") as fh:
        for line_index, line in enumerate(fh):
            m = DAMAGE_RE.match(line)
            if not m:
                continue
            src = int(m.group("src"))
            cls = confirmed.get(src)
            events.append({
                "ts": parse_time(m.group("ts")),
                "line": line_index,
                "src": src,
                "target": int(m.group("tgt")),
                "skill": m.group("skill"),
                "amount": int(m.group("amount")),
                "class": cls,
            })
    return events

def collapse_actions(events, window_seconds: float):
    grouped = defaultdict(list)
    for event in events:
        if event["class"] in CLASSES:
            grouped[(event["src"], event["class"])].append(event)

    result = {}
    for key, rows in grouped.items():
        rows.sort(key=lambda x: (x["ts"], x["line"]))
        actions = []
        for row in rows:
            if (
                actions
                and actions[-1]["skill"] == row["skill"]
                and (row["ts"] - actions[-1]["ts"]).total_seconds() <= window_seconds
            ):
                actions[-1]["hits"] += 1
                actions[-1]["amount"] += row["amount"]
                actions[-1]["last_ts"] = row["ts"]
            else:
                actions.append({
                    "ts": row["ts"],
                    "last_ts": row["ts"],
                    "skill": row["skill"],
                    "amount": row["amount"],
                    "hits": 1,
                    "target": row["target"],
                })
        result[key] = actions
    return result

def summarize(paths, collapse_window: float, purity_threshold: float):
    all_events = []
    actions_by_file_actor = {}
    confirmed_counts = Counter()
    raw_damage_events = 0

    for path in paths:
        _, confirmed = class_maps(path)
        events = damage_events(path, confirmed)
        raw_damage_events += len(events)
        all_events.extend((path.name, e) for e in events)
        for e in events:
            if e["class"] in CLASSES:
                confirmed_counts[e["class"]] += 1
        collapsed = collapse_actions(events, collapse_window)
        for (actor, cls), seq in collapsed.items():
            actions_by_file_actor[(path.name, actor, cls)] = seq

    skill_classes = defaultdict(Counter)
    for _, event in all_events:
        if event["class"] in CLASSES:
            skill_classes[event["skill"]][event["class"]] += 1

    majority_skill = {}
    for skill, counts in skill_classes.items():
        total = sum(counts.values())
        cls, count = counts.most_common(1)[0]
        if total >= 3 and count / total >= purity_threshold:
            majority_skill[skill] = cls

    classes = {}
    for cls in CLASSES:
        skill_counts = Counter()
        transitions = Counter()
        intervals = defaultdict(list)
        actor_sessions = 0
        action_groups = 0

        for (_, _, actor_cls), seq in actions_by_file_actor.items():
            if actor_cls != cls:
                continue
            actor_sessions += 1
            seq = [x for x in seq if majority_skill.get(x["skill"]) == cls]
            action_groups += len(seq)
            skill_counts.update(x["skill"] for x in seq)

            last_by_skill = {}
            for i, action in enumerate(seq):
                skill = action["skill"]
                previous = last_by_skill.get(skill)
                if previous is not None:
                    delta = (action["ts"] - previous).total_seconds()
                    if collapse_window < delta < 180:
                        intervals[skill].append(delta)
                last_by_skill[skill] = action["ts"]

                if i + 1 < len(seq):
                    nxt = seq[i + 1]
                    gap = (nxt["ts"] - action["last_ts"]).total_seconds()
                    if 0 <= gap <= 8 and nxt["skill"] != skill:
                        transitions[(skill, nxt["skill"])] += 1

        repeat_intervals = []
        for skill, values in intervals.items():
            if len(values) < 5:
                continue
            repeat_intervals.append({
                "skill": skill,
                "samples": len(values),
                "medianSeconds": round(statistics.median(values), 3),
                "minimumSeconds": round(min(values), 3),
            })
        repeat_intervals.sort(key=lambda x: (-x["samples"], x["skill"]))

        classes[cls] = {
            "confirmedDamageEvents": confirmed_counts[cls],
            "actorSessions": actor_sessions,
            "filteredObservedActionGroups": action_groups,
            "topObservedSkills": [
                {"skill": skill, "groups": count}
                for skill, count in skill_counts.most_common(15)
            ],
            "topObservedTransitions": [
                {"from": a, "to": b, "count": count}
                for (a, b), count in transitions.most_common(15)
            ],
            "observedRepeatIntervals": repeat_intervals[:15],
        }

    return {
        "generatedUtc": datetime.now(timezone.utc).isoformat(),
        "source": {
            "logCount": len(paths),
            "totalBytes": sum(p.stat().st_size for p in paths),
            "rawDamageEvents": raw_damage_events,
            "confirmedClassDamageEvents": sum(confirmed_counts.values()),
        },
        "method": {
            "classEvidence": "confirmed identity mappings only",
            "actionGrouping": f"same-skill direct-damage hits within {collapse_window}s collapsed",
            "skillPurityThreshold": purity_threshold,
            "privacy": "no names, IPs, raw packets, or raw lines are emitted",
            "warning": "Observed sequences are evidence, not proof of exact casts or an optimal rotation.",
        },
        "classes": classes,
    }

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("paths", nargs="+", help="Combat log files or directories")
    parser.add_argument("-o", "--output", default="rotation-observations.json")
    parser.add_argument("--collapse-window", type=float, default=0.4)
    parser.add_argument("--purity", type=float, default=0.90)
    args = parser.parse_args()

    paths = []
    for raw in args.paths:
        p = Path(raw)
        if p.is_dir():
            paths.extend(sorted(p.glob("combat-*.log")))
        elif p.is_file():
            paths.append(p)
    paths = sorted(set(paths))
    if not paths:
        raise SystemExit("No combat logs found.")

    result = summarize(paths, args.collapse_window, args.purity)
    Path(args.output).write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"Wrote sanitized observations for {len(paths)} logs to {args.output}")

if __name__ == "__main__":
    main()
