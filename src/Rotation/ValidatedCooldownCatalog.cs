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
        new(AionClass.Templar,"Annihilate",20,"Global 1.0.21.0","AION 2 Global release skill 12300000; requires Stun or Knockdown, level-12 -10s specialization excluded"),
        new(AionClass.Templar,"Empyrean Lord's Punishment",60,"1.0.21.0","AION 2 Global database skill 12310000"),
        new(AionClass.Templar,"Doom Shield",30,"Global 0.0.4387.0","Global client skill 12070000; triggers Judgment for 3s"),
        new(AionClass.Templar,"Shield Rush",20,"Global Season 1","AION 2 Global database skill 12430000; triggers Judgment for 2s, specialization -10s excluded"),
        new(AionClass.Gladiator,"Overhead Slam",5,"Global Season 1","current Global client data; level-16 no-cooldown specialization excluded"),
        new(AionClass.Gladiator,"Ruinous Blow",45,"Global Season 1","current Global client data; specialization/cooldown-reduction effects excluded"),
        new(AionClass.Gladiator,"Crushing Wave",20,"Global Season 1","current Global client data; base cooldown only"),
        new(AionClass.Gladiator,"Rage Burst",45,"Global Season 1","current Global client data; level-5 30s specialization excluded"),
        new(AionClass.Ranger,"Marking Shot",10,"Global 1.0.21.0","current Global release skill 14090000; base 10s Precision duration, +5s duration and consecutive-use specializations excluded"),
        new(AionClass.Ranger,"Deadshot",20,"Global release","AION 2 Global release database skill 14010000; charge level and specialization modifiers excluded"),
        new(AionClass.Ranger,"Drill Dart",5,"1.0.21.0","AION 2 Global database skill 14050000"),
        new(AionClass.Ranger,"Burst Arrow",20,"Global Season 1","current Global April balance; base cooldown reduced from 30s to 20s"),
        new(AionClass.Cleric,"Earth Punishment",30,"Global Season 1","current Global stigma skill; 10s base effect, +10s duration specialization excluded"),
        new(AionClass.Cleric,"Chain of Torment",20,"Global Season 1","AION 2 Global database skill 17070000"),
        new(AionClass.Cleric,"Condemnation",3,"Global Season 1","AION 2 Global database skill 17350000"),
        new(AionClass.Cleric,"Divine Aura",30,"Global 1.0.21.0","current Global skill data; level-16 -10s cooldown specialization excluded"),
        new(AionClass.Cleric,"Bolt",45,"Global 1.0.21.0","AION 2 Global skill 17060000; Earths Retribution/Discharge cooldown reduction excluded"),
        new(AionClass.Assassin,"Savage Fang",60,"Global 2.0.5.0","Global stigma skill 13270000; equipped loadout requires observed use"),
        new(AionClass.Assassin,"Illusive Clone",90,"Global Season 1","current Global Season 1 guide; base cooldown only"),
        new(AionClass.Assassin,"Shadowstrike",20,"Global 0.0.4387.0","current Global client extraction; 10s Critical Damage buff, positional safety remains player-controlled"),
        new(AionClass.Spiritmaster,"Summon: Ancient Spirit",90,"Global 1.0.21.0","current Global release skill 16250000; 30s summon duration, specialization effects excluded"),
        new(AionClass.Spiritmaster,"Jointstrike: Corrode",45,"Global 1.0.21.0","current Global release skill 16150000; 20s base Corrode duration, +10s duration specialization excluded"),
        new(AionClass.Spiritmaster,"Jointstrike: Curse",10,"Global 1.0.21.0","current Global skill database; base cooldown only"),
        new(AionClass.Sorcerer,"Element Enhancement",60,"Global Season 1","current Global stigma base cooldown; loadout must be passively observed"),
        new(AionClass.Sorcerer,"Delayed Explosion",30,"Global Season 1","current Global stigma base cooldown; -10s specialization excluded"),
        new(AionClass.Sorcerer,"Fire Wall",60,"Global Season 1","current Global stigma base cooldown; specialization duration effects excluded"),
        new(AionClass.Sorcerer,"Cold Storm",60,"Global Season 1","current Global stigma base cooldown; specialization duration effects excluded"),
        new(AionClass.Sorcerer,"Firestorm",5,"1.0.21.0","Global client skill data; specialty Hellfire reduction excluded"),
        new(AionClass.Sorcerer,"Bittercold Wind",15,"1.0.21.0","Global client skill data; base cooldown only"),
        new(AionClass.Sorcerer,"Blaze",5,"1.0.21.0","Global client skill data; Wish reduction specialty excluded"),
        new(AionClass.Sorcerer,"Hellfire",45,"1.0.21.0","Global client skill data; specialty cooldown reductions excluded"),
        new(AionClass.Sorcerer,"Wish of Concentration",60,"1.0.21.0","AION 2 Global database skill 15310000; base cooldown only"),
        new(AionClass.Chanter,"Fracturing Blow",45,"Global 2.0.5.0","Global client skill 18130000; stigma, -15s specialization excluded"),
        new(AionClass.Chanter,"Impactful Crush",15,"Global Season 1 Sep 2","current Global Season 1 Chanter guide; Sep 2 cooldown reverted to 15s"),
        new(AionClass.Chanter,"Dark Crush",5,"Global Season 1","current Global release base cooldown; level-16 no-cooldown specialization excluded"),
        new(AionClass.Chanter,"Heat Wave Blow",10,"1.0.21.0","AION 2 Global database skill 18150000; base cooldown only"),
        new(AionClass.Chanter,"Wave Blow",20,"Global 1.0.21.0","current Global release skill 18080000; requires Stun target state, incapacitation-immunity specialization behavior excluded"),
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
