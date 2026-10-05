namespace Aion2DPSPro.Rotation;

public enum AionClass
{
    Gladiator,
    Templar,
    Assassin,
    Ranger,
    Sorcerer,
    Spiritmaster,
    Cleric,
    Chanter
}

public enum RotationMode { SingleTarget, Area, Solo, Support }
public enum ProfileValidation { Unvalidated, Provisional, Validated }

public enum RotationConditionKind
{
    Always,
    CooldownReady,
    BuffPresent,
    BuffMissing,
    DebuffPresent,
    DebuffMissing,
    ResourceAtLeast,
    ResourceAtMost,
    TargetHpAtMost,
    TargetHpAtLeast,
    EnemyCountAtLeast,
    EnemyCountAtMost,
    Moving,
    Stationary
}

public sealed record RotationCondition(
    RotationConditionKind Kind,
    string Key = "",
    double Value = 0,
    double Bonus = 0,
    string Reason = "");

public sealed record RotationRule(
    string Skill,
    double BasePriority,
    IReadOnlyList<RotationCondition> Conditions);

public sealed record RotationProfile(
    AionClass ClassName,
    string BuildId,
    RotationMode Mode,
    ProfileValidation Validation,
    IReadOnlyList<RotationRule> Rules,
    string SourceNote = "");

public sealed record RotationState(
    DateTime Utc,
    AionClass ClassName,
    string BuildId,
    RotationMode Mode,
    IReadOnlyDictionary<string, double> CooldownSeconds,
    IReadOnlySet<string> Buffs,
    IReadOnlySet<string> Debuffs,
    double ResourcePercent,
    double TargetHpPercent,
    int EnemyCount,
    bool IsMoving,
    bool TargetAlive,
    double ObservationConfidence);

public sealed record SkillRecommendation(
    string Skill,
    double Score,
    double Confidence,
    bool Actionable,
    IReadOnlyList<string> Reasons);

public sealed record RotationDecision(
    SkillRecommendation? Next,
    IReadOnlyList<SkillRecommendation> Alternatives,
    string Diagnostic);
