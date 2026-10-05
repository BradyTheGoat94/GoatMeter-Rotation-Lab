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
            // Generic activity is evidence only for the passively resolved local class.
            // Never broadcast filler readiness across classes: an unresolved/mismatched
            // class must fail closed instead of manufacturing recommendation state.
            var fillerSignal=observedClass switch
            {
                AionClass.Assassin => "AssassinFillerWindow",
                AionClass.Gladiator => "GladiatorFillerWindow",
                AionClass.Ranger => "RangerFillerWindow",
                AionClass.Sorcerer => "SorcererFillerWindow",
                AionClass.Spiritmaster => "SpiritmasterFillerWindow",
                AionClass.Cleric => "ClericFillerWindow",
                AionClass.Chanter => "ChanterFillerWindow",
                AionClass.Templar => "TemplarFillerWindow",
                _ => null
            };
            if(fillerSignal is not null)signals.Add(fillerSignal);
        }

        if(observedClass==AionClass.Gladiator)
        {
            if(observed.UsedRecently("Rending Blow",now,3))signals.Add("GladiatorSmashingWindow");
            if(observed.UsedRecently("Keen Strike",now,3))signals.Add("GladiatorRuptureWindow");
            if(observed.UsedRecently("Rupture Strike",now,3))signals.Add("GladiatorWrathfulWindow");
        }
        if(observedClass==AionClass.Assassin)
        {
            if(observed.UsedRecently("Quick Slice",now,3))signals.Add("AssassinBreakingSliceWindow");
            if(observed.UsedRecently("Breaking Slice",now,3))signals.Add("AssassinSwiftSliceWindow");
            if(observed.UsedRecently("Savage Roar",now,3))signals.Add("AssassinSavageBackKickWindow");
            if(observed.UsedRecently("Savage Back Kick",now,3))signals.Add("AssassinSavageSmashWindow");
        }
        if(observedClass==AionClass.Sorcerer && observed.UsedRecently("Ice Chain",now,3))
            signals.Add("SorcererColdWaveWindow");
        if(observedClass==AionClass.Cleric && observed.UsedRecently("Chain of Torment",now,10))
            signals.Add("ClericCondemnationWindow");
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
