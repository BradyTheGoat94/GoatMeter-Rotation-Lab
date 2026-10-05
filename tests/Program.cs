using Aion2DPSPro;
using Aion2DPSPro.Protocol;
using Aion2DPSPro.Storage;
using Aion2DPSPro.Rotation;

int checks=0;
void Equal(double actual,double expected,string label) {checks++;if(Math.Abs(actual-expected)>.00001)throw new Exception($"{label}: expected {expected}, got {actual}");}
void True(bool actual,string label) {checks++;if(!actual)throw new Exception(label);}
var t=new DateTime(2026,10,3,0,0,0,DateTimeKind.Utc);
var now=t;
var e=new CombatEngine(()=>now);
CombatEvent Hit(int seconds,long id,long amount,string name="Player",bool boss=false)=>new(t.AddSeconds(seconds),CombatKind.Damage,id,name,99,"Boss","Strike",amount,IsBoss:boss);
e.Apply(new(t,CombatKind.PlayerName,1,"Player"));e.Apply(Hit(0,1,100));e.Apply(Hit(5,1,200));
Equal(e.Snapshot().FightDamage,300,"damage sum");Equal(e.Snapshot().FightDps,60,"encounter DPS");
now=t.AddSeconds(7);Equal(e.Snapshot().FightDps,60,"DPS frozen during idle");
e.Apply(new(t.AddSeconds(2),CombatKind.BuffApply,1,"Player",1,"Player",Effect:"Power",Stacks:1));
e.Apply(new(t.AddSeconds(6),CombatKind.BuffRemove,1,"Player",1,"Player",Effect:"Power"));
// Late timestamps must not reset encounter activity.
now=t.AddSeconds(13);True(!e.Snapshot().InFight,"timeout completes");Equal(e.Snapshot(MeterSegment.Previous,MeterCategory.Damage).FightDamage,300,"completed preserved");Equal(e.History.Count,1,"single completion");
e.Apply(new(t.AddSeconds(50),CombatKind.PlayerName,2,"Player"));e.Apply(Hit(50,2,400,"Player"));Equal(e.Snapshot(MeterSegment.Previous,MeterCategory.Damage).FightDamage,300,"previous fight");
Equal(e.Snapshot(MeterSegment.Overall,MeterCategory.Damage).FightDamage,700,"overall sum");
e.Apply(new(t.AddSeconds(51),CombatKind.Heal,2,"Player",1,"Player","Heal",50));Equal(e.Snapshot(MeterSegment.Current,MeterCategory.Healing).Players.Single().Damage,50,"healing independent");
Equal(e.Snapshot(MeterSegment.Current,MeterCategory.DamageTaken).Players.Single().Damage,400,"damage taken independent");
e.Apply(new(t.AddSeconds(52),CombatKind.Death,TargetId:2));Equal(e.Snapshot(MeterSegment.Current,MeterCategory.Deaths).Players.Single().Damage,1,"death target");
e.Apply(new(t.AddSeconds(53),CombatKind.Interrupt,2));Equal(e.Snapshot(MeterSegment.Current,MeterCategory.Interrupts).Players.Single().Damage,1,"interrupt count");
e.Apply(new(t.AddSeconds(54),CombatKind.Dispel,2));Equal(e.Snapshot(MeterSegment.Current,MeterCategory.Dispels).Players.Single().Damage,1,"dispel count");
var b=new CombatEngine(()=>t.AddSeconds(7));b.Apply(Hit(0,1,100));b.Apply(new(t.AddSeconds(2),CombatKind.BuffApply,1,"Player",1,"Player",Effect:"Power",Stacks:2));b.Apply(new(t.AddSeconds(4),CombatKind.BuffRemove,1,"Player",1,"Player",Effect:"Power"));b.Apply(Hit(5,1,100));
Equal(b.Snapshot().Buffs.Single().Uptime,40,"buff actual fight denominator");
b.Apply(new(t.AddSeconds(6),CombatKind.PlayerName,1,"Resolved",SourceClass:"Cleric"));b.Apply(new(t.AddSeconds(6),CombatKind.PlayerName,2,"Player"));b.Apply(Hit(6,2,300,"Player"));Equal(b.Snapshot().Players.Count,2,"same names remain separate");Equal(b.Snapshot().Skills.Count,2,"skills per actor");
True(b.Snapshot().Players.Any(p=>p.Name=="Resolved"&&p.ClassName=="Cleric"&&p.Damage==200),"late identity");
b.Apply(new(t.AddSeconds(6),CombatKind.Despawn,1));b.Apply(new(t.AddSeconds(7),CombatKind.PlayerName,1,"Replacement"));b.Apply(Hit(7,1,50,"Replacement"));True(b.Snapshot().Players.Any(p=>p.Name=="Resolved"&&p.Damage==200),"reused ID history preserved");True(b.Snapshot().Players.Any(p=>p.Name=="Replacement"&&p.Damage==50),"reused ID new actor");
var pet=new CombatEngine(()=>t);pet.Apply(new(t,CombatKind.PlayerName,1,"Owner"));pet.Apply(new(t,CombatKind.Ownership,5,OwnerId:1));pet.Apply(Hit(0,5,90,"Summon"));True(pet.Snapshot().Players.Single().Name=="Owner","explicit pet ownership");
var bossNow=t;var bossEngine=new CombatEngine(()=>bossNow);bossEngine.Apply(new(t,CombatKind.PlayerName,1,"Player"));bossEngine.Apply(Hit(0,1,100,boss:true));bossNow=t.AddSeconds(7);True(bossEngine.Snapshot().InFight,"boss retained inside inactivity window");bossEngine.Apply(new(t.AddSeconds(7),CombatKind.TargetHp,TargetId:99,CurrentHp:0,MaxHp:1000,IsBoss:true));True(!bossEngine.Snapshot().InFight,"boss death completes");
var tcp=new TcpStreamReassembler();
Equal(tcp.Push("a",100,new byte[]{1,2,3},t).SelectMany(x=>x).Count(),3,"initial TCP");
Equal(tcp.Push("a",102,new byte[]{3,4,5},t).SelectMany(x=>x).Count(),2,"overlap keeps new suffix");
Equal(tcp.Push("a",102,new byte[]{3,4,5},t).Count,0,"duplicate discarded");
Equal(tcp.Push("a",108,new byte[]{9},t).Count,0,"out of order waits");Equal(tcp.Push("a",105,new byte[]{6,7,8},t).SelectMany(x=>x).Count(),4,"gap filled flushes");
int resets=0;tcp.StreamReset+=_=>resets++;tcp.Push("a",120,new byte[]{1},t);tcp.Push("a",121,new byte[]{2},t.AddSeconds(6));True(resets>0,"gap recovery reset");
Equal(tcp.Push("wrap",uint.MaxValue-1,new byte[]{1,2,3},t).Count,1,"sequence wrap first");Equal(tcp.Push("wrap",1,new byte[]{4},t).Count,1,"sequence wrap continuation");
var profile=new ProtocolProfile("fixture","Global","fixture",13328,new Dictionary<string,PacketTag>{{"damage",new(4,56)},{"entityRemoved",new(33,141)}});
// Observed frame regressions, with logged values independently encoded here. These are NOT a ground-truth fight total.
var observed=new[]{("210438E3A00204008D3540B7B70009020B95C34701000000D658E7020100",359L),("210438E3A00204008D3540B7B70009021595C34702000000D658E8020200",360L),("240438E3A00224008D3540B7B70009021595C34703000000D65888030203030300",392L),("280438E3A00226008D351042B7000B020800004BCE954701000000D658D50F031414140100",2005L)};
foreach(var (hex,amount) in observed)
{
 var decoder=new CurrentClientDecoder(profile);var evt=decoder.Decode(Convert.FromHexString(hex),t).Single(x=>x.Kind==CombatKind.Damage);Equal(evt.Amount,amount,"observed damage replay");Equal(evt.SourceId,6797,"observed actor");Equal(evt.TargetId,36963,"observed target");
 var split=new CurrentClientDecoder(profile);var bytes=Convert.FromHexString(hex);True(!split.DecodeStream("in",bytes[..8],t).Any(),"split frame waits");Equal(split.DecodeStream("in",bytes[8..],t).Single(x=>x.Kind==CombatKind.Damage).Amount,amount,"split frame replay");
}
var directions=new CurrentClientDecoder(profile);var f=Convert.FromHexString(observed[0].Item1);directions.DecodeStream("in",f[..8],t).ToArray();directions.DecodeStream("out",f[..5],t).ToArray();Equal(directions.DecodeStream("in",f[8..],t).Single().Amount,359,"directions isolated");
var removal=new CurrentClientDecoder(profile).Decode(Convert.FromHexString("0B218DED8A010000"),t).Single();True(removal.Kind==CombatKind.Despawn,"observed removal");Equal(removal.SourceId,17773,"observed removal ID");
Equal(Lz4BlockDecoder.Decompress(new byte[]{0,0,0},new byte[10]),-1,"bad LZ4 rejects");
var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));try {var store=new FightStore(folder);await store.SaveAsync(b.Snapshot());var saved=await store.LoadAsync(store.List().Single());Equal(saved!.FightDamage,b.Snapshot().FightDamage,"history roundtrip");Equal(saved.Skills.Count,b.Snapshot().Skills.Count,"history skill roundtrip");}finally{Directory.Delete(folder,true);}
var reuseRow=b.Snapshot().Players.Single(p=>p.Name=="Replacement");Equal(b.Snapshot().RecentEvents.Where(x=>x.Kind==CombatKind.Damage&&x.SourceId==reuseRow.ActorId).Sum(x=>x.Amount),50,"reused actor report events isolated");
var savedSnapshot=e.Snapshot();e.ResetFight();True(e.History.Last().Categories[MeterCategory.Healing].Players.Count>0,"completed history retains healing dataset");
// Sanitized identity prefixes observed in the new log: no private names or inventory data.
byte[] Frame(string bodyHex)
{
 var body=Convert.FromHexString(bodyHex);var length=new List<byte>();uint v=(uint)body.Length+4;
 do {byte part=(byte)(v&127);v>>=7;length.Add((byte)(part|(v>0?128:0)));}while(v>0);
 return length.Concat(body).ToArray();
}
var identityProfile=profile with {Tags=new Dictionary<string,PacketTag>(profile.Tags) {{"selfInfo",new(51,54)},{"otherInfo",new(69,54)}}};
var identityDecoder=new CurrentClientDecoder(identityProfile);
var self=identityDecoder.Decode(Frame("3336FD235F81C1283708546573744865726F000000"),t).Single(x=>x.Kind==CombatKind.PlayerName);
True(self.Source=="TestHero"&&self.SourceId==4605,"observed self identity prefix");
// Sanitized replay of selfInfo -> local entity removal -> damage in the 13:00 capture.
var localDecoder=new CurrentClientDecoder(identityProfile);
localDecoder.Decode(Frame("3336D10B5F81C1283708546573744865726F000000"),t).ToArray();
True(!localDecoder.Decode(Convert.FromHexString("0A218DD10B0001"),t.AddSeconds(3)).Any(x=>x.Kind==CombatKind.Despawn),"local removal must not invalidate self identity");
var localHit=localDecoder.Decode(Convert.FromHexString("230438E3A0022400D10B40B7B70001020B95C34701000000D658F10201030100"),t.AddSeconds(3)).Single(x=>x.Kind==CombatKind.Damage);
True(localHit.SourceId==1489&&localHit.Source=="TestHero","captured local removal sequence retains character name");
True(localDecoder.Decode(Convert.FromHexString("0B218DED8A010000"),t.AddSeconds(4)).Any(x=>x.Kind==CombatKind.Despawn),"other entities still despawn");
localDecoder.ResetConnection();
True(localDecoder.Decode(Convert.FromHexString("0A218DD10B0001"),t.AddSeconds(5)).Any(x=>x.Kind==CombatKind.Despawn),"reset clears protected self identity");
var ally=identityDecoder.Decode(Frame("4536A2210120A401070854657374416C6C7913000000"),t).Last(x=>x.Kind==CombatKind.PlayerName);
True(ally.Source=="TestAlly"&&ally.SourceId==4258,"observed other identity prefix");
True(!identityDecoder.Decode(Frame("218DA2210001"),t.AddSeconds(1)).Any(x=>x.Kind==CombatKind.Despawn),"known ally visibility removal preserves identity");
var allyDamage=identityDecoder.Decode(Convert.FromHexString("210438E3A0020400A22140B7B70009020B95C34701000000D658E7020100"),t.AddSeconds(2)).Single(x=>x.Kind==CombatKind.Damage);
True(allyDamage.Source=="TestAlly","ally name remains after visibility removal");
var nameReuse=new CombatEngine(()=>t);
nameReuse.Apply(new(t,CombatKind.PlayerName,4258,"FirstPlayer"));
nameReuse.Apply(Hit(0,4258,100,"FirstPlayer"));
nameReuse.Apply(new(t.AddSeconds(1),CombatKind.PlayerName,4258,"SecondPlayer"));
nameReuse.Apply(Hit(2,4258,50,"SecondPlayer"));
True(nameReuse.Snapshot().Players.Any(p=>p.Name=="FirstPlayer"&&p.Damage==100),"explicit name reuse preserves old totals");
True(nameReuse.Snapshot().Players.Any(p=>p.Name=="SecondPlayer"&&p.Damage==50),"explicit name reuse separates new totals");
var bridge=new Aion2DPSPro.Capture.CaptureIdentityBridge();string scope="adapter|local|server";
bridge.Observe(scope,new(t,CombatKind.PlayerName,self.SourceId,self.Source));bridge.Observe(scope,new(t,CombatKind.PlayerName,ally.SourceId,ally.Source));
var bridged=new CombatEngine(()=>t);
foreach(var identity in bridge.Identities(scope,t))bridged.Apply(identity);
bridged.Apply(bridge.Resolve(scope,Hit(0,4605,100,"Actor 4605")));bridged.Apply(bridge.Resolve(scope,Hit(0,4258,50,"Actor 4258")));
True(bridged.Snapshot().Players.Any(p=>p.Name=="TestHero"&&p.EntityId==4605),"pre-lock self identity reaches combat rows");
True(bridged.Snapshot().Players.Any(p=>p.Name=="TestAlly"&&p.EntityId==4258),"sibling identity reaches exact actor");
True(bridge.Resolve(scope,Hit(0,4605,100,"FreshName")).Source=="FreshName","bridge preserves explicitly decoded names");
True(bridge.Resolve("adapter|local|different-server",Hit(0,4605,100,"Actor 4605")).Source=="Actor 4605","identity scope isolation");
bridge.Observe(scope,new(t,CombatKind.Despawn,4605));True(bridge.Resolve(scope,Hit(0,4605,100,"Actor 4605")).Source=="Actor 4605","bridge removes stale identity");
True(bridge.Identities(scope,t.AddMinutes(11)).Count==0,"identity cache expires");
var reportEngine=new CombatEngine(()=>t);reportEngine.Apply(new(t,CombatKind.PlayerName,1,"Tester",SourceClass:"Templar"));reportEngine.Apply(new(t,CombatKind.Damage,1,"Tester",2,"Dummy","One",100,DamageFlags:DamageFlags.Critical|DamageFlags.Back|DamageFlags.Perfect));reportEngine.Apply(new(t.AddSeconds(1),CombatKind.Heal,1,"Tester",1,"Tester","Mend",50));
var full=reportEngine.Snapshot(MeterSegment.Current,MeterCategory.Damage,true);Equal(full.Categories[MeterCategory.Healing].Skills.Single().Damage,50,"report retains healing skills");Equal(full.Skills.Single().FlagHits[DamageFlags.Perfect],1,"perfect counts");Equal(full.Targets.Single().Damage,100,"actor target totals");
reportEngine.ResetFight();reportEngine.Apply(Hit(10,1,999));Equal(reportEngine.SnapshotEncounter(full.EncounterId)!.FightDamage,100,"open report stays on its encounter");
True(PublicGameData.EnglishTargetLabel("Training Scarecrow")=="Training Scarecrow","English target label preserved");
True(PublicGameData.EnglishTargetLabel("노인 주민")==null,"untranslated Korean NPC falls back to English target label");
True(PublicGameData.EnglishTargetLabel("Boss 마족")==null,"mixed language target label rejected");
// Rotation Lab fail-closed safety regressions.
var stubs=RotationProfileCatalog.CreateUnvalidatedGlobalStubs();
Equal(stubs.Count,8,"all eight rotation class stubs");
True(stubs.All(x=>x.Validation==ProfileValidation.Unvalidated&&x.Rules.Count==0),"unvalidated profiles contain no guessed rules");
var rotationEngine=new RotationEngine();
var rotationState=new RotationState(t,AionClass.Templar,"global-unvalidated",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,1.0);
var emptyDecision=rotationEngine.Evaluate(rotationState,stubs.Single(x=>x.ClassName==AionClass.Templar));
True(emptyDecision.Next is null,"unvalidated empty profile yields no recommendation");
var validatedProfile=new RotationProfile(AionClass.Templar,"fixture",RotationMode.SingleTarget,ProfileValidation.Validated,
    new[]{new RotationRule("Fixture Strike",100,new[]{new RotationCondition(RotationConditionKind.CooldownReady,"Fixture Strike")})},
    "Regression fixture only");
