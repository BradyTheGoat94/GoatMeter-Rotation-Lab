namespace Aion2DPSPro.Rotation;

public sealed record ValidatedCooldown(AionClass ClassName,string Skill,double BaseSeconds,string GlobalVersion,string Evidence);

/// <summary>
/// Explicit allow-list only. Entries belong here only when a current Global
/// source establishes the base cooldown. Build/specialization modifiers are
/// intentionally excluded until the player's build can be observed safely.
/// </summary>
public static class ValidatedCooldownCatalog
{
    static readonly ValidatedCooldown[] entries =
    {
        new(AionClass.Templar,"Punishment",30,"1.0.21.0","AION 2 Global database skill 12090000"),
        new(AionClass.Templar,"Empyrean Lord's Punishment",60,"1.0.21.0","AION 2 Global database skill 12310000"),
        new(AionClass.Ranger,"Drill Dart",5,"1.0.21.0","AION 2 Global database skill 14050000")
    };

    public static IReadOnlyList<ValidatedCooldown> Entries => entries;

    public static IReadOnlyDictionary<string,double> Remaining(
        PassiveRotationObservation observed,AionClass className,DateTime utc)
    {
        var result=new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase);
        foreach(var entry in entries.Where(x=>x.ClassName==className))
            if(observed.LastSkillUse.TryGetValue(entry.Skill,out var used))
                result[entry.Skill]=Math.Max(0,entry.BaseSeconds-(utc-used).TotalSeconds);
        return result;
    }
}
