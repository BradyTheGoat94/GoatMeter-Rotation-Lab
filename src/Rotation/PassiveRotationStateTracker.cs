namespace Aion2DPSPro.Rotation;

/// <summary>
/// Reconstructs only rotation facts directly observed in passive combat events.
/// It never reads process memory, injects into the game, modifies packets, or
/// controls input. Unknown cooldown durations deliberately remain unknown.
/// </summary>
public sealed class PassiveRotationStateTracker
{
    readonly object gate=new();
    readonly Dictionary<string,DateTime> lastSkillUse=new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> buffs=new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> debuffs=new(StringComparer.OrdinalIgnoreCase);
    long playerId;
    AionClass? playerClass;
    DateTime? judgmentWindowUntil;
    string judgmentTrigger="";
    DateTime? criticalHitWindowUntil;

    public void Observe(CombatEvent e)
    {
        lock(gate)
        {
            if(e.Kind==CombatKind.Zone) {Reset();return;}
            if(e.Kind==CombatKind.PlayerName && e.SourceIdentityConfirmed && e.SourceId!=0 && Enum.TryParse<AionClass>(e.SourceClass,true,out var cls))
            { playerId=e.SourceId;playerClass=cls; }

            if(playerId==0 || e.SourceId!=playerId)return;
            if(e.Kind is CombatKind.Damage or CombatKind.Heal or CombatKind.Cast)
                if(!string.IsNullOrWhiteSpace(e.Skill))
                {
                    lastSkillUse[e.Skill]=e.Utc;
                    if(playerClass==AionClass.Assassin && e.Kind==CombatKind.Damage && e.DamageFlags.HasFlag(DamageFlags.Critical))
                        criticalHitWindowUntil=e.Utc.AddSeconds(2);
                    if(playerClass==AionClass.Templar && JudgmentWindowSeconds.TryGetValue(e.Skill,out var seconds))
                    {
                        judgmentWindowUntil=e.Utc.AddSeconds(seconds);
                        judgmentTrigger=e.Skill;
                    }
                }
            if(e.Kind==CombatKind.BuffApply && !string.IsNullOrWhiteSpace(e.Effect))buffs.Add(e.Effect);
            if(e.Kind==CombatKind.BuffRemove && !string.IsNullOrWhiteSpace(e.Effect))buffs.Remove(e.Effect);
            if(e.Kind==CombatKind.DebuffApply && !string.IsNullOrWhiteSpace(e.Effect))debuffs.Add(e.Effect);
            if(e.Kind==CombatKind.DebuffRemove && !string.IsNullOrWhiteSpace(e.Effect))debuffs.Remove(e.Effect);
        }
    }

    public PassiveRotationObservation Snapshot()
    {
        lock(gate)return new(playerId,playerClass,
            new Dictionary<string,DateTime>(lastSkillUse,StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(buffs,StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(debuffs,StringComparer.OrdinalIgnoreCase),
            judgmentWindowUntil,judgmentTrigger,criticalHitWindowUntil);
    }

    public void Reset()
    {
        playerId=0;playerClass=null;lastSkillUse.Clear();buffs.Clear();debuffs.Clear();judgmentWindowUntil=null;judgmentTrigger="";criticalHitWindowUntil=null;
    }

    static readonly IReadOnlyDictionary<string,double> JudgmentWindowSeconds =
        new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase)
        {
            ["Shield Smite"]=2,
            ["Warding Strike"]=2,
            ["Doom Shield"]=3
        };
}

public sealed record PassiveRotationObservation(long PlayerId,AionClass? ClassName,
    IReadOnlyDictionary<string,DateTime> LastSkillUse,IReadOnlySet<string> Buffs,IReadOnlySet<string> Debuffs,
    DateTime? JudgmentWindowUntil=null,string JudgmentTrigger="",DateTime? CriticalHitWindowUntil=null)
{
    public bool JudgmentWindowActive(DateTime utc)=>JudgmentWindowUntil is DateTime until && utc<=until;
    public bool CriticalHitWindowActive(DateTime utc)=>CriticalHitWindowUntil is DateTime until && utc<=until;
    public bool UsedRecently(string skill,DateTime utc,double seconds)=>
        LastSkillUse.TryGetValue(skill,out var used) && utc-used>=TimeSpan.Zero && utc-used<=TimeSpan.FromSeconds(seconds);
}