var validatedState=rotationState with {BuildId="fixture",CooldownSeconds=new Dictionary<string,double>{{"Fixture Strike",0}},ObservationConfidence=.95};
var recommendation=rotationEngine.Evaluate(validatedState,validatedProfile);
True(recommendation.Next?.Skill=="Fixture Strike"&&recommendation.Next.Actionable,"validated high-confidence rule can recommend");
var lowConfidence=rotationEngine.Evaluate(validatedState with {ObservationConfidence=.4},validatedProfile);
True(lowConfidence.Next is not null&&!lowConfidence.Next.Actionable,"low-confidence observation is informational only");
var tracker=new PassiveRotationStateTracker();
tracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
tracker.Observe(new(t.AddSeconds(1),CombatKind.Cast,77,"Tester",Skill:"Observed Cast",SourceClass:"Templar"));
tracker.Observe(new(t.AddSeconds(2),CombatKind.Damage,77,"Tester",99,"Dummy","Observed Strike",100,SourceClass:"Templar"));
tracker.Observe(new(t.AddSeconds(3),CombatKind.BuffApply,77,"Tester",77,"Tester",Effect:"Observed Buff",SourceClass:"Templar"));
var observedRotation=tracker.Snapshot();
True(observedRotation.PlayerId==77&&observedRotation.ClassName==AionClass.Templar,"passive tracker resolves confirmed class");
True(observedRotation.LastSkillUse.ContainsKey("Observed Cast")&&observedRotation.LastSkillUse.ContainsKey("Observed Strike"),"passive tracker records observed skill use");
True(observedRotation.UsedRecently("Observed Strike",t.AddSeconds(4),3),"recent-skill helper accepts an observed local skill inside its evidence window");
True(!observedRotation.UsedRecently("Observed Strike",t.AddSeconds(6),3),"recent-skill helper expires an observed local skill after its evidence window");
True(!observedRotation.UsedRecently("Never Observed",t.AddSeconds(3),3),"recent-skill helper fails closed for an unobserved skill");
True(observedRotation.Buffs.Contains("Observed Buff"),"passive tracker records observed buff");
var fillerClasses=new[]{AionClass.Templar,AionClass.Gladiator,AionClass.Assassin,AionClass.Ranger,AionClass.Sorcerer,AionClass.Spiritmaster,AionClass.Cleric,AionClass.Chanter};
var fillerNames=new Dictionary<AionClass,string>
{
    [AionClass.Templar]="TemplarFillerWindow",[AionClass.Gladiator]="GladiatorFillerWindow",
    [AionClass.Assassin]="AssassinFillerWindow",[AionClass.Ranger]="RangerFillerWindow",
    [AionClass.Sorcerer]="SorcererFillerWindow",[AionClass.Spiritmaster]="SpiritmasterFillerWindow",
    [AionClass.Cleric]="ClericFillerWindow",[AionClass.Chanter]="ChanterFillerWindow"
};
foreach(var cls in fillerClasses)
{
    var activity=new PassiveRotationObservation(77,cls,
        new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Observed Combat Action",t}},
        new HashSet<string>(),new HashSet<string>());
    var signals=PassiveRotationSignalDeriver.Derive(activity,cls,t.AddSeconds(1));
    True(signals.Contains(fillerNames[cls]),$"{cls} observed combat emits its own filler window");
    True(fillerClasses.Where(other=>other!=cls).All(other=>!signals.Contains(fillerNames[other])),
        $"{cls} observed combat cannot leak filler evidence into another class");
}
tracker.Observe(new(t.AddSeconds(4),CombatKind.BuffRemove,77,"Tester",77,"Tester",Effect:"Observed Buff",SourceClass:"Templar"));
True(!tracker.Snapshot().Buffs.Contains("Observed Buff"),"passive tracker removes observed buff");
tracker.Observe(new(t.AddSeconds(5),CombatKind.Zone));
True(tracker.Snapshot().PlayerId==0&&tracker.Snapshot().LastSkillUse.Count==0,"passive tracker clears across zone");
var cooldownTracker=new PassiveRotationStateTracker();
cooldownTracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
cooldownTracker.Observe(new(t,CombatKind.Damage,77,"Tester",99,"Dummy","Punishment",100,SourceClass:"Templar"));
var knownCooldowns=ValidatedCooldownCatalog.Remaining(cooldownTracker.Snapshot(),AionClass.Templar,t.AddSeconds(12));
Equal(knownCooldowns["Punishment"],18,"validated Punishment cooldown reconstruction");
var rangerCooldownTracker=new PassiveRotationStateTracker();
rangerCooldownTracker.Observe(new(t,CombatKind.PlayerName,88,"RangerTester",SourceClass:"Ranger",SourceIdentityConfirmed:true));
rangerCooldownTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,88,"RangerTester",99,"Dummy","Drill Dart",100,SourceClass:"Ranger"));
var rangerCooldowns=ValidatedCooldownCatalog.Remaining(rangerCooldownTracker.Snapshot(),AionClass.Ranger,t.AddSeconds(4));
Equal(rangerCooldowns["Drill Dart"],2,"validated Drill Dart cooldown reconstruction");
var rangerCooldownReady=ValidatedCooldownCatalog.Remaining(rangerCooldownTracker.Snapshot(),AionClass.Ranger,t.AddSeconds(6));
Equal(rangerCooldownReady["Drill Dart"],0,"validated Drill Dart becomes ready after 5s");
var clericCooldownTracker=new PassiveRotationStateTracker();
clericCooldownTracker.Observe(new(t,CombatKind.PlayerName,89,"ClericTester",SourceClass:"Cleric",SourceIdentityConfirmed:true));
clericCooldownTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,89,"ClericTester",99,"Dummy","Condemnation",100,SourceClass:"Cleric"));
var clericCooldowns=ValidatedCooldownCatalog.Remaining(clericCooldownTracker.Snapshot(),AionClass.Cleric,t.AddSeconds(3));
Equal(clericCooldowns["Condemnation"],1,"validated base Condemnation cooldown reconstruction excludes specialty resets");
Equal(ValidatedCooldownCatalog.Remaining(clericCooldownTracker.Snapshot(),AionClass.Cleric,t.AddSeconds(4))["Condemnation"],0,"base Condemnation readiness returns after 3s when no reset is proven");
True(!knownCooldowns.ContainsKey("Unknown Skill"),"unknown cooldown is never guessed");
True(ValidatedCooldownCatalog.Entries.All(x=>!string.IsNullOrWhiteSpace(x.GlobalVersion)),"every validated cooldown pins its Global evidence/version");
Equal(ValidatedCooldownCatalog.Entries.Count,
    ValidatedCooldownCatalog.Entries.Select(x=>$"{x.ClassName}:{x.Skill}").Distinct(StringComparer.OrdinalIgnoreCase).Count(),
    "validated cooldown catalog has no duplicate class/skill entries");
