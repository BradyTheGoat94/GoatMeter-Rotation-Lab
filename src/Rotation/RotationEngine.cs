namespace Aion2DPSPro.Rotation;

/// <summary>
/// Pure recommendation logic only. This layer has no keyboard, mouse, process-memory,
/// injection, packet-modification, or game-control capability.
/// </summary>
public sealed class RotationEngine
{
    public RotationDecision Evaluate(RotationState state, RotationProfile profile)
    {
        if (!state.TargetAlive)
            return Empty("Target is not alive.");

        if (state.ClassName != profile.ClassName)
            return Empty($"Profile class {profile.ClassName} does not match state class {state.ClassName}.");

        if (!string.Equals(state.BuildId, profile.BuildId, StringComparison.OrdinalIgnoreCase))
            return Empty($"Profile build '{profile.BuildId}' does not match state build '{state.BuildId}'.");

        if (state.Mode != profile.Mode)
            return Empty($"Profile mode {profile.Mode} does not match state mode {state.Mode}.");

        if (profile.Rules.Count == 0)
            return Empty($"No validated rules are loaded for {profile.ClassName} / {profile.BuildId}.");

        var validationFactor = profile.Validation switch
        {
            ProfileValidation.Validated => 1.0,
            ProfileValidation.Provisional => 0.65,
            _ => 0.25
        };

        var confidence = Math.Clamp(state.ObservationConfidence, 0, 1) * validationFactor;
        var actionable = profile.Validation == ProfileValidation.Validated && confidence >= 0.80;

        var candidates = new List<SkillRecommendation>();
        foreach (var rule in profile.Rules)
        {
            var score = rule.BasePriority;
            var reasons = new List<string>();
            var eligible = true;

            foreach (var condition in rule.Conditions)
            {
                if (!ConditionPasses(state, condition, out var generatedReason))
                {
                    eligible = false;
                    break;
                }

                score += condition.Bonus;
                if (!string.IsNullOrWhiteSpace(condition.Reason))
                    reasons.Add(condition.Reason);
                else if (!string.IsNullOrWhiteSpace(generatedReason))
                    reasons.Add(generatedReason);
            }

            if (!eligible)
                continue;

            if (reasons.Count == 0)
                reasons.Add("highest eligible priority");

            candidates.Add(new SkillRecommendation(rule.Skill, score, confidence, actionable, reasons));
        }

        var ordered = candidates
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Skill, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ordered.Length == 0)
            return Empty("No rule is currently eligible.");

        var diagnostic = actionable
            ? "Validated recommendation from sufficiently confident passive observations."
            : $"Recommendation is informational only: profile={profile.Validation}, observationConfidence={state.ObservationConfidence:0.00}.";

        return new RotationDecision(ordered[0], ordered.Skip(1).Take(2).ToArray(), diagnostic);
    }

    private static RotationDecision Empty(string diagnostic) =>
        new(null, Array.Empty<SkillRecommendation>(), diagnostic);

    private static bool ConditionPasses(RotationState state, RotationCondition condition, out string reason)
    {
        reason = "";
        switch (condition.Kind)
        {
            case RotationConditionKind.Always:
                return true;

            case RotationConditionKind.CooldownReady:
                var remaining = Lookup(state.CooldownSeconds, condition.Key, double.PositiveInfinity);
                if (remaining <= Math.Max(condition.Value, 0.05))
                {
                    reason = $"{condition.Key} ready";
                    return true;
                }
                return false;

            case RotationConditionKind.BuffPresent:
                if (Contains(state.Buffs, condition.Key))
                {
                    reason = $"{condition.Key} buff active";
                    return true;
                }
                return false;

            case RotationConditionKind.BuffMissing:
                if (!Contains(state.Buffs, condition.Key))
                {
                    reason = $"{condition.Key} buff missing";
                    return true;
                }
                return false;

            case RotationConditionKind.DebuffPresent:
                if (Contains(state.Debuffs, condition.Key))
                {
                    reason = $"{condition.Key} debuff active";
                    return true;
                }
                return false;

            case RotationConditionKind.DebuffMissing:
                if (!Contains(state.Debuffs, condition.Key))
                {
                    reason = $"{condition.Key} debuff missing";
                    return true;
                }
                return false;

            case RotationConditionKind.ResourceAtLeast:
                if (state.ResourcePercent >= condition.Value)
                {
                    reason = $"resource >= {condition.Value:0.#}%";
                    return true;
                }
                return false;

            case RotationConditionKind.ResourceAtMost:
                if (state.ResourcePercent <= condition.Value)
                {
                    reason = $"resource <= {condition.Value:0.#}%";
                    return true;
                }
                return false;

            case RotationConditionKind.TargetHpAtMost:
                if (state.TargetHpPercent <= condition.Value)
                {
                    reason = $"target HP <= {condition.Value:0.#}%";
                    return true;
                }
                return false;

            case RotationConditionKind.TargetHpAtLeast:
                if (state.TargetHpPercent >= condition.Value)
                {
                    reason = $"target HP >= {condition.Value:0.#}%";
                    return true;
                }
                return false;

            case RotationConditionKind.EnemyCountAtLeast:
                return state.EnemyCount >= condition.Value;

            case RotationConditionKind.EnemyCountAtMost:
                return state.EnemyCount <= condition.Value;

            case RotationConditionKind.Moving:
                return state.IsMoving;

            case RotationConditionKind.Stationary:
                return !state.IsMoving;

            case RotationConditionKind.SignalPresent:
                if (Contains(state.Signals, condition.Key))
                {
                    reason = $"{condition.Key} observed";
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    private static bool Contains(IReadOnlySet<string> set, string value) =>
        set.Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));

    private static double Lookup(IReadOnlyDictionary<string, double> map, string key, double fallback)
    {
        foreach (var pair in map)
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        return fallback;
    }
}
