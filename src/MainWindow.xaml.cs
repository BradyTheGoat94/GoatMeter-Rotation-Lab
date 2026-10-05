using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using Aion2DPSPro.Capture;
using Aion2DPSPro.Overlay;
using Aion2DPSPro.Protocol;
using Aion2DPSPro.Storage;
using Aion2DPSPro.Rotation;

namespace Aion2DPSPro;

public partial class MainWindow : Window
{
    readonly CombatEngine engine=new();
    readonly DispatcherTimer timer=new() {Interval=TimeSpan.FromMilliseconds(250)};
    readonly OverlayWindow overlay=new();
    readonly LiveCaptureAdapter capture;
    readonly object diagnosticsGate=new();
    readonly Queue<string> inspector=new();
    long packets,decoderDiagnostics,recognized,parsedEvents,duplicates,damageEvents,healEvents,hpEvents,otherEvents,acceptedDamageEvents,snapshotId;
    string activeFlow="Waiting...",captureStatus="Starting capture...",lastDecoder="",lastEvent="",profileId="",validationPath="",historyError="";
    Task saveTail=Task.CompletedTask;
    readonly FightStore store=new();
    readonly RotationEngine rotationEngine=new();
    readonly PassiveRotationStateTracker rotationTracker=new();
    readonly IReadOnlyList<RotationProfile> rotationProfiles=RotationProfileCatalog.CreateUnvalidatedGlobalStubs();
    public MainWindow()
    {
        InitializeComponent();
        overlay.Engine=engine;
        overlay.SnapshotProvider=(segment,category)=>engine.Snapshot(segment,category);
        overlay.HistoryProvider=()=>store.List().ToArray();
        overlay.HistoryLoader=path=>store.LoadAsync(path);
        var path=System.IO.Path.Combine(AppContext.BaseDirectory,"Protocol","Profiles","global-current.json");
        ProtocolProfile profile;
        try {profile=ProtocolProfile.Load(path);} catch {profile=ProtocolProfile.SafeGlobalScaffold();}
        var decoder=new CurrentClientDecoder(profile); profileId=decoder.ProfileId;
        var validationDir=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Aion2DPSPro","Validation");
        System.IO.Directory.CreateDirectory(validationDir);
        validationPath=System.IO.Path.Combine(validationDir,$"combat-{DateTime.Now:yyyyMMdd-HHmmss}.log");
        int verboseValidationCount=0;
        var validationGate=new object();
        void RecordValidation(string line)
        {
            var tag=line.Split('|').FirstOrDefault(x=>x.StartsWith("tag="))??"tag=other";
            // Discovery packets are sampled; combat, HP, identity and ownership
            // evidence must continue through the entire session and boss kill.
            bool verbose=tag is "tag=identityLifecycle" or "tag=entityBridge" or "tag=targetHpCandidate";
            lock(validationGate)
            {
                if(verbose && verboseValidationCount>=50)return;
                try
                {
                    System.IO.File.AppendAllText(validationPath,line+Environment.NewLine);
                    if(verbose)verboseValidationCount++;
                }
                catch(Exception ex) {lock(diagnosticsGate)captureStatus=$"Validation logging failed: {ex.Message}";}
            }
        }
        decoder.ValidationRecord+=RecordValidation;
        decoder.Diagnostic += d => {lock(diagnosticsGate) {decoderDiagnostics++;if(d.Stage=="parse")recognized++;lastDecoder=$"[{d.Stage}] {d.Message} ({d.Bytes} bytes)";}};
        engine.EncounterCompleted += snapshot =>
        {
            // Serialize writes, preserving ordering and reporting failures without stopping capture.
            lock(diagnosticsGate) saveTail=saveTail.ContinueWith(async _=>
            {
                try {await store.SaveAsync(snapshot);} catch(Exception ex) {lock(diagnosticsGate) historyError=$"History save failed: {ex.Message}";}
            }).Unwrap();
        };
        capture=new LiveCaptureAdapter(decoder);
        capture.ValidationRecord+=RecordValidation;
        capture.PacketCaptured += ()=>Interlocked.Increment(ref packets);
        capture.DuplicateSuppressed += ()=>Interlocked.Increment(ref duplicates);
        capture.FlowLocked += s=> {lock(diagnosticsGate)activeFlow=s;};
        capture.ConnectionReset += ()=>engine.Apply(new(DateTime.UtcNow,CombatKind.Zone));
        capture.StatusChanged += s=> {lock(diagnosticsGate)captureStatus=s;};
        capture.EventReceived += e=>
        {
            engine.Apply(e);
            rotationTracker.Observe(e);
            lock(diagnosticsGate)
            {
                parsedEvents++;
                lastEvent=$"kind={e.Kind} amount={e.Amount} src={e.SourceId} name={e.Source} tgt={e.TargetId} skill={e.Skill} type={e.DamageType}";
                if(e.Kind==CombatKind.Damage) {damageEvents++;if(e.Amount>0)acceptedDamageEvents++;}
                else if(e.Kind==CombatKind.Heal)healEvents++;
                else if(e.Kind==CombatKind.TargetHp)hpEvents++;
                else otherEvents++;
                inspector.Enqueue($"{e.Utc:HH:mm:ss.fff} {lastEvent}");while(inspector.Count>12)inspector.Dequeue();
            }
        };
        timer.Tick += (_,_)=>Render(); timer.Start(); capture.Start();
    }
    void OpenOverlay(object sender,RoutedEventArgs e) {if(!overlay.IsVisible)overlay.Show();overlay.Activate();}
    void Reset(object sender,RoutedEventArgs e)=>engine.ResetFight();
    void Render()
    {
        var s=engine.Snapshot();
        snapshotId++; SnapshotState.Text=$"Snapshot #{snapshotId:N0} | FightDamage={s.FightDamage:N0} | FightDPS={s.FightDps:0.##}";
        Dps.Text=$"{s.FightDps:0} DPS"; Damage.Text=$"{s.FightDamage:N0} damage";
        Target.Text=s.Target is null?"No target":$"{s.Target.Name} {s.Target.Percent:0.0}% {s.Target.CurrentHp:N0}/{s.Target.MaxHp:N0}";
        Grid.ItemsSource=s.Players.Select((p,i)=>new {Rank=i+1,p.Name,Dps=p.Dps.ToString("N0"),Damage=p.Damage.ToString("N0"),Share=$"{p.Share:0.0}%"}).ToArray();
        overlay.Render(s);
        // Lab-only passive bridge: derive only facts present in the meter snapshot.
        // Unknown build/readiness/resource/movement state deliberately keeps all
        // eight profiles fail-closed until stronger passive evidence is validated.
        var observed=rotationTracker.Snapshot();
        var self=s.Players.FirstOrDefault(p=>p.EntityId==observed.PlayerId);
        if(self is null || observed.ClassName is not AionClass observedClass)
            overlay.RenderRotation(new RotationDecision(null,Array.Empty<SkillRecommendation>(),"Waiting for confirmed player class from passive combat."));
        else
        {
            var profile=observedClass switch
            {
                AionClass.Templar=>RotationProfileCatalog.CreateProvisionalTemplarSingleTarget(),
                AionClass.Assassin=>RotationProfileCatalog.CreateProvisionalAssassinSingleTarget(),
                AionClass.Gladiator=>RotationProfileCatalog.CreateProvisionalGladiatorSingleTarget(),
                AionClass.Ranger=>RotationProfileCatalog.CreateProvisionalRangerSingleTarget(),
                AionClass.Sorcerer=>RotationProfileCatalog.CreateProvisionalSorcererSingleTarget(),
                AionClass.Spiritmaster=>RotationProfileCatalog.CreateProvisionalSpiritmasterSingleTarget(),
                AionClass.Cleric=>RotationProfileCatalog.CreateProvisionalClericSingleTarget(),
                AionClass.Chanter=>RotationProfileCatalog.CreateProvisionalChanterSingleTarget(),
                _=>rotationProfiles.First(p=>p.ClassName==observedClass)
            };
            var targetHp=s.Target?.Percent??100;
            var now=DateTime.UtcNow;
            var cooldowns=ValidatedCooldownCatalog.Remaining(observed,observedClass,now);
            var signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if(observed.JudgmentWindowActive(now))signals.Add("JudgmentWindow");
            if(observed.CriticalHitWindowActive(now))signals.Add("CriticalHitWindow");
            // A confirmed local skill observation proves combat activity without
            // guessing any hidden game state. It may unlock only generic sustained
            // damage/filler rules; proc, chain, buff and burst rules stay gated.
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
            // Sequence windows below use only the observed local skill history. They
            // do not infer hidden buffs/stacks: an exact preceding action is required.
            if(observedClass==AionClass.Gladiator)
            {
                if(observed.UsedRecently("Rending Blow",now,3))
                    signals.Add("GladiatorSmashingWindow");
                if(observed.UsedRecently("Keen Strike",now,3))
                    signals.Add("GladiatorRuptureWindow");
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
            var state=new RotationState(now,observedClass,profile.BuildId,profile.Mode,
                cooldowns,observed.Buffs,observed.Debuffs,0,targetHp,1,false,
                s.Target is not null && (s.Target.MaxHp<=0 || s.Target.CurrentHp>0),0.25)
                {Signals=signals};
            var decision=rotationEngine.Evaluate(state,profile);
            var readiness=cooldowns.Count==0
                ?"Cooldown readiness: insufficient validated observations"
                :"Cooldown readiness: "+string.Join(" • ",cooldowns.OrderBy(x=>x.Key).Select(x=>$"{x.Key} {(x.Value<=0?"READY":$"{x.Value:0.0}s")}"));
            overlay.RenderRotation(decision with {ReadinessContext=readiness});
        }
        lock(diagnosticsGate)
        {
            Status.Text=$"{captureStatus} | {capture.Health}"+(historyError.Length>0?$" | {historyError}":"");
            PacketCount.Text=Interlocked.Read(ref packets).ToString("N0"); DuplicateCount.Text=Interlocked.Read(ref duplicates).ToString("N0");
            DiagnosticCount.Text=decoderDiagnostics.ToString("N0"); RecognizedCount.Text=recognized.ToString("N0"); EventCount.Text=parsedEvents.ToString("N0");
            AcceptedDamageCount.Text=acceptedDamageEvents.ToString("N0"); DamageEventCount.Text=damageEvents.ToString("N0"); HealEventCount.Text=healEvents.ToString("N0");
            HpEventCount.Text=hpEvents.ToString("N0"); OtherEventCount.Text=otherEvents.ToString("N0"); ActiveFlow.Text=activeFlow; Profile.Text=profileId;
            LastDecoder.Text=lastDecoder; LastEvent.Text=LatestEvent.Text=lastEvent; EventInspector.Text=string.Join(Environment.NewLine,inspector.Reverse()); ValidationFile.Text=validationPath;
        }
    }
    bool allowClose,closing;
    protected override async void OnClosing(CancelEventArgs e)
    {
        if(allowClose) {base.OnClosing(e);return;}
        e.Cancel=true;base.OnClosing(e);
        if(closing)return;
        closing=true;timer.Stop();capture.Dispose();
        try
        {
            await capture.Completion;
            engine.ResetFight();
            Task pending;lock(diagnosticsGate)pending=saveTail;
            await pending;
            if(historyError.Length>0)MessageBox.Show(this,historyError,"History was not saved");
        }
        catch(Exception ex) {MessageBox.Show(this,ex.Message,"Unable to finish saving history");}
        overlay.Close();allowClose=true;Close();
    }
}