var templarProvisional=RotationProfileCatalog.CreateProvisionalTemplarSingleTarget();
True(templarProvisional.Validation==ProfileValidation.Provisional,"Templar fixture remains provisional");
True(templarProvisional.Rules.Single(x=>x.Skill=="Judgment").Conditions.Any(x=>x.Kind==RotationConditionKind.SignalPresent&&x.Key=="JudgmentWindow"),"Judgment requires observed trigger signal");
var templarReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>{{"Punishment",0},{"Empyrean Lord's Punishment",12}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),templarProvisional);
True(templarReady.Next?.Skill=="Punishment"&&!templarReady.Next.Actionable,"provisional Templar emits informational Punishment when observed ready");
var templarUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),templarProvisional);
True(templarUnknown.Next is null,"provisional Templar fails closed when cooldown readiness is unknown");
var judgmentTracker=new PassiveRotationStateTracker();
judgmentTracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
judgmentTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,77,"Tester",99,"Dummy","Shield Smite",100,SourceClass:"Templar"));
var shieldWindow=judgmentTracker.Snapshot();
True(shieldWindow.JudgmentTrigger=="Shield Smite"&&shieldWindow.JudgmentWindowActive(t.AddSeconds(2.9)),"Shield Smite opens observed Judgment window");
True(!shieldWindow.JudgmentWindowActive(t.AddSeconds(3.1)),"Shield Smite Judgment window expires after 2s");
var judgmentAfterShield=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"JudgmentWindow","TemplarFillerWindow"}},templarProvisional);
True(judgmentAfterShield.Next?.Skill=="Judgment","observed Shield Smite chain prioritizes Judgment over sustained filler");
var judgmentAndPunishmentReady=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>{{"Punishment",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"JudgmentWindow","TemplarFillerWindow"}},templarProvisional);
True(judgmentAndPunishmentReady.Next?.Skill=="Judgment","short observed Judgment opportunity is consumed before ready Punishment");
var punishmentWithoutJudgment=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>{{"Punishment",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"TemplarFillerWindow"}},templarProvisional);
True(punishmentWithoutJudgment.Next?.Skill=="Punishment","ready Punishment remains Templar priority when no observed Judgment window exists");
var templarFillerOnly=rotationEngine.Evaluate(new RotationState(t,AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"TemplarFillerWindow"}},templarProvisional);
True(templarFillerOnly.Next?.Skill=="Pummel","confirmed combat activity uses Pummel as priority filler without inventing a fixed combo");
var templarViciousObservation=new PassiveRotationObservation(77,AionClass.Templar,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Vicious Strike",t}},new HashSet<string>(),new HashSet<string>());
var templarDecisiveSignals=PassiveRotationSignalDeriver.Derive(templarViciousObservation,AionClass.Templar,t.AddSeconds(2.9));
True(templarDecisiveSignals.Contains("TemplarDecisiveStrikeWindow"),"observed Vicious Strike opens Decisive Strike chain window");
True(!PassiveRotationSignalDeriver.Derive(templarViciousObservation,AionClass.Templar,t.AddSeconds(3.1)).Contains("TemplarDecisiveStrikeWindow"),"Templar main-chain continuation expires after 3s");
var templarDecisive=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95){Signals=templarDecisiveSignals},templarProvisional);
True(templarDecisive.Next?.Skill=="Decisive Strike","observed Templar main chain advances to Decisive Strike");
var templarDesperateObservation=new PassiveRotationObservation(77,AionClass.Templar,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Decisive Strike",t}},new HashSet<string>(),new HashSet<string>());
var templarDesperateSignals=PassiveRotationSignalDeriver.Derive(templarDesperateObservation,AionClass.Templar,t.AddSeconds(2));
True(templarDesperateSignals.Contains("TemplarDesperateStrikeWindow"),"observed Decisive Strike opens Desperate Strike chain window");
var templarThreatObservation=new PassiveRotationObservation(77,AionClass.Templar,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Desperate Strike",t}},new HashSet<string>(),new HashSet<string>());
var templarThreatSignals=PassiveRotationSignalDeriver.Derive(templarThreatObservation,AionClass.Templar,t.AddSeconds(2));
True(templarThreatSignals.Contains("TemplarThreateningBlowWindow"),"observed Desperate Strike opens Threatening Blow chain window");
var templarThreat=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95){Signals=templarThreatSignals},templarProvisional);
True(templarThreat.Next?.Skill=="Threatening Blow","observed Templar main chain finishes at Threatening Blow");
judgmentTracker.Observe(new(t.AddSeconds(4),CombatKind.Cast,77,"Tester",Skill:"Doom Shield",SourceClass:"Templar"));
var doomWindow=judgmentTracker.Snapshot();
True(doomWindow.JudgmentTrigger=="Doom Shield"&&doomWindow.JudgmentWindowActive(t.AddSeconds(6.9)),
    "current Global Doom Shield opens its documented 3s Judgment window");
True(!doomWindow.JudgmentWindowActive(t.AddSeconds(7.1)),
    "Doom Shield Judgment window expires after 3s");
