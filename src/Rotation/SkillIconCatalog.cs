namespace Aion2DPSPro.Rotation;

// Display identity only. An icon never grants cooldown, skill, level or loadout eligibility.
public sealed record SkillIconIdentity(AionClass ClassName, string Skill, int SkillId)
{
    public string ResourcePath => $"Assets/SkillIcons/{SkillId}.png";
    public string EvidenceUrl => SkillId is 11440000 or 12440000
        ? $"https://www.aion2pb.com/skills/{SkillId}" : $"https://aion2.gaming.tools/skills/{SkillId}";
}
public static class SkillIconCatalog
{
    static readonly SkillIconIdentity[] entries =
    {
        new(AionClass.Templar,"Pummel",12040000),
        new(AionClass.Templar,"Punishing Strike",12060000),
        new(AionClass.Templar,"Punishment",12090000),
        new(AionClass.Templar,"Judgment",12240000),
        new(AionClass.Templar,"Decisive Strike",12020000),
        new(AionClass.Templar,"Desperate Strike",12030000),
        new(AionClass.Templar,"Vicious Strike",12010000),
        new(AionClass.Templar,"Empyrean Lord's Punishment",12310000),
        new(AionClass.Templar,"Annihilate",12300000),
        new(AionClass.Chanter,"Onslaught",18010000),
        new(AionClass.Chanter,"Resonance Crush",18020000),
        new(AionClass.Chanter,"Bolt Crush",18030000),
        new(AionClass.Chanter,"Incandescent Blow",18040000),
        new(AionClass.Chanter,"Bursting Blow",18050000),
        new(AionClass.Chanter,"Impactful Crush",18060000),
        new(AionClass.Chanter,"Dark Crush",18100000),
        new(AionClass.Chanter,"Spinning Strike",18290000),
        new(AionClass.Chanter,"Wave Blow",18080000),
        new(AionClass.Chanter,"Heat Wave Blow",18150000),
        new(AionClass.Chanter,"Fracturing Blow",18130000),
        new(AionClass.Assassin,"Savage Fang",13270000),
        new(AionClass.Assassin,"Breaking Slice",13030000),
        new(AionClass.Assassin,"Heart Gore",13350000),
        new(AionClass.Assassin,"Insignia Explosion",13130000),
        new(AionClass.Assassin,"Quick Slice",13010000),
        new(AionClass.Assassin,"Savage Back Kick",13110000),
        new(AionClass.Assassin,"Savage Roar",13100000),
        new(AionClass.Assassin,"Savage Smash",13120000),
        new(AionClass.Assassin,"Swift Slice",13040000),
        new(AionClass.Assassin,"Shadowstrike",13070000),
        new(AionClass.Gladiator,"Crushing Wave",11050000),
        new(AionClass.Gladiator,"Frenzied Wave",11060000),
        new(AionClass.Gladiator,"Keen Strike",11020000),
        new(AionClass.Gladiator,"Overhead Slam",11170000),
        new(AionClass.Gladiator,"Rage Burst",11390000),
        new(AionClass.Gladiator,"Rending Blow",11010000),
        new(AionClass.Gladiator,"Ruinous Blow",11100000),
        new(AionClass.Gladiator,"Rupture Strike",11030000),
        new(AionClass.Gladiator,"Smashing Blow",11420000),
        new(AionClass.Gladiator,"Wrathful Strike",11040000),
        new(AionClass.Ranger,"Burst Arrow",14080000),
        new(AionClass.Ranger,"Deadshot",14010000),
        new(AionClass.Ranger,"Drill Dart",14050000),
        new(AionClass.Ranger,"Gale Arrow",14110000),
        new(AionClass.Ranger,"Marking Shot",14090000),
        new(AionClass.Ranger,"Rapid Fire",14030000),
        new(AionClass.Ranger,"Snipe",14020000),
        new(AionClass.Ranger,"Spiral Arrow",14040000),
        new(AionClass.Ranger,"Tempest Shot",14340000),
        new(AionClass.Sorcerer,"Wish of Concentration",15310000),
        new(AionClass.Sorcerer,"Bittercold Wind",15280000),
        new(AionClass.Sorcerer,"Blaze",15050000),
        new(AionClass.Sorcerer,"Cold Storm",15200000),
        new(AionClass.Sorcerer,"Cold Wave",15100000),
        new(AionClass.Sorcerer,"Delayed Explosion",15320000),
        new(AionClass.Sorcerer,"Element Enhancement",15400000),
        new(AionClass.Sorcerer,"Fire Wall",15390000),
        new(AionClass.Sorcerer,"Firestorm",15040000),
        new(AionClass.Sorcerer,"Flame Arrow",15210000),
        new(AionClass.Sorcerer,"Hellfire",15060000),
        new(AionClass.Sorcerer,"Ice Chain",15090000),
        new(AionClass.Cleric,"Bolt",17060000),
        new(AionClass.Cleric,"Chain of Torment",17070000),
        new(AionClass.Cleric,"Condemnation",17350000),
        new(AionClass.Cleric,"Divine Aura",17150000),
        new(AionClass.Cleric,"Earth Punishment",17400000),
        new(AionClass.Cleric,"Earth's Retribution",17010000),
        new(AionClass.Cleric,"Judgment Thunder",17040000),
        new(AionClass.Spiritmaster,"Cold Shock",16010000),
        new(AionClass.Spiritmaster,"Combustion",16040000),
        new(AionClass.Spiritmaster,"Dimensional Control",16330000),
        new(AionClass.Spiritmaster,"Earth Tremor",16030000),
        new(AionClass.Spiritmaster,"Elemental Fusion",16300000),
        new(AionClass.Spiritmaster,"Jointstrike: Corrode",16150000),
        new(AionClass.Spiritmaster,"Jointstrike: Curse",16140000),
        new(AionClass.Spiritmaster,"Jointstrike: Destructive Attack",16240000),
        new(AionClass.Spiritmaster,"Summon: Ancient Spirit",16250000),
        new(AionClass.Templar,"Threatening Blow",12440000),
        new(AionClass.Gladiator,"Upward Strike",11440000),
    };
    public static IReadOnlyList<SkillIconIdentity> Entries => entries;
    public static SkillIconIdentity? Find(AionClass? className, string? skill) =>
        className is null || string.IsNullOrWhiteSpace(skill) ? null :
        entries.FirstOrDefault(e => e.ClassName == className &&
            string.Equals(e.Skill, skill, StringComparison.OrdinalIgnoreCase));
}
