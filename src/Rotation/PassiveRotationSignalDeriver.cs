namespace Aion2DPSPro.Rotation;

/// <summary>
/// Derives recommendation signals only from facts already observed passively.
/// Generic combat activity may unlock sustained filler; exact sequence windows
/// require the observed local class and prerequisite skill.
/// </summary>
public static class PassiveRotationSignalDeriver
{
    public static HashSet<string> Derive(PassiveRotationObservation observed,AionClass observedClass,DateTime now)
    {
        var signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if(observed.JudgmentWindowActive(now))signals.Add("JudgmentWindow");
        if(observed.CriticalHitWindowActive(now))signals.Add("CriticalHitWindow");

        bool observedCombatAction=observed.LastSkillUse.Values.Any(used=>now-used>=TimeSpan.Zero && now-used<=TimeSpan.FromSeconds(8));
        if(observedCombatAction)
        {
            signals.Add("AssassinFillerWindow");
            signals.Add("GladiatorFillerWindow");
            signals.Add("RangerFillerWindow");
            signals.Add("SorcererFillerWindow");
            signals.Add("SpiritmasterFillerWindow");
            signals.Add("ClericFillerWindow");
            signals.Add("ChanterFillerWindow");
            signals.Add("TemplarFillerWindow");
        }

        if(observedClass==AionClass.Gladiator)
        {
            if(observed.UsedRecently("Rending Blow",now,3))signals.Add("GladiatorSmashingWindow");
            if(observed.UsedRecently("Keen Strike",now,3))signals.Add("GladiatorRuptureWindow");
            if(observed.UsedRecently("Rupture Strike",now,3))signals.Add("GladiatorWrathfulWindow");
        }
        if(observedClass==AionClass.Chanter && observed.UsedRecently("Impactful Crush",now,3))
            signals.Add("ChanterDarkCrushWindow");
        if(observedClass==AionClass.Spiritmaster)
        {
            if(observed.UsedRecently("Flame Blessing",now,8)||observed.UsedRecently("Spirit's Benediction",now,8))
                signals.Add("SpiritmasterAncientWindow");
            if(observed.UsedRecently("Summon: Ancient Spirit",now,8))
                signals.Add("SpiritmasterCorrodeWindow");
        }
        return signals;
    }
}