var doomDecision=rotationEngine.Evaluate(new RotationState(t.AddSeconds(5),AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"JudgmentWindow"}},templarProvisional);
True(doomDecision.Next?.Skill=="Judgment","observed Doom Shield window prioritizes Judgment");
var gladiatorSignalObservation=new PassiveRotationObservation(77,AionClass.Gladiator,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Rending Blow",t}},new HashSet<string>(),new HashSet<string>());
var gladiatorSignals=PassiveRotationSignalDeriver.Derive(gladiatorSignalObservation,AionClass.Gladiator,t.AddSeconds(2));
True(gladiatorSignals.Contains("GladiatorSmashingWindow")&&gladiatorSignals.Contains("GladiatorFillerWindow"),"pure passive signal derivation opens observed Gladiator chain and sustained windows");
var expiredGladiatorSignals=PassiveRotationSignalDeriver.Derive(gladiatorSignalObservation,AionClass.Gladiator,t.AddSeconds(4));
True(!expiredGladiatorSignals.Contains("GladiatorSmashingWindow")&&expiredGladiatorSignals.Contains("GladiatorFillerWindow"),"Gladiator chain signal expires before generic observed-combat window");
var wrongClassSignals=PassiveRotationSignalDeriver.Derive(gladiatorSignalObservation,AionClass.Sorcerer,t.AddSeconds(2));
var wrathfulSignalObservation=new PassiveRotationObservation(78,AionClass.Gladiator,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Rupture Strike",t}},new HashSet<string>(),new HashSet<string>());
True(PassiveRotationSignalDeriver.Derive(wrathfulSignalObservation,AionClass.Gladiator,t.AddSeconds(2.9)).Contains("GladiatorWrathfulWindow"),"observed Rupture Strike opens Gladiator Wrathful Strike chain window");
True(!PassiveRotationSignalDeriver.Derive(wrathfulSignalObservation,AionClass.Gladiator,t.AddSeconds(3.1)).Contains("GladiatorWrathfulWindow"),"Gladiator Wrathful Strike chain window expires after 3s");
True(!wrongClassSignals.Contains("GladiatorSmashingWindow"),"passive sequence signals remain isolated to the observed class");
var chanterSignalObservation=new PassiveRotationObservation(88,AionClass.Chanter,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Impactful Crush",t}},new HashSet<string>(),new HashSet<string>());
True(PassiveRotationSignalDeriver.Derive(chanterSignalObservation,AionClass.Chanter,t.AddSeconds(2.9)).Contains("ChanterDarkCrushWindow"),"Impactful Crush opens observed Chanter Dark Crush window");
True(!PassiveRotationSignalDeriver.Derive(chanterSignalObservation,AionClass.Chanter,t.AddSeconds(3.1)).Contains("ChanterDarkCrushWindow"),"Chanter Dark Crush window expires after 3s");
var chanterGlobalSetupObservation=new PassiveRotationObservation(89,AionClass.Chanter,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Spinning Strike",t}},
    new HashSet<string>(),new HashSet<string>());
True(PassiveRotationSignalDeriver.Derive(chanterGlobalSetupObservation,AionClass.Chanter,t.AddSeconds(2.9)).Contains("ChanterDarkCrushWindow"),
    "current Global observed Spinning Strike setup opens Chanter Dark Crush window");
True(!PassiveRotationSignalDeriver.Derive(chanterGlobalSetupObservation,AionClass.Chanter,t.AddSeconds(3.1)).Contains("ChanterDarkCrushWindow"),
    "current Global Chanter Dark Crush setup remains bounded to the short reaction window");
var chanterMeleeOnlyObservation=new PassiveRotationObservation(90,AionClass.Chanter,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Onslaught",t}},new HashSet<string>(),new HashSet<string>());
True(!PassiveRotationSignalDeriver.Derive(chanterMeleeOnlyObservation,AionClass.Chanter,t.AddSeconds(2)).Contains("ChanterDarkCrushWindow"),
    "generic Chanter melee activity cannot manufacture the current Global ranged-skill Dark Crush trigger");
var spiritSignalObservation=new PassiveRotationObservation(99,AionClass.Spiritmaster,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Flame Blessing",t},{"Summon: Ancient Spirit",t}},new HashSet<string>(),new HashSet<string>());
var spiritSignals=PassiveRotationSignalDeriver.Derive(spiritSignalObservation,AionClass.Spiritmaster,t.AddSeconds(7.9));
True(spiritSignals.Contains("SpiritmasterAncientWindow")&&spiritSignals.Contains("SpiritmasterCorrodeWindow"),"observed Spiritmaster opener actions open Ancient and Corrode windows");
var expiredSpiritSignals=PassiveRotationSignalDeriver.Derive(spiritSignalObservation,AionClass.Spiritmaster,t.AddSeconds(8.1));
True(!expiredSpiritSignals.Contains("SpiritmasterAncientWindow")&&!expiredSpiritSignals.Contains("SpiritmasterCorrodeWindow"),"Spiritmaster observed sequence windows expire after 8s");
var wardingTracker=new PassiveRotationStateTracker();
wardingTracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
wardingTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,77,"Tester",99,"Dummy","Warding Strike",100,SourceClass:"Templar"));
True(wardingTracker.Snapshot().JudgmentWindowActive(t.AddSeconds(2.9)),"Warding Strike opens observed 2s Judgment window");
var rushTracker=new PassiveRotationStateTracker();
rushTracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
rushTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,77,"Tester",99,"Dummy","Shield Rush",100,SourceClass:"Templar"));
True(rushTracker.Snapshot().JudgmentTrigger=="Shield Rush"&&rushTracker.Snapshot().JudgmentWindowActive(t.AddSeconds(2.9)),
    "current Global Shield Rush opens its documented 2s Judgment window");
True(!rushTracker.Snapshot().JudgmentWindowActive(t.AddSeconds(3.1)),
    "Shield Rush Judgment window expires after 2s");
var judgmentState=new RotationState(t,AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"JudgmentWindow"}};
var judgmentDecision=rotationEngine.Evaluate(judgmentState,templarProvisional);
True(judgmentDecision.Next?.Skill=="Judgment"&&!judgmentDecision.Next.Actionable,"observed Judgment window yields provisional informational Judgment");
var noJudgmentSignal=rotationEngine.Evaluate(judgmentState with {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase)},templarProvisional);
True(noJudgmentSignal.Next is null,"Judgment remains ineligible without observed trigger signal");
var assassinProvisional=RotationProfileCatalog.CreateProvisionalAssassinSingleTarget();
True(assassinProvisional.Validation==ProfileValidation.Provisional,"Assassin fixture remains provisional");
var assassinQuickObservation=new PassiveRotationObservation(77,AionClass.Assassin,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Quick Slice",t}},new HashSet<string>(),new HashSet<string>());
var assassinQuickSignals=PassiveRotationSignalDeriver.Derive(assassinQuickObservation,AionClass.Assassin,t.AddSeconds(2.9));
True(assassinQuickSignals.Contains("AssassinBreakingSliceWindow"),"observed Quick Slice opens Breaking Slice chain window");
True(!PassiveRotationSignalDeriver.Derive(assassinQuickObservation,AionClass.Assassin,t.AddSeconds(3.1)).Contains("AssassinBreakingSliceWindow"),"Breaking Slice chain window expires after 3s");
var assassinBreaking=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"AssassinBreakingSliceWindow","AssassinFillerWindow"}},assassinProvisional);
True(assassinBreaking.Next?.Skill=="Breaking Slice"&&!assassinBreaking.Next.Actionable,"observed Quick Slice prioritizes Breaking Slice over Assassin filler");
var assassinMiddleObservation=new PassiveRotationObservation(77,AionClass.Assassin,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Breaking Slice",t}},new HashSet<string>(),new HashSet<string>());
var assassinSwiftSignals=PassiveRotationSignalDeriver.Derive(assassinMiddleObservation,AionClass.Assassin,t.AddSeconds(2.9));
True(assassinSwiftSignals.Contains("AssassinSwiftSliceWindow"),"observed Breaking Slice opens Swift Slice chain window");
var assassinSwift=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"AssassinSwiftSliceWindow","AssassinFillerWindow"}},assassinProvisional);
True(assassinSwift.Next?.Skill=="Swift Slice","observed Breaking Slice prioritizes Swift Slice over Assassin filler");
var assassinSavageObservation=new PassiveRotationObservation(77,AionClass.Assassin,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Savage Roar",t}},new HashSet<string>(),new HashSet<string>());
var assassinSavageSignals=PassiveRotationSignalDeriver.Derive(assassinSavageObservation,AionClass.Assassin,t.AddSeconds(2.9));
True(assassinSavageSignals.Contains("AssassinSavageBackKickWindow"),"observed Savage Roar opens Savage Back Kick chain window");
True(!PassiveRotationSignalDeriver.Derive(assassinSavageObservation,AionClass.Assassin,t.AddSeconds(3.1)).Contains("AssassinSavageBackKickWindow"),"Savage Back Kick chain window expires after 3s");
var assassinSavageKick=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"AssassinSavageBackKickWindow","AssassinFillerWindow"}},assassinProvisional);
True(assassinSavageKick.Next?.Skill=="Savage Back Kick","observed Savage Roar prioritizes Savage Back Kick over Assassin filler");
var assassinSavageMiddle=new PassiveRotationObservation(77,AionClass.Assassin,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Savage Back Kick",t}},new HashSet<string>(),new HashSet<string>());
True(PassiveRotationSignalDeriver.Derive(assassinSavageMiddle,AionClass.Assassin,t.AddSeconds(2.9)).Contains("AssassinSavageSmashWindow"),"observed Savage Back Kick opens Savage Smash chain window");
True(!PassiveRotationSignalDeriver.Derive(assassinSavageMiddle,AionClass.Assassin,t.AddSeconds(3.1)).Contains("AssassinSavageSmashWindow"),"Savage Smash chain window expires after 3s");
var assassinSavageSmash=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"AssassinSavageSmashWindow","AssassinFillerWindow"}},assassinProvisional);
True(assassinSavageSmash.Next?.Skill=="Savage Smash","observed Savage Back Kick prioritizes Savage Smash over Assassin filler");
var assassinUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),assassinProvisional);
True(assassinUnknown.Next is null,"Assassin fails closed without passively proven crit or Insignia state");
var assassinCrit=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Heart Gore",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"CriticalHitWindow"}},assassinProvisional);
True(assassinCrit.Next?.Skill=="Heart Gore"&&!assassinCrit.Next.Actionable,"observed Assassin crit signal yields informational Heart Gore");
var assassinInsignia=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Insignia Explosion",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"InsigniaReady"}},assassinProvisional);
True(assassinInsignia.Next?.Skill=="Insignia Explosion"&&!assassinInsignia.Next.Actionable,"observed Assassin Insignia signal yields informational explosion");
var assassinHeartRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Heart Gore",2}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"CriticalHitWindow","AssassinFillerWindow"}},assassinProvisional);
True(assassinHeartRecovering.Next?.Skill!="Heart Gore","Assassin does not recommend Heart Gore while validated base cooldown is recovering");
var assassinCooldownTracker=new PassiveRotationStateTracker();
assassinCooldownTracker.Observe(new(t,CombatKind.PlayerName,77,"AssassinTester",SourceClass:"Assassin",SourceIdentityConfirmed:true));
assassinCooldownTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,77,"AssassinTester",99,"Dummy","Insignia Explosion",100,SourceClass:"Assassin"));
Equal(ValidatedCooldownCatalog.Remaining(assassinCooldownTracker.Snapshot(),AionClass.Assassin,t.AddSeconds(5))["Insignia Explosion"],6,"validated Assassin Insignia Explosion base cooldown reconstruction");
var assassinFiller=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"AssassinFillerWindow"}},assassinProvisional);
True(assassinFiller.Next?.Skill=="Savage Roar"&&!assassinFiller.Next.Actionable,"expanded observed Assassin sustained skills remain below the existing priority filler");
var assassinTracker=new PassiveRotationStateTracker();
assassinTracker.Observe(new(t,CombatKind.PlayerName,88,"AssassinTester",SourceClass:"Assassin",SourceIdentityConfirmed:true));
assassinTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,88,"AssassinTester",99,"Dummy","Observed Crit",100,
    SourceClass:"Assassin",DamageFlags:DamageFlags.Critical));
