namespace Aion2DPSPro.Rotation;

// Display identity only. An icon never grants cooldown, skill, level or loadout eligibility.
public sealed record SkillIconIdentity(AionClass ClassName, string Skill, int SkillId)
{
    public string ResourcePath => $"Assets/SkillIcons/{SkillId}.png";
    public string EvidenceUrl => $"https://aion2.gaming.tools/skills/{SkillId}";
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
    };
    public static IReadOnlyList<SkillIconIdentity> Entries => entries;
    public static SkillIconIdentity? Find(AionClass? className, string? skill) =>
        className is null || string.IsNullOrWhiteSpace(skill) ? null :
        entries.FirstOrDefault(e => e.ClassName == className &&
            string.Equals(e.Skill, skill, StringComparison.OrdinalIgnoreCase));
}
