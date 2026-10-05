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
        if(observedClass==AionClass.Ranger)
        {
            // Precision must be observed, never inferred from generic activity.
            // Current Global evidence makes Deadshot the immediate high-value payoff.
            if(observed.Buffs.Contains("Precision"))
                signals.Add("RangerDeadshotWindow");
        }
        if(observedClass==AionClass.Sorcerer && observed.UsedRecently("Ice Chain",now,3))
            signals.Add("SorcererColdWaveWindow");
        if(observedClass==AionClass.Cleric)
        {
            // Condemnation itself requires Chain of Torment on the target.
            if(observed.UsedRecently("Chain of Torment",now,10))
                signals.Add("ClericCondemnationWindow");

            // Earth's Punishment is a separate, directly observable target state.
            // It materially changes the Condemnation loop in current Global builds,
            // but specialty-dependent guaranteed-crit/reset behavior is not inferred here.
            if(observed.Debuffs.Contains("Earth Punishment"))
                signals.Add("ClericEarthPunishmentWindow");
        }
        if(observedClass==AionClass.Chanter)
        {
            // Current Global Season 1 changed Dark Crush to activate after a ranged skill.
            // Spinning Strike is an observed ranged setup in the current PvE loop.
            // Retain the older Impactful Crush path only as provisional corroboration until
            // live Global observations can conclusively retire it.
            if(observed.UsedRecently("Spinning Strike",now,3))
                signals.Add("ChanterDarkCrushWindow");
            else if(observed.UsedRecently("Impactful Crush",now,3))
                signals.Add("ChanterDarkCrushWindow");
        }
        if(observedClass==AionClass.Spiritmaster)
        {
            if(observed.UsedRecently("Flame Blessing",now,8)||observed.UsedRecently("Spirit's Benediction",now,8))
                signals.Add("SpiritmasterAncientWindow");
            if(observed.UsedRecently("Summon: Ancient Spirit",now,8))
                signals.Add("SpiritmasterCorrodeWindow");

            // Elemental Fusion is enabled by the four-element state. Consume only a
            // passively observed state name; never infer stacks from generic combat.
            if(observed.Buffs.Contains("Four Elements"))
                signals.Add("SpiritmasterBurstWindow");
        }
        return signals;
    }
}