var cloneObservation=new PassiveRotationObservation(88,AionClass.Assassin,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Illusive Clone",t}},new HashSet<string>(),new HashSet<string>());
var cloneSignals=PassiveRotationSignalDeriver.Derive(cloneObservation,AionClass.Assassin,t.AddSeconds(19.9));
True(cloneSignals.Contains("AssassinBurstWindow"),"observed Illusive Clone opens bounded Assassin burst window");
True(!PassiveRotationSignalDeriver.Derive(cloneObservation,AionClass.Assassin,t.AddSeconds(20.1)).Contains("AssassinBurstWindow"),"Assassin Illusive Clone burst window expires after 20s");
var cloneDecision=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Shadowstrike",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95){Signals=cloneSignals},assassinProvisional);
True(cloneDecision.Next?.Skill=="Shadowstrike","observed Illusive Clone exposes Assassin burst recommendation");
var assassinShadowRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Shadowstrike",5}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"AssassinBurstWindow","AssassinFillerWindow"}},assassinProvisional);
True(assassinShadowRecovering.Next?.Skill!="Shadowstrike","Assassin burst cannot recommend Shadowstrike while its validated cooldown is recovering");
True(assassinTracker.Snapshot().CriticalHitWindowActive(t.AddSeconds(2.9)),"observed Assassin critical opens passive 2s Heart Gore window");
True(!assassinTracker.Snapshot().CriticalHitWindowActive(t.AddSeconds(3.1)),"Assassin critical window expires after 2s");
var identityTracker=new PassiveRotationStateTracker();
identityTracker.Observe(new(t,CombatKind.PlayerName,101,"Local",SourceClass:"Templar",SourceIdentityConfirmed:true));
identityTracker.Observe(new(t.AddMilliseconds(10),CombatKind.PlayerName,202,"Ally",SourceClass:"Sorcerer",SourceIdentityConfirmed:false));
var identitySnapshot=identityTracker.Snapshot();
True(identitySnapshot.PlayerId==101&&identitySnapshot.ClassName==AionClass.Templar,
    "unconfirmed ally identity cannot replace confirmed passive player");
var gladiatorProvisional=RotationProfileCatalog.CreateProvisionalGladiatorSingleTarget();
True(gladiatorProvisional.Validation==ProfileValidation.Provisional,"Gladiator fixture remains provisional");
var gladiatorUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),gladiatorProvisional);
True(gladiatorUnknown.Next is null,"Gladiator fails closed without passively proven chain state");
var gladiatorFinisher=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"GladiatorFinisherWindow"}},gladiatorProvisional);
True(gladiatorFinisher.Next?.Skill=="Seismic Crash"&&!gladiatorFinisher.Next.Actionable,"observed Gladiator finisher signal yields informational Seismic Crash");
var gladiatorSmashing=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"GladiatorSmashingWindow","GladiatorFillerWindow"}},gladiatorProvisional);
True(gladiatorSmashing.Next?.Skill=="Smashing Blow","observed Rending Blow chain advances to Smashing Blow ahead of sustained filler");
var gladiatorRupture=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"GladiatorRuptureWindow","GladiatorFillerWindow"}},gladiatorProvisional);
True(gladiatorRupture.Next?.Skill=="Rupture Strike","observed Keen Strike chain advances to the current Global Rupture Strike name");
var gladiatorRageObservation=new PassiveRotationObservation(77,AionClass.Gladiator,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Rage Burst",t}},new HashSet<string>(),new HashSet<string>());
var gladiatorRageSignals=PassiveRotationSignalDeriver.Derive(gladiatorRageObservation,AionClass.Gladiator,t.AddSeconds(9.9));
True(gladiatorRageSignals.Contains("GladiatorOverheadWindow"),"observed Rage Burst opens the documented 10s Overhead Slam window");
True(!PassiveRotationSignalDeriver.Derive(gladiatorRageObservation,AionClass.Gladiator,t.AddSeconds(10.1)).Contains("GladiatorOverheadWindow"),"Gladiator Rage Burst Overhead window expires after 10s");
var gladiatorOverheadReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Overhead Slam",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"GladiatorOverheadWindow","GladiatorFillerWindow"}},gladiatorProvisional);
True(gladiatorOverheadReady.Next?.Skill=="Overhead Slam","ready base-rank Overhead Slam consumes observed Rage Burst window");
var gladiatorOverheadRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Overhead Slam",2}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"GladiatorOverheadWindow","GladiatorFillerWindow"}},gladiatorProvisional);
True(gladiatorOverheadRecovering.Next?.Skill!="Overhead Slam","base-rank Gladiator cannot recommend Overhead Slam while its validated cooldown is recovering");
var gladiatorOverheadObservation=new PassiveRotationObservation(77,AionClass.Gladiator,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Overhead Slam",t}},new HashSet<string>(),new HashSet<string>());
var gladiatorUpwardSignals=PassiveRotationSignalDeriver.Derive(gladiatorOverheadObservation,AionClass.Gladiator,t.AddSeconds(2));
True(gladiatorUpwardSignals.Contains("GladiatorUpwardStrikeWindow"),"observed Overhead Slam opens Upward Strike chain window");
var gladiatorUpward=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95){Signals=gladiatorUpwardSignals},gladiatorProvisional);
True(gladiatorUpward.Next?.Skill=="Upward Strike","observed Overhead Slam immediately prioritizes Upward Strike");
var gladiatorCrushingObservation=new PassiveRotationObservation(77,AionClass.Gladiator,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Crushing Wave",t}},new HashSet<string>(),new HashSet<string>());
var gladiatorFrenziedSignals=PassiveRotationSignalDeriver.Derive(gladiatorCrushingObservation,AionClass.Gladiator,t.AddSeconds(2.9));
True(gladiatorFrenziedSignals.Contains("GladiatorFrenziedWaveWindow"),"observed current-Global Crushing Wave opens Frenzied Wave chain window");
True(!PassiveRotationSignalDeriver.Derive(gladiatorCrushingObservation,AionClass.Gladiator,t.AddSeconds(3.1)).Contains("GladiatorFrenziedWaveWindow"),"Gladiator Frenzied Wave chain window expires after 3s");
var gladiatorFrenzied=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=gladiatorFrenziedSignals},gladiatorProvisional);
True(gladiatorFrenzied.Next?.Skill=="Frenzied Wave","observed Crushing Wave immediately prioritizes current-Global Frenzied Wave");
var gladiatorCrushingReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Crushing Wave",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"GladiatorFillerWindow"}},gladiatorProvisional);
True(gladiatorCrushingReady.Next?.Skill=="Crushing Wave","ready Crushing Wave enters current-Global Gladiator sustained priority");
var gladiatorCrushingRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Crushing Wave",7}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"GladiatorFillerWindow"}},gladiatorProvisional);
True(gladiatorCrushingRecovering.Next?.Skill!="Crushing Wave","Gladiator cannot recommend Crushing Wave while its validated cooldown is recovering");
var rangerProvisional=RotationProfileCatalog.CreateProvisionalRangerSingleTarget();
True(rangerProvisional.Validation==ProfileValidation.Provisional,"Ranger fixture remains provisional");
var rangerUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),rangerProvisional);
True(rangerUnknown.Next is null,"Ranger fails closed without passively proven proc/state");
var rangerRupture=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerRuptureWindow"}},rangerProvisional);
True(rangerRupture.Next?.Skill=="Rupture Arrow"&&!rangerRupture.Next.Actionable,"observed Ranger rupture signal yields informational Rupture Arrow");
var rangerFiller=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.25)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerFillerWindow"}},rangerProvisional);
True(rangerFiller.Next?.Skill=="Snipe"&&!rangerFiller.Next.Actionable,"Global APL starts the Ranger Snipe chain before lower sustained fillers");
var rangerBurstReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Burst Arrow",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerFillerWindow"}},rangerProvisional);
True(rangerBurstReady.Next?.Skill=="Snipe","Global APL starts Snipe chain before ready Burst Arrow");
var rangerAfterSnipeUnavailable=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"ranger-burst-test",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Burst Arrow",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerFillerWindow","RangerBurstArrowWindow"}},new RotationProfile(AionClass.Ranger,"ranger-burst-test",RotationMode.SingleTarget,ProfileValidation.Provisional,
    rangerProvisional.Rules.Where(r=>!r.Skill.Equals("Snipe",StringComparison.OrdinalIgnoreCase)).ToArray(),"synthetic priority isolation"));
True(rangerAfterSnipeUnavailable.Next?.Skill=="Burst Arrow","ready Burst Arrow with observed Slow/Root outranks Gale/Drill/Tempest when Snipe is unavailable");
var rangerBurstNoCc=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"ranger-burst-test",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Burst Arrow",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerFillerWindow"}},new RotationProfile(AionClass.Ranger,"ranger-burst-test",RotationMode.SingleTarget,ProfileValidation.Provisional,
    rangerProvisional.Rules.Where(r=>!r.Skill.Equals("Snipe",StringComparison.OrdinalIgnoreCase)).ToArray(),"synthetic CC fail-closed isolation"));
