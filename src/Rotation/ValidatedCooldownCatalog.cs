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
        new(AionClass.Templar,"Doom Shield",30,"Global 0.0.4387.0","Global client skill 12070000; triggers Judgment for 3s"),
        new(AionClass.Templar,"Shield Rush",20,"Global Season 1","AION 2 Global database skill 12430000; triggers Judgment for 2s, specialization -10s excluded"),
        new(AionClass.Ranger,"Drill Dart",5,"1.0.21.0","AION 2 Global database skill 14050000"),
        new(AionClass.Cleric,"Chain of Torment",20,"Global Season 1","AION 2 Global database skill 17070000"),
        new(AionClass.Cleric,"Condemnation",3,"Global Season 1","AION 2 Global database skill 17350000"),
        new(AionClass.Assassin,"Illusive Clone",90,"Global Season 1","current Global Season 1 guide; base cooldown only"),
        new(AionClass.Sorcerer,"Firestorm",5,"1.0.21.0","Global client skill data; specialty Hellfire reduction excluded"),
        new(AionClass.Sorcerer,"Bittercold Wind",15,"1.0.21.0","Global client skill data; base cooldown only"),
        new(AionClass.Sorcerer,"Blaze",5,"1.0.21.0","Global client skill data; Wish reduction specialty excluded"),
        new(AionClass.Sorcerer,"Hellfire",45,"1.0.21.0","Global client skill data; specialty cooldown reductions excluded"),
        new(AionClass.Sorcerer,"Wish of Concentration",60,"1.0.21.0","AION 2 Global database skill 15310000; base cooldown only"),
        new(AionClass.Chanter,"Impactful Crush",15,"Global Season 1 Sep 2","current Global Season 1 Chanter guide; Sep 2 cooldown reverted to 15s"),
        new(AionClass.Assassin,"Heart Gore",5,"Global Season 1","Global client skill data; base cooldown only, level-16 critical reset excluded"),
        new(AionClass.Assassin,"Insignia Explosion",10,"1.0.21.0","AION 2 Global database skill 13130000; base cooldown only, specialization reductions excluded")
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
