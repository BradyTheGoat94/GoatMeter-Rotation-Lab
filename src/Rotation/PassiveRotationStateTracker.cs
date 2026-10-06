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
    readonly Dictionary<string,DateTime> targetSkillUse=new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string,DateTime> targetEffectRemovals=new(StringComparer.OrdinalIgnoreCase);
    TargetStats? target;
    long targetId;
    DateTime lastTargetActionUtc=DateTime.MinValue;
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
            if(e.SourceIsLocal && e.SourceIdentityConfirmed && e.SourceId!=0 && Enum.TryParse<AionClass>(e.SourceClass,true,out var cls))
            {
                if(playerId!=0 && (playerId!=e.SourceId || playerClass!=cls))Reset();
                playerId=e.SourceId;playerClass=cls;
            }

            if(playerId==0)return;
            if(e.Kind==CombatKind.Despawn)
            {
                if(e.SourceId==playerId)Reset();
                else if(e.SourceId==targetId)ClearTarget();
                return;
            }
            // Buffs belong to their recipient; party buffs on self are observable,
            // while a local buff cast on an ally says nothing about our own state.
            if(e.TargetId==playerId && !string.IsNullOrWhiteSpace(e.Effect))
            {
                if(e.Kind==CombatKind.BuffApply)buffs.Add(e.Effect);
                if(e.Kind==CombatKind.BuffRemove)buffs.Remove(e.Effect);
            }
            // Only local offensive actions establish the active target. Late events
            // cannot move the assistant back to a previous target.
            if(e.SourceId==playerId && (e.Kind is CombatKind.Damage or CombatKind.Cast)
                && e.TargetId!=0 && e.TargetId!=playerId && e.Utc>=lastTargetActionUtc)
            {
                if(targetId!=e.TargetId) {ClearTarget();targetId=e.TargetId;}
                lastTargetActionUtc=e.Utc;
            }
            if(targetId!=0 && e.TargetId==targetId && !string.IsNullOrWhiteSpace(e.Effect))
            {
                if(e.Kind==CombatKind.DebuffApply)debuffs.Add(e.Effect);
                if(e.Kind==CombatKind.DebuffRemove)
                {
                    debuffs.Remove(e.Effect);
                    targetEffectRemovals[e.Effect]=e.Utc;
                }
            }
            if(e.Kind==CombatKind.TargetHp && e.TargetId==targetId && targetId!=0 && e.MaxHp>0)
                target=new(e.Target,e.CurrentHp,e.MaxHp,Math.Clamp(e.CurrentHp*100.0/e.MaxHp,0,100),0) {EntityId=targetId};
            if(e.SourceId!=playerId)return;
            if(e.Kind is CombatKind.Damage or CombatKind.Heal or CombatKind.Cast)
                if(!string.IsNullOrWhiteSpace(e.Skill))
                {
                    // Retain the newest passive timestamp across late delivery.
                    if(lastSkillUse.TryGetValue(e.Skill,out var previousUse) && previousUse>e.Utc)return;
                    lastSkillUse[e.Skill]=e.Utc;
                    if(targetId!=0 && e.TargetId==targetId && (e.Kind is CombatKind.Damage or CombatKind.Cast))
                        targetSkillUse[e.Skill]=e.Utc;
                    // A directly observed Judgment consumes the shield opportunity.
                    if(playerClass==AionClass.Templar && string.Equals(e.Skill,"Judgment",StringComparison.OrdinalIgnoreCase))
                    {
                        judgmentWindowUntil=null;
                        judgmentTrigger="";
                    }
                    if(playerClass==AionClass.Assassin && e.Kind==CombatKind.Damage && e.DamageFlags.HasFlag(DamageFlags.Critical))
                        criticalHitWindowUntil=e.Utc.AddSeconds(2);
                    if(playerClass==AionClass.Templar && JudgmentWindowSeconds.TryGetValue(e.Skill,out var seconds))
                    {
                        judgmentWindowUntil=e.Utc.AddSeconds(seconds);
                        judgmentTrigger=e.Skill;
                    }
                }

        }
    }

    public PassiveRotationObservation Snapshot()
    {
        lock(gate)return new(playerId,playerClass,
            new Dictionary<string,DateTime>(lastSkillUse,StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(buffs,StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(debuffs,StringComparer.OrdinalIgnoreCase),
            judgmentWindowUntil,judgmentTrigger,criticalHitWindowUntil)
            {
                TargetId=targetId,
                Target=target,
                TargetSkillUse=new Dictionary<string,DateTime>(targetSkillUse,StringComparer.OrdinalIgnoreCase),
                TargetEffectRemovals=new Dictionary<string,DateTime>(targetEffectRemovals,StringComparer.OrdinalIgnoreCase)
            };
    }

    public void Reset()
    {
        lock(gate)
        {
            ClearTarget();playerId=0;playerClass=null;lastSkillUse.Clear();buffs.Clear();debuffs.Clear();judgmentWindowUntil=null;judgmentTrigger="";criticalHitWindowUntil=null;
        }
    }

    void ClearTarget()
    {
        target=null;targetId=0;lastTargetActionUtc=DateTime.MinValue;targetSkillUse.Clear();targetEffectRemovals.Clear();debuffs.Clear();
    }

    static readonly IReadOnlyDictionary<string,double> JudgmentWindowSeconds =
        new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase)
        {
            ["Shield Smite"]=2,
            ["Warding Strike"]=2,
            ["Doom Shield"]=3,
            ["Shield Rush"]=2
        };
}

public sealed record PassiveRotationObservation(long PlayerId,AionClass? ClassName,
    IReadOnlyDictionary<string,DateTime> LastSkillUse,IReadOnlySet<string> Buffs,IReadOnlySet<string> Debuffs,
    DateTime? JudgmentWindowUntil=null,string JudgmentTrigger="",DateTime? CriticalHitWindowUntil=null)
{
    public long TargetId {get;init;}
    public TargetStats? Target {get;init;}
    // Null supports standalone fixtures with explicitly supplied current-target
    // history. Live snapshots always supply a separate target-scoped dictionary.
    public IReadOnlyDictionary<string,DateTime>? TargetSkillUse {get;init;}
    public IReadOnlyDictionary<string,DateTime> TargetEffectRemovals {get;init;}=new Dictionary<string,DateTime>();
    public bool UsedRecentlyOnTarget(string skill,DateTime utc,double seconds,string effect)
    {
        var uses=TargetSkillUse??LastSkillUse;
        return uses.TryGetValue(skill,out var used) && utc>=used && utc-used<=TimeSpan.FromSeconds(seconds)
            && (!TargetEffectRemovals.TryGetValue(effect,out var removed) || removed<used);
    }

    public bool JudgmentWindowActive(DateTime utc)=>JudgmentWindowUntil is DateTime until && utc<=until;
    public bool CriticalHitWindowActive(DateTime utc)=>CriticalHitWindowUntil is DateTime until && utc<=until;
    /// <summary>One observed opener grants one continuation. Equal timestamps fail
    /// closed because coarse capture clocks cannot prove a fresh activation.</summary>
    public bool PendingFollowUp(string opener,string followUp,DateTime utc,double seconds)=>
        UsedRecently(opener,utc,seconds)
        && (!LastSkillUse.TryGetValue(followUp,out var consumed) || consumed<LastSkillUse[opener]);

    public bool UsedRecently(string skill,DateTime utc,double seconds)=>
        LastSkillUse.TryGetValue(skill,out var used) && utc-used>=TimeSpan.Zero && utc-used<=TimeSpan.FromSeconds(seconds);
}