True(rangerBurstNoCc.Next?.Skill!="Burst Arrow","ready Burst Arrow fails closed without passively observed Slow or Root");
var rangerSlowObservation=new PassiveRotationObservation(79,AionClass.Ranger,new Dictionary<string,DateTime>(),new HashSet<string>(),new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Slow"});
True(PassiveRotationSignalDeriver.Derive(rangerSlowObservation,AionClass.Ranger,t).Contains("RangerBurstArrowWindow"),"observed target Slow exposes Ranger Burst Arrow window");
var rangerDrillReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>{{"Drill Dart",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerFillerWindow"}},rangerProvisional);
True(rangerDrillReady.Next?.Skill=="Snipe"&&!rangerDrillReady.Next.Actionable,"Global APL keeps the Snipe chain above ready Drill Dart outside Precision");
var rangerDrillRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>{{"Drill Dart",2.5}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerFillerWindow"}},rangerProvisional);
True(rangerDrillRecovering.Next?.Skill=="Snipe","Ranger keeps the Snipe chain priority while Drill Dart is recovering");
var rangerObservedOnly=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerFillerWindow"}},rangerProvisional);
True(rangerObservedOnly.Next?.Skill!="Marking Shot"&&rangerObservedOnly.Next?.Skill!="Deadshot","generic observed Ranger combat does not invent unreconciled mark or charged-shot state");
var rangerPrecisionObservation=new PassiveRotationObservation(77,AionClass.Ranger,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Observed Ranger Action",t}},
    new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Precision"},new HashSet<string>());
var rangerPrecisionSignals=PassiveRotationSignalDeriver.Derive(rangerPrecisionObservation,AionClass.Ranger,t.AddSeconds(1));
True(rangerPrecisionSignals.Contains("RangerDeadshotWindow"),"observed Precision buff opens Ranger Deadshot window");
var rangerPrecision=rotationEngine.Evaluate(new RotationState(t.AddSeconds(1),AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Deadshot",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,1)
    {Signals=rangerPrecisionSignals},rangerProvisional);
True(rangerPrecision.Next?.Skill=="Deadshot","observed Precision prioritizes Ranger Deadshot over sustained filler");
var rangerDeadshotRecovering=rotationEngine.Evaluate(new RotationState(t.AddSeconds(1),AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Deadshot",6}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,1)
    {Signals=rangerPrecisionSignals},rangerProvisional);
True(rangerDeadshotRecovering.Next?.Skill!="Deadshot","observed Precision cannot recommend Deadshot while its validated Global cooldown is recovering");
var rangerNoPrecisionObservation=rangerPrecisionObservation with { Buffs=new HashSet<string>() };
True(!PassiveRotationSignalDeriver.Derive(rangerNoPrecisionObservation,AionClass.Ranger,t.AddSeconds(1)).Contains("RangerDeadshotWindow"),
    "Ranger Deadshot state fails closed without observed Precision");
var rangerMarkObservation=new PassiveRotationObservation(77,AionClass.Ranger,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Marking Shot",t}},new HashSet<string>(),new HashSet<string>());
var rangerMarkSignals=PassiveRotationSignalDeriver.Derive(rangerMarkObservation,AionClass.Ranger,t.AddSeconds(9.9));
True(rangerMarkSignals.Contains("RangerDeadshotWindow"),"observed current-Global Marking Shot reconstructs base Precision through 10s");
True(!PassiveRotationSignalDeriver.Derive(rangerMarkObservation,AionClass.Ranger,t.AddSeconds(10.1)).Contains("RangerDeadshotWindow"),
    "reconstructed Ranger Precision expires after base 10s and does not assume +5s specialization");
var rangerMarkDeadshot=rotationEngine.Evaluate(new RotationState(t.AddSeconds(9.9),AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Deadshot",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,1)
    {Signals=rangerMarkSignals},rangerProvisional);
True(rangerMarkDeadshot.Next?.Skill=="Deadshot","reconstructed current-Global Precision safely enables ready Deadshot");
var rangerMarkTracker=new PassiveRotationStateTracker();
rangerMarkTracker.Observe(new(t,CombatKind.PlayerName,77,"RangerTester",SourceClass:"Ranger",SourceIdentityConfirmed:true));
rangerMarkTracker.Observe(new(t.AddSeconds(1),CombatKind.Cast,77,"RangerTester",Skill:"Marking Shot",SourceClass:"Ranger"));
Equal(ValidatedCooldownCatalog.Remaining(rangerMarkTracker.Snapshot(),AionClass.Ranger,t.AddSeconds(6))["Marking Shot"],5,
    "validated Ranger Marking Shot base cooldown reconstructs remaining readiness");
Equal(ValidatedCooldownCatalog.Remaining(rangerMarkTracker.Snapshot(),AionClass.Ranger,t.AddSeconds(11))["Marking Shot"],0,
    "Ranger Marking Shot returns after validated current-Global 10s base cooldown");
var rangerSnipeObservation=new PassiveRotationObservation(77,AionClass.Ranger,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Snipe",t}},new HashSet<string>(),new HashSet<string>());
var rangerSnipeSignals=PassiveRotationSignalDeriver.Derive(rangerSnipeObservation,AionClass.Ranger,t.AddSeconds(2.9));
True(rangerSnipeSignals.Contains("RangerRapidFireWindow"),"observed Snipe opens Ranger Rapid Fire chain window");
True(!PassiveRotationSignalDeriver.Derive(rangerSnipeObservation,AionClass.Ranger,t.AddSeconds(3.1)).Contains("RangerRapidFireWindow"),"Ranger Rapid Fire chain window expires after 3s");
var rangerRapidObservation=new PassiveRotationObservation(77,AionClass.Ranger,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Rapid Fire",t}},new HashSet<string>(),new HashSet<string>());
var rangerSpiralSignals=PassiveRotationSignalDeriver.Derive(rangerRapidObservation,AionClass.Ranger,t.AddSeconds(2));
True(rangerSpiralSignals.Contains("RangerSpiralArrowWindow"),"observed Rapid Fire opens Ranger Spiral Arrow chain window");
var rangerSpiral=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95){Signals=rangerSpiralSignals},rangerProvisional);
True(rangerSpiral.Next?.Skill=="Spiral Arrow","observed Ranger Snipe chain progresses to Spiral Arrow");
var sorcererProvisional=RotationProfileCatalog.CreateProvisionalSorcererSingleTarget();
True(sorcererProvisional.Validation==ProfileValidation.Provisional,"Sorcerer fixture remains provisional");
var sorcererChainObservation=new PassiveRotationObservation(66,AionClass.Sorcerer,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Ice Chain",t}},new HashSet<string>(),new HashSet<string>());
var sorcererChainSignals=PassiveRotationSignalDeriver.Derive(sorcererChainObservation,AionClass.Sorcerer,t.AddSeconds(2.9));
True(sorcererChainSignals.Contains("SorcererColdWaveWindow"),"observed Ice Chain opens Sorcerer Cold Wave chain window");
True(!PassiveRotationSignalDeriver.Derive(sorcererChainObservation,AionClass.Sorcerer,t.AddSeconds(3.1)).Contains("SorcererColdWaveWindow"),"Sorcerer Cold Wave chain window expires after 3s");
var sorcererColdWave=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererColdWaveWindow","SorcererFillerWindow"}},sorcererProvisional);
True(sorcererColdWave.Next?.Skill=="Cold Wave"&&!sorcererColdWave.Next.Actionable,"observed Ice Chain prioritizes Cold Wave over Sorcerer sustained filler");
var sorcererUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),sorcererProvisional);
True(sorcererUnknown.Next is null,"Sorcerer fails closed without passively proven burst/state");
var sorcererBurst=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Hellfire",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererBurstWindow"}},sorcererProvisional);
True(sorcererBurst.Next?.Skill=="Hellfire"&&!sorcererBurst.Next.Actionable,"observed Sorcerer burst signal follows expanded priority with Hellfire first");
var sorcererBurstRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Hellfire",12}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererBurstWindow"}},sorcererProvisional);
True(sorcererBurstRecovering.Next?.Skill=="Fire Wall","recovering Hellfire fails closed and falls through to the next proven Sorcerer burst action");
var sorcererFillerState=new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Blaze",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererFillerWindow"}};
var sorcererFiller=rotationEngine.Evaluate(sorcererFillerState,sorcererProvisional);
True(sorcererFiller.Next?.Skill!="Blaze","Sorcerer Blaze fails closed without observed Fire Mark");
var sorcererFireMark=rotationEngine.Evaluate(sorcererFillerState with
    {Debuffs=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Fire Mark"}},sorcererProvisional);
True(sorcererFireMark.Next?.Skill=="Blaze"&&!sorcererFireMark.Next.Actionable,"observed Fire Mark prioritizes Sorcerer Blaze over sustained filler");
var sorcererBlazeRecovering=rotationEngine.Evaluate(sorcererFillerState with
    {CooldownSeconds=new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Blaze",2}},
     Debuffs=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Fire Mark"}},sorcererProvisional);
True(sorcererBlazeRecovering.Next?.Skill!="Blaze","observed Fire Mark cannot bypass validated Blaze cooldown");
var sorcererFirestormReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Firestorm",0},{"Bittercold Wind",8},{"Blaze",3}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererFillerWindow"}},sorcererProvisional);
True(sorcererFirestormReady.Next?.Skill=="Firestorm","ready Firestorm leads Sorcerer sustained filler when Blaze and Bittercold Wind are recovering");
var sorcererBittercoldReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Firestorm",2},{"Bittercold Wind",0},{"Blaze",3}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererFillerWindow"}},sorcererProvisional);
True(sorcererBittercoldReady.Next?.Skill=="Bittercold Wind","ready Bittercold Wind leads Sorcerer sustained filler when Firestorm and Blaze are recovering");
var sorcererObservedFire=new PassiveRotationObservation(66,AionClass.Sorcerer,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Flame Arrow",t}},new HashSet<string>(),new HashSet<string>());
var sorcererObservedFireSignals=PassiveRotationSignalDeriver.Derive(sorcererObservedFire,AionClass.Sorcerer,t.AddSeconds(2));
True(sorcererObservedFireSignals.Contains("SorcererFireMarkWindow"),"observed Global fire hit reconstructs Sorcerer Fire Mark window");
var sorcererFireHitState=sorcererFillerState with {Signals=sorcererObservedFireSignals};
True(rotationEngine.Evaluate(sorcererFireHitState,sorcererProvisional).Next?.Skill=="Blaze","reconstructed Fire Mark makes Blaze eligible without inventing a debuff packet");
var sorcererIceOnly=new PassiveRotationObservation(66,AionClass.Sorcerer,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Ice Chain",t}},new HashSet<string>(),new HashSet<string>());
True(!PassiveRotationSignalDeriver.Derive(sorcererIceOnly,AionClass.Sorcerer,t.AddSeconds(2)).Contains("SorcererFireMarkWindow"),"non-fire Sorcerer activity cannot manufacture Fire Mark");
True(PassiveRotationSignalDeriver.Derive(sorcererObservedFire,AionClass.Sorcerer,t.AddSeconds(4.9)).Contains("SorcererFireMarkWindow"),
    "Sorcerer reconstructed Fire Mark remains valid through the documented 5s duration");
True(!PassiveRotationSignalDeriver.Derive(sorcererObservedFire,AionClass.Sorcerer,t.AddSeconds(5.1)).Contains("SorcererFireMarkWindow"),
    "Sorcerer reconstructed Fire Mark expires after the documented 5s duration");
var sorcererWishTracker=new PassiveRotationStateTracker();
sorcererWishTracker.Observe(new(t,CombatKind.PlayerName,66,"SorcererTester",SourceClass:"Sorcerer",SourceIdentityConfirmed:true));
sorcererWishTracker.Observe(new(t.AddSeconds(1),CombatKind.Cast,66,"SorcererTester",Skill:"Wish of Concentration",SourceClass:"Sorcerer"));
Equal(ValidatedCooldownCatalog.Remaining(sorcererWishTracker.Snapshot(),AionClass.Sorcerer,t.AddSeconds(31))["Wish of Concentration"],30,
    "validated Sorcerer Wish base cooldown reconstruction excludes specialty reductions");
Equal(ValidatedCooldownCatalog.Remaining(sorcererWishTracker.Snapshot(),AionClass.Sorcerer,t.AddSeconds(61))["Wish of Concentration"],0,
    "Sorcerer Wish returns after validated 60s base cooldown when no specialization reduction is proven");
var sorcererWishObservation=sorcererWishTracker.Snapshot();
var sorcererWishSignals=PassiveRotationSignalDeriver.Derive(sorcererWishObservation,AionClass.Sorcerer,t.AddSeconds(10.9));
True(sorcererWishSignals.Contains("SorcererBurstWindow"),"observed current-Global Wish opens Sorcerer 10s burst window");
True(!PassiveRotationSignalDeriver.Derive(sorcererWishObservation,AionClass.Sorcerer,t.AddSeconds(11.1)).Contains("SorcererBurstWindow"),"Sorcerer Wish burst window expires after current-Global 10s duration");
var sorcererWishReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Wish of Concentration",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererFillerWindow"}},sorcererProvisional);
True(sorcererWishReady.Next?.Skill=="Wish of Concentration","ready current-Global Wish leads ordinary Sorcerer filler");
var sorcererWishRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Wish of Concentration",20}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererFillerWindow"}},sorcererProvisional);
True(sorcererWishRecovering.Next?.Skill!="Wish of Concentration","Sorcerer cannot recommend Wish while its validated Global cooldown is recovering");
var spiritmasterProvisional=RotationProfileCatalog.CreateProvisionalSpiritmasterSingleTarget();
True(spiritmasterProvisional.Validation==ProfileValidation.Provisional,"Spiritmaster fixture remains provisional");
var spiritmasterUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),spiritmasterProvisional);
True(spiritmasterUnknown.Next is null,"Spiritmaster fails closed without passively proven target/spirit state");
var spiritmasterBurst=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterBurstWindow"}},spiritmasterProvisional);
True(spiritmasterBurst.Next?.Skill=="Elemental Fusion"&&!spiritmasterBurst.Next.Actionable,"observed Spiritmaster burst signal follows expanded priority with Elemental Fusion");
var spiritmasterFusionObservation=new PassiveRotationObservation(99,AionClass.Spiritmaster,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Observed Spirit Action",t}},
    new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Four Elements"},new HashSet<string>());
var spiritmasterFusionSignals=PassiveRotationSignalDeriver.Derive(spiritmasterFusionObservation,AionClass.Spiritmaster,t.AddSeconds(1));
True(spiritmasterFusionSignals.Contains("SpiritmasterBurstWindow"),
    "observed Four Elements state opens Spiritmaster Elemental Fusion window");
var spiritmasterFusion=rotationEngine.Evaluate(new RotationState(t.AddSeconds(1),AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Four Elements"},new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=spiritmasterFusionSignals},spiritmasterProvisional);
True(spiritmasterFusion.Next?.Skill=="Elemental Fusion"&&!spiritmasterFusion.Next.Actionable,
    "observed Four Elements prioritizes Elemental Fusion over sustained Spiritmaster actions");
var spiritmasterSummonObservation=new PassiveRotationObservation(99,AionClass.Spiritmaster,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Summon: Fire Spirit",t}},
    new HashSet<string>(),new HashSet<string>());
var spiritmasterControlSignals=PassiveRotationSignalDeriver.Derive(spiritmasterSummonObservation,AionClass.Spiritmaster,t.AddSeconds(2.9));
True(spiritmasterControlSignals.Contains("SpiritmasterDimensionalControlWindow"),
    "observed normal spirit summon opens bounded Dimensional Control window");
True(!PassiveRotationSignalDeriver.Derive(spiritmasterSummonObservation,AionClass.Spiritmaster,t.AddSeconds(3.1)).Contains("SpiritmasterDimensionalControlWindow"),
    "Spiritmaster Dimensional Control window expires after 3s");
var spiritmasterControl=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=spiritmasterControlSignals},spiritmasterProvisional);
True(spiritmasterControl.Next?.Skill=="Dimensional Control",
    "observed post-summon Spiritmaster state prioritizes Dimensional Control over filler");
var spiritmasterNoFusion=spiritmasterFusionObservation with {Buffs=new HashSet<string>()};
True(!PassiveRotationSignalDeriver.Derive(spiritmasterNoFusion,AionClass.Spiritmaster,t.AddSeconds(1)).Contains("SpiritmasterBurstWindow"),
    "Spiritmaster Fusion state fails closed without observed Four Elements");
var spiritmasterAncient=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Summon: Ancient Spirit",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterAncientWindow","SpiritmasterFillerWindow"}},spiritmasterProvisional);
True(spiritmasterAncient.Next?.Skill=="Summon: Ancient Spirit","observed Spiritmaster opener sequence with ready Ancient Spirit outranks sustained filler");
var spiritmasterAncientRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Summon: Ancient Spirit",35}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterAncientWindow","SpiritmasterFillerWindow"}},spiritmasterProvisional);
True(spiritmasterAncientRecovering.Next?.Skill!="Summon: Ancient Spirit","observed opener buffs cannot bypass Ancient Spirit's validated Global cooldown");
var spiritmasterAncientTracker=new PassiveRotationStateTracker();
spiritmasterAncientTracker.Observe(new(t,CombatKind.PlayerName,88,"SpiritTester",SourceClass:"Spiritmaster",SourceIdentityConfirmed:true));
spiritmasterAncientTracker.Observe(new(t.AddSeconds(1),CombatKind.Cast,88,"SpiritTester",Skill:"Summon: Ancient Spirit",SourceClass:"Spiritmaster"));
Equal(ValidatedCooldownCatalog.Remaining(spiritmasterAncientTracker.Snapshot(),AionClass.Spiritmaster,t.AddSeconds(46))["Summon: Ancient Spirit"],45,
    "validated Ancient Spirit base cooldown reconstructs remaining readiness");
Equal(ValidatedCooldownCatalog.Remaining(spiritmasterAncientTracker.Snapshot(),AionClass.Spiritmaster,t.AddSeconds(91))["Summon: Ancient Spirit"],0,
    "Ancient Spirit returns after validated current-Global 90s base cooldown");
var spiritmasterCorrode=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterCorrodeWindow","SpiritmasterFillerWindow"}},spiritmasterProvisional);
var spiritmasterCorrodeMissingSignals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterCorrodeWindow","SpiritmasterCorrodeMissingWindow","SpiritmasterFillerWindow"};
var spiritmasterCorrodeMissing=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=spiritmasterCorrodeMissingSignals},spiritmasterProvisional);
True(spiritmasterCorrodeMissing.Next?.Skill=="Jointstrike: Corrode","observed Ancient Spirit sequence advances to Corrode only when Corrode is missing");
var spiritmasterCorrodeObservation=new PassiveRotationObservation(88,AionClass.Spiritmaster,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Jointstrike: Corrode",t}},new HashSet<string>(),new HashSet<string>());
var spiritmasterCorrodeActiveSignals=PassiveRotationSignalDeriver.Derive(spiritmasterCorrodeObservation,AionClass.Spiritmaster,t.AddSeconds(19.9));
True(spiritmasterCorrodeActiveSignals.Contains("SpiritmasterCorrodeActiveWindow")&&!spiritmasterCorrodeActiveSignals.Contains("SpiritmasterCorrodeMissingWindow"),
    "current-Global base Corrode remains active through 20s");
var spiritmasterCorrodeExpiredSignals=PassiveRotationSignalDeriver.Derive(spiritmasterCorrodeObservation,AionClass.Spiritmaster,t.AddSeconds(20.1));
True(!spiritmasterCorrodeExpiredSignals.Contains("SpiritmasterCorrodeActiveWindow")&&spiritmasterCorrodeExpiredSignals.Contains("SpiritmasterCorrodeMissingWindow"),
    "base Corrode expires after 20s without assuming the 30s specialization");
var spiritmasterNoEarlyRefresh=rotationEngine.Evaluate(new RotationState(t.AddSeconds(19.9),AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterCorrodeWindow","SpiritmasterCorrodeActiveWindow","SpiritmasterFillerWindow"}},spiritmasterProvisional);
True(spiritmasterNoEarlyRefresh.Next?.Skill!="Jointstrike: Corrode","Spiritmaster does not waste Corrode while the proven base window remains active");
var spiritmasterFiller=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterFillerWindow"}},spiritmasterProvisional);
True(spiritmasterFiller.Next?.Skill=="Cold Shock","current Global APL places Cold Shock ahead of sustained Combustion filler");
var spiritmasterCurseReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Jointstrike: Curse",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterFillerWindow"}},new RotationProfile(AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
    spiritmasterProvisional.Rules.Where(r=>r.Skill!="Cold Shock").ToArray(),"synthetic Curse readiness isolation"));
True(spiritmasterCurseReady.Next?.Skill=="Jointstrike: Curse","ready Jointstrike Curse leads secondary Spiritmaster sustained filler");
var spiritmasterCurseRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Jointstrike: Curse",4}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterFillerWindow"}},new RotationProfile(AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
    spiritmasterProvisional.Rules.Where(r=>r.Skill!="Cold Shock").ToArray(),"synthetic Curse recovery isolation"));
True(spiritmasterCurseRecovering.Next?.Skill!="Jointstrike: Curse","Spiritmaster cannot recommend Jointstrike Curse while its validated cooldown is recovering");
var spiritmasterWithoutCoreDebuffs=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterFillerWindow"}},new RotationProfile(AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
    spiritmasterProvisional.Rules.Where(r=>r.Skill!="Cold Shock"&&r.Skill!="Jointstrike: Curse").ToArray(),"synthetic sustained isolation"));
True(spiritmasterWithoutCoreDebuffs.Next?.Skill=="Combustion","Combustion remains Spiritmaster primary spam filler after core debuff actions");
var clericProvisional=RotationProfileCatalog.CreateProvisionalClericSingleTarget();
var clericUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),clericProvisional);
True(clericUnknown.Next is null,"Cleric fails closed without passively proven damage/heal state");
var clericFiller=rotationEngine.Evaluate(new RotationState(t,AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ClericFillerWindow"}},clericProvisional);
True(clericFiller.Next?.Skill=="Judgment Thunder"&&!clericFiller.Next.Actionable,"confirmed combat activity uses observed Global Judgment Thunder as Cleric sustained priority");
var clericDamageWindow=rotationEngine.Evaluate(new RotationState(t,AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ClericDamageWindow","ClericFillerWindow"}},clericProvisional);
True(clericDamageWindow.Next?.Skill=="Earth Punishment","Earth Punishment remains the Cleric damage-window setup above Bolt and filler");
var clericBoltIsolation=rotationEngine.Evaluate(new RotationState(t,AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Bolt",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ClericDamageWindow","ClericFillerWindow"}},new RotationProfile(AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
    clericProvisional.Rules.Where(r=>r.Skill!="Earth Punishment"&&r.Skill!="Divine Aura").ToArray(),"synthetic damage-window isolation"));
True(clericBoltIsolation.Next?.Skill=="Bolt","current Global Cleric damage window includes Bolt above sustained filler");
var clericBoltRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Bolt",12}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ClericDamageWindow","ClericFillerWindow"}},new RotationProfile(AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
    clericProvisional.Rules.Where(r=>r.Skill!="Earth Punishment"&&r.Skill!="Divine Aura").ToArray(),"synthetic Bolt recovery isolation"));
True(clericBoltRecovering.Next?.Skill!="Bolt","Cleric cannot recommend Bolt while its validated base cooldown is recovering");
var clericAuraReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Divine Aura",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ClericDamageWindow","ClericFillerWindow"}},new RotationProfile(AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
    clericProvisional.Rules.Where(r=>r.Skill!="Earth Punishment").ToArray(),"synthetic Aura readiness isolation"));
True(clericAuraReady.Next?.Skill=="Divine Aura","ready Divine Aura leads remaining Cleric damage-window actions");
var clericAuraRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Divine Aura",8},{"Bolt",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ClericDamageWindow","ClericFillerWindow"}},new RotationProfile(AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
    clericProvisional.Rules.Where(r=>r.Skill!="Earth Punishment").ToArray(),"synthetic Aura recovery isolation"));
True(clericAuraRecovering.Next?.Skill=="Bolt","recovering Divine Aura falls through to ready Bolt rather than emitting an unavailable action");
var clericChainObservation=new PassiveRotationObservation(88,AionClass.Cleric,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Chain of Torment",t}},new HashSet<string>(),new HashSet<string>());
True(PassiveRotationSignalDeriver.Derive(clericChainObservation,AionClass.Cleric,t.AddSeconds(9.9)).Contains("ClericCondemnationWindow"),"observed Chain of Torment opens bounded Cleric Condemnation window");
True(!PassiveRotationSignalDeriver.Derive(clericChainObservation,AionClass.Cleric,t.AddSeconds(10.1)).Contains("ClericCondemnationWindow"),"Cleric Condemnation window expires with base Chain of Torment duration");
var clericCondemnation=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Condemnation",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ClericCondemnationWindow","ClericFillerWindow"}},clericProvisional);
True(clericCondemnation.Next?.Skill=="Condemnation"&&!clericCondemnation.Next.Actionable,"observed Chain of Torment prioritizes Condemnation over Cleric filler");
var clericEarthObservation=new PassiveRotationObservation(88,AionClass.Cleric,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Chain of Torment",t}},
    new HashSet<string>(),new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Earth Punishment"});
var clericEarthSignals=PassiveRotationSignalDeriver.Derive(clericEarthObservation,AionClass.Cleric,t.AddSeconds(2));
True(clericEarthSignals.Contains("ClericCondemnationWindow")&&clericEarthSignals.Contains("ClericEarthPunishmentWindow"),
    "observed Chain of Torment plus Earth Punishment exposes the Cleric high-value Condemnation window");
var clericEarth=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Condemnation",0}},new HashSet<string>(),new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Earth Punishment"},0,100,1,false,true,.95)
    {Signals=clericEarthSignals},clericProvisional);
True(clericEarth.Next?.Skill=="Condemnation"&&!clericEarth.Next.Actionable,
    "observed Cleric Earth Punishment window keeps Condemnation above sustained filler without assuming reset specialty");
var clericEarthRecovering=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Condemnation",1.5}},new HashSet<string>(),new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Earth Punishment"},0,100,1,false,true,.95)
    {Signals=clericEarthSignals},clericProvisional);
True(clericEarthRecovering.Next?.Skill!="Condemnation",
    "high-value Cleric Earth Punishment state cannot bypass the validated Condemnation cooldown");
var clericNoEarthSignals=PassiveRotationSignalDeriver.Derive(clericChainObservation,AionClass.Cleric,t.AddSeconds(2));
True(!clericNoEarthSignals.Contains("ClericEarthPunishmentWindow"),
    "Cleric Earth Punishment state fails closed when the debuff is not observed");
var chanterProvisional=RotationProfileCatalog.CreateProvisionalChanterSingleTarget();
var chanterUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),chanterProvisional);
True(chanterUnknown.Next is null,"Chanter fails closed without passively proven damage/heal state");
var chanterReaction=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ChanterDarkCrushWindow","ChanterFillerWindow"}},chanterProvisional);
var chanterDarkReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Dark Crush",0}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ChanterDarkCrushWindow","ChanterFillerWindow"}},chanterProvisional);
True(chanterDarkReady.Next?.Skill=="Dark Crush","observed current-Global ranged window recommends ready Dark Crush");
var chanterDarkRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Dark Crush",3}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ChanterDarkCrushWindow","ChanterFillerWindow"}},chanterProvisional);
True(chanterDarkRecovering.Next?.Skill!="Dark Crush","current-Global ranged window cannot bypass Dark Crush base cooldown");
var chanterHeatReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Heat Wave Blow",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ChanterBurstWindow","ChanterFillerWindow"}},chanterProvisional);
True(chanterHeatReady.Next?.Skill=="Heat Wave Blow","ready Heat Wave Blow leads proven Chanter burst window");
var chanterHeatRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Heat Wave Blow",4}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ChanterBurstWindow","ChanterFillerWindow"}},chanterProvisional);
True(chanterHeatRecovering.Next?.Skill!="Heat Wave Blow","Chanter burst cannot recommend Heat Wave Blow while its validated cooldown is recovering");
var chanterStunObservation=new PassiveRotationObservation(89,AionClass.Chanter,
    new Dictionary<string,DateTime>(),new HashSet<string>(),new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Stun"});
var chanterStunSignals=PassiveRotationSignalDeriver.Derive(chanterStunObservation,AionClass.Chanter,t);
True(chanterStunSignals.Contains("ChanterWaveBlowWindow"),"observed target Stun opens current-Global Wave Blow window");
var chanterWaveReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Wave Blow",0}},new HashSet<string>(),new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Stun"},100,100,1,false,true,.95)
    {Signals=chanterStunSignals},chanterProvisional);
True(chanterWaveReady.Next?.Skill=="Wave Blow","ready Wave Blow is recommended only with observed current-Global Stun state");
var chanterWaveRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Wave Blow",8}},new HashSet<string>(),new HashSet<string>(StringComparer.OrdinalIgnoreCase){"Stun"},100,100,1,false,true,.95)
    {Signals=chanterStunSignals},chanterProvisional);
True(chanterWaveRecovering.Next?.Skill!="Wave Blow","observed Stun cannot bypass Wave Blow's validated 20s cooldown");
var chanterNoStunSignals=PassiveRotationSignalDeriver.Derive(chanterStunObservation with {Debuffs=new HashSet<string>()},AionClass.Chanter,t);
True(!chanterNoStunSignals.Contains("ChanterWaveBlowWindow"),"Chanter Wave Blow fails closed without observed target Stun");
var chanterOnslaughtObservation=new PassiveRotationObservation(89,AionClass.Chanter,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Onslaught",t}},new HashSet<string>(),new HashSet<string>());
var chanterOnslaughtSignals=PassiveRotationSignalDeriver.Derive(chanterOnslaughtObservation,AionClass.Chanter,t.AddSeconds(2.9));
True(chanterOnslaughtSignals.Contains("ChanterResonanceCrushWindow"),"observed Onslaught opens Resonance Crush chain window");
var chanterResonanceObservation=new PassiveRotationObservation(89,AionClass.Chanter,
    new Dictionary<string,DateTime>(StringComparer.OrdinalIgnoreCase){{"Resonance Crush",t}},new HashSet<string>(),new HashSet<string>());
var chanterBoltSignals=PassiveRotationSignalDeriver.Derive(chanterResonanceObservation,AionClass.Chanter,t.AddSeconds(2.9));
True(chanterBoltSignals.Contains("ChanterBoltCrushWindow"),"observed Resonance Crush opens Bolt Crush chain window");
var chanterBolt=rotationEngine.Evaluate(new RotationState(t.AddSeconds(2),AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95){Signals=chanterBoltSignals},chanterProvisional);
True(chanterBolt.Next?.Skill=="Bolt Crush","observed Chanter chain progresses to Bolt Crush");
var chanterFiller=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ChanterFillerWindow"}},chanterProvisional);
True(chanterFiller.Next?.Skill=="Incandescent Blow","expanded observed Chanter sustained skills remain below the existing priority filler");
var chanterImpactReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Impactful Crush",0}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ChanterFillerWindow"}},chanterProvisional);
True(chanterImpactReady.Next?.Skill=="Impactful Crush","ready Impactful Crush outranks the Chanter Onslaught chain and sustained filler");
var chanterImpactRecovering=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase){{"Impactful Crush",4}},new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"ChanterFillerWindow"}},chanterProvisional);
True(chanterImpactRecovering.Next?.Skill=="Incandescent Blow","recovering Impactful Crush fails closed and falls through to Chanter sustained damage");
True(clericProvisional.Validation==ProfileValidation.Provisional&&chanterProvisional.Validation==ProfileValidation.Provisional,
    "Cleric and Chanter fixtures remain provisional");

Console.WriteLine($"PASS: {checks} regression assertions");
