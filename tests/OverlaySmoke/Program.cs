using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aion2DPSPro;
using Aion2DPSPro.Overlay;
using Aion2DPSPro.Rotation;

internal static class Program
{
 [STAThread]
 static void Main()
 {
  var app=new Application {ShutdownMode=ShutdownMode.OnExplicitShutdown};
  var window=new OverlayWindow();
  var preferencePath=(string)typeof(OverlayWindow).GetProperty("PreferencesPath",BindingFlags.NonPublic|BindingFlags.Static)!.GetValue(null)!;
  var expectedLabRoot=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"GoatMeterRotationLab");
  if(!preferencePath.StartsWith(expectedLabRoot+System.IO.Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
   throw new Exception("Lab overlay settings must be isolated from production");
  Console.WriteLine("PASS: lab preferences use an isolated data directory");
  void Invoke(string name,string value)=>typeof(OverlayWindow).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(window,new object[]{value});
  string? previous=null;
  foreach(var theme in new[]{"Aion Blue/Red","Neon Spectrum","Void Purple","Emerald Glass","Solar Flare","Ice Crystal"})
  {
   Invoke("ApplyTheme",theme);
   string color=((SolidColorBrush)((Border)window.FindName("Root")).Background).Color.ToString();
   if(color==previous)throw new Exception($"Theme did not change: {theme}");previous=color;
  }
  foreach(var style in new[]{"Details Inspired","Kagerou Inspired","Bars Only","Raid Compact","Glass Cards","Tournament","Classic Dashboard"})Invoke("ApplyOverlayStyle",style);
  if(((FrameworkElement)window.FindName("ToolbarGrid")).Visibility!=Visibility.Visible)throw new Exception("Classic did not restore toolbar");
  var t=DateTime.UtcNow;var engine=new CombatEngine(()=>t);
  engine.Apply(new(t,CombatKind.PlayerName,1,"Player",SourceClass:"Templar"));
  engine.Apply(new(t,CombatKind.Damage,1,"Player",2,"Target","Strike",100,SourceClass:"Templar"));
  window.SnapshotProvider=(segment,category)=>engine.Snapshot(segment,category);
  var tabs=(TabControl)window.FindName("Tabs");var segment=(ComboBox)window.FindName("Segment");
  for(int s=0;s<3;s++)for(int c=0;c<8;c++){segment.SelectedIndex=s;tabs.SelectedIndex=c;window.Render(engine.Snapshot());}
  tabs.SelectedIndex=0;segment.SelectedIndex=0;window.Render(engine.Snapshot());
  if(((ListView)window.FindName("Rows")).Items.Count!=1)throw new Exception("Damage rows missing");
  tabs.SelectedIndex=1;window.Render(engine.Snapshot());
  if(((ListView)window.FindName("Rows")).Items.Count!=0)throw new Exception("Healing incorrectly reused damage rows");
  // Integration: identity-only socket and combat socket, same exact entity and endpoint scope.
  var captureEngine=new CombatEngine(()=>t);
  var identityProfile=new Aion2DPSPro.Protocol.ProtocolProfile("test","test","test",13328,new Dictionary<string,Aion2DPSPro.Protocol.PacketTag>{{"selfInfo",new(51,54)},{"damage",new(4,56)}});
  using(var adapter=new Aion2DPSPro.Capture.LiveCaptureAdapter(new Aion2DPSPro.Protocol.CurrentClientDecoder(identityProfile)))
  {
   adapter.EventReceived+=captureEngine.Apply;
   var localIdentityEvents=new List<CombatEvent>();
   adapter.EventReceived+=evt=>localIdentityEvents.Add(evt);
   var process=typeof(Aion2DPSPro.Capture.LiveCaptureAdapter).GetMethod("Process",BindingFlags.NonPublic|BindingFlags.Instance)!;
   void Packet(ushort localPort,string hex)
   {
    var ip=new PacketDotNet.IPv4Packet(System.Net.IPAddress.Parse("10.0.0.2"),System.Net.IPAddress.Parse("10.0.0.1"));
    var tcp=new PacketDotNet.TcpPacket(13328,localPort) {SequenceNumber=100,PayloadData=Convert.FromHexString(hex)};
    ip.PayloadPacket=tcp;process.Invoke(adapter,new object[]{"test-adapter",ip,t});
   }
   Packet(50001,"193336FD235F81C1283708546573744865726F000000");
   Packet(50002,"210438E3A0020400FD2340B7B70009020B95C34701000000D658E7020100");
   if(captureEngine.Snapshot().Players.Single().Name!="TestHero")throw new Exception("Identity-only connection did not resolve combat name");
   if(!localIdentityEvents.Any(evt=>evt.Kind==CombatKind.PlayerName&&evt.SourceIsLocal&&evt.SourceIdentityConfirmed&&evt.SourceId==4605))
    throw new Exception("Decoded selfInfo did not carry exact local-player proof");
   if(!localIdentityEvents.Any(evt=>evt.Kind==CombatKind.Damage&&evt.SourceIsLocal&&evt.SourceId==4605))
    throw new Exception("Identity-only socket did not bridge local-player proof to combat");
   // The latest captured self ID changes after reconnect; never carry a name across generations.
   t=t.AddSeconds(31);
   Packet(50003,"193336AD5F5F91C1283708546573744865726F000000");
   Packet(50004,"240438E3A0020600AD5F40B7B70002020000020B95C34701000000AC5881030100");
   if(!captureEngine.Snapshot().Players.Any(p=>p.EntityId==12205&&p.Name=="TestHero"))throw new Exception("Reconnect identity did not resolve new self entity");
   adapter.Dispose();adapter.Completion.GetAwaiter().GetResult();
  }
  Console.WriteLine("PASS: separate identity/combat sockets resolve exact actor name");
  var reportEngine=new CombatEngine(()=>t.AddSeconds(15));
  reportEngine.Apply(new(t,CombatKind.PlayerName,1,"TestHero",SourceClass:"Templar"));
  reportEngine.Apply(new(t,CombatKind.Damage,1,"TestHero",2,"Training Scarecrow","Punishing Strike",18000,SourceClass:"Templar",DamageFlags:DamageFlags.Critical|DamageFlags.Perfect));
  reportEngine.Apply(new(t.AddSeconds(4),CombatKind.Damage,1,"TestHero",2,"Training Scarecrow","Desperate Strike",12000,SourceClass:"Templar",DamageFlags:DamageFlags.Back));
  reportEngine.Apply(new(t.AddSeconds(8),CombatKind.Damage,1,"TestHero",2,"Training Scarecrow","Pummel",9000,SourceClass:"Templar"));
  reportEngine.Apply(new(t.AddSeconds(10),CombatKind.Heal,1,"TestHero",1,"TestHero","Mend",3500,SourceClass:"Templar"));
  var report=new CombatReportWindow(()=>reportEngine.Snapshot(MeterSegment.Current,MeterCategory.Damage,true),1);
  report.Show();report.UpdateLayout();
  if(((DataGrid)report.FindName("SkillGrid")).Items.Count!=3)throw new Exception("Report damage skills missing");
  if(((DataGrid)report.FindName("TargetGrid")).Items.Count!=1)throw new Exception("Report targets missing");
  var reportCategories=(ComboBox)report.FindName("Category");reportCategories.SelectedItem=MeterCategory.Healing;report.Refresh();
  if(((DataGrid)report.FindName("SkillGrid")).Items.Count!=1)throw new Exception("Report healing skills missing");
  reportEngine.Apply(new(t.AddSeconds(11),CombatKind.Heal,1,"TestHero",1,"TestHero","Second heal",500));report.Refresh();
  if(((DataGrid)report.FindName("SkillGrid")).Items.Count!=2)throw new Exception("Report live refresh missing");
  ((CheckBox)report.FindName("Pause")).IsChecked=true;
  reportEngine.Apply(new(t.AddSeconds(12),CombatKind.Heal,1,"TestHero",1,"TestHero","Third heal",500));report.Refresh(true);
  if(((DataGrid)report.FindName("SkillGrid")).Items.Count!=2)throw new Exception("Report pause changed data");
  ((CheckBox)report.FindName("Pause")).IsChecked=false;reportCategories.SelectedItem=MeterCategory.Damage;report.Refresh();report.UpdateLayout();
  var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)report.ActualWidth,(int)report.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(report);
  var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
  using(var file=System.IO.File.Create("report-preview.png"))encoder.Save(file);
  report.Close();
  Console.WriteLine("PASS: modern report data, categories, targets, live refresh and pause");
  window.SnapshotProvider=null;
  var previewEngine=new CombatEngine(()=>t.AddSeconds(25)) {PreviewMode=true};
  string[] classes={"Templar","Sorcerer","Assassin","Ranger","Spiritmaster","Gladiator","Cleric","Chanter"};
  for(int i=0;i<classes.Length;i++) {
   string playerName=i==0?"TestHero":"Party member "+(i+1);
   previewEngine.Apply(new(t,CombatKind.PlayerName,i+100,playerName,SourceClass:classes[i]));
   previewEngine.Apply(new(t,CombatKind.Damage,i+100,playerName,900,"Training target","Opening skill",(8-i)*5000,SourceClass:classes[i]));
   previewEngine.Apply(new(t.AddSeconds(25),CombatKind.Damage,i+100,playerName,900,"Training target","Finishing skill",(8-i)*3500,SourceClass:classes[i]));
  }
  tabs.SelectedIndex=0;segment.SelectedIndex=0;Invoke("ApplyTheme","Ice Crystal");window.Show();
  var rows=(ListView)window.FindName("Rows");
  DataTemplate? previousTemplate=null;
  foreach(var style in new[]{"Details Inspired","Kagerou Inspired","Classic Dashboard"}) {
   Invoke("ApplyOverlayStyle",style);window.Render(previewEngine.Snapshot());window.UpdateLayout();
   if(rows.ItemTemplate==previousTemplate)throw new Exception("Styles reused the same row design");previousTemplate=rows.ItemTemplate;
   if(rows.Items.Count!=8)throw new Exception("Style lost party rows");
   rows.SelectedIndex=0;
   var overlayBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32);overlayBitmap.Render(window);
   var png=new System.Windows.Media.Imaging.PngBitmapEncoder();png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(overlayBitmap));
   using(var output=System.IO.File.Create("overlay-"+style.Replace(" ","-")+".png"))png.Save(output);
  }
  Invoke("ApplyOverlayStyle","Kagerou Inspired");
  var picker=(ComboBox)window.FindName("CategoryPicker");picker.SelectedIndex=1;
  if(tabs.SelectedIndex!=1)throw new Exception("Compact category picker did not select healing");
  window.Render(previewEngine.Snapshot(MeterSegment.Current,MeterCategory.Healing));
  if(rows.Items.Count!=0)throw new Exception("Compact healing view reused damage");
  Console.WriteLine("PASS: distinct responsive meter designs, party rows and compact category selection");
  var assistantFixture=new RotationDecision(
   new SkillRecommendation("Fixture Primary",120,.94,true,new[]{"fixture ready"}),
   new[]{new SkillRecommendation("Fixture Followup",110,.94,true,new[]{"fixture"})},
   "Validated fixture");
  window.RenderRotation(assistantFixture);window.UpdateLayout();
  if(((TextBlock)window.FindName("RotationNextSkill")).Text!="Fixture Primary")throw new Exception("Assistant primary label missing");
  if(!((TextBlock)window.FindName("RotationQueue")).Text.Contains("Fixture Followup"))throw new Exception("Assistant queue missing");
  if(((TextBlock)window.FindName("RotationIconGlyph")).Text!="?")throw new Exception("Unverified skill icon fallback missing");
  if(((TextBlock)window.FindName("RotationIconState")).Text!="ICON UNVERIFIED")throw new Exception("Unverified skill icon was not labeled");
  if(((TextBlock)window.FindName("RotationQueueIconGlyph")).Text!="?")throw new Exception("Unverified planned-skill icon fallback missing");
  if(((TextBlock)window.FindName("RotationQueueIconState")).Text!="CURRENT CHOICES • NOT A SEQUENCE")throw new Exception("Unverified planned-skill icons were not labeled");
  Console.WriteLine("PASS: assistant panel renders fixture decision and fail-closed primary/queue icon states");


  foreach(var identity in SkillIconCatalog.Entries)
  {
   if(SkillIconImages.Find(identity.ClassName,identity.Skill) is not System.Windows.Media.Imaging.BitmapSource bmp || bmp.PixelWidth!=64 || bmp.PixelHeight!=64)
    throw new Exception("Packaged icon missing or corrupt: "+identity.Skill);
  }
  var iconFixture=new RotationDecision(new SkillRecommendation("Pummel",500,.25,false,new[]{"observed filler"}),
   new[]{new SkillRecommendation("Punishing Strike",875,.25,false,new[]{"chain"}),new SkillRecommendation("Punishment",800,.25,false,new[]{"ready"})},
   "Informational"){ClassName=AionClass.Templar,LearningHint="Readiness unknown: Annihilate. Use learned skills manually once."};
  window.RenderRotation(iconFixture);window.UpdateLayout();
  if(((Image)window.FindName("RotationSkillImage")).Source is null || ((TextBlock)window.FindName("RotationIconGlyph")).Visibility!=Visibility.Collapsed)
   throw new Exception("Templar main icon missing");
  var optionCards=(ItemsControl)window.FindName("RotationOptionCards");
  if(optionCards.Items.Count!=2 || optionCards.Items.Cast<OverlayWindow.RotationOptionCard>().Any(o=>o.Icon is null || o.Fallback!=""))
   throw new Exception("Each alternative must have its own matching icon");
  if(((TextBlock)window.FindName("RotationLearningHint")).Visibility!=Visibility.Visible)throw new Exception("First-use hint missing");
  window.RenderRotation(iconFixture with {ClassName=AionClass.Chanter});window.UpdateLayout();
  if(((Image)window.FindName("RotationSkillImage")).Source is not null || optionCards.Items.Cast<OverlayWindow.RotationOptionCard>().Any(o=>o.Icon is not null))
   throw new Exception("Class switch retained stale Templar icons");
  window.RenderRotation(new RotationDecision(null,Array.Empty<SkillRecommendation>(),"Waiting"));
  if(((Image)window.FindName("RotationSkillImage")).Source is not null || optionCards.Items.Count!=0 || ((TextBlock)window.FindName("RotationLearningHint")).Visibility!=Visibility.Collapsed)
   throw new Exception("Waiting state retained stale icons or learning guidance");
  Console.WriteLine("PASS: all 30 embedded icons load; primary/alternatives/class switch/waiting lifecycle verified");


  foreach(var iconStyle in new[]{"Classic Dashboard","Details Inspired","Kagerou Inspired","Bars Only","Raid Compact","Glass Cards","Tournament"})
  {
   Invoke("ApplyOverlayStyle",iconStyle);
   window.RenderRotation(iconFixture with {Next=new SkillRecommendation("Empyrean Lord's Punishment",650,.25,false,new[]{"observed equipped stigma"}),
    LearningHint="Readiness unknown: Punishment, Annihilate. Use only learned/equipped skills manually once. Pummel alone cannot open Judgment."});
   window.UpdateLayout();
   var panel=(FrameworkElement)window.FindName("RotationAssistant");
   foreach(var name in new[]{"RotationSkillImage","RotationNextSkill","RotationOptionCards","RotationLearningHint"})
   {
    var child=(FrameworkElement)window.FindName(name);
    var bounds=child.TransformToAncestor(panel).TransformBounds(new Rect(0,0,child.ActualWidth,child.ActualHeight));
    if(!child.IsVisible||bounds.Bottom>panel.ActualHeight+.5||bounds.Right>panel.ActualWidth+.5)
     throw new Exception(iconStyle+" clips icons/long skill name/guidance "+name);
   }
  }
  Invoke("ApplyOverlayStyle","Classic Dashboard");
  window.RenderRotation(iconFixture);window.UpdateLayout();
  var assistantPreview=new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32);
  assistantPreview.Render(window);
  var assistantEncoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
  assistantEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(assistantPreview));
  using(var output=System.IO.File.Create("assistant-icons-preview.png"))assistantEncoder.Save(output);
  Console.WriteLine("PASS: icons, long names and first-use guidance stay visible in every preset");

  var optionFixture=assistantFixture with {Alternatives=new[]{
   new SkillRecommendation("Option A",110,.94,true,new[]{"fixture"}),
   new SkillRecommendation("Option B",100,.94,true,new[]{"fixture"})}};
  window.RenderRotation(optionFixture);
  if(((TextBlock)window.FindName("RotationOptionsLabel")).Text!="OTHER OPTIONS")
   throw new Exception("Alternatives must not be labeled as a predicted future cast sequence");
  if(((TextBlock)window.FindName("RotationQueue")).Text!="Option A • Option B")
   throw new Exception("Distinct alternatives must render as choices without sequence arrows");
  var liveProfile=RotationProfileCatalog.CreateUnvalidatedGlobalStubs().Single(p=>p.ClassName==AionClass.Templar);
  var liveState=new RotationState(t,AionClass.Templar,liveProfile.BuildId,liveProfile.Mode,
   new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.25);
  var liveDecision=new RotationEngine().Evaluate(liveState,liveProfile);
  window.RenderRotation(liveDecision);
  if(liveDecision.Next is not null)throw new Exception("Unvalidated live profile produced a recommendation");
  if(((TextBlock)window.FindName("RotationNextSkill")).Text!="Waiting for observed state")throw new Exception("Fail-closed passive waiting state was not rendered");
  if(((TextBlock)window.FindName("RotationStateText")).Text!="PASSIVE • WAITING")throw new Exception("Passive waiting label missing");
  if(!((TextBlock)window.FindName("RotationReason")).Text.Contains("No validated rules"))throw new Exception("Fail-closed diagnostic missing");
  Console.WriteLine("PASS: passive live-state bridge remains fail closed without validated class rules");
  window.RenderRotation(liveDecision with {ReadinessContext="Cooldown readiness: Punishment 18.0s"});
  if(!((TextBlock)window.FindName("RotationDiagnostic")).Text.Contains("Punishment 18.0s"))throw new Exception("Validated readiness context not rendered");
  window.RenderRotation(liveDecision with {ReadinessContext="Cooldown readiness: insufficient validated observations"});
  if(!((TextBlock)window.FindName("RotationDiagnostic")).Text.Contains("insufficient validated observations"))throw new Exception("Unknown readiness did not fail closed");
  Console.WriteLine("PASS: readiness context renders validated timers and explicit unknown state");

  foreach(var diagnosticStyle in new[]{"Classic Dashboard","Details Inspired","Kagerou Inspired","Bars Only","Raid Compact","Glass Cards","Tournament"})
  {
   Invoke("ApplyOverlayStyle",diagnosticStyle);
   window.RenderRotation(new RotationDecision(null,Array.Empty<SkillRecommendation>(),"Waiting for selfInfo local-player identity and known class."));
   window.UpdateLayout();
   var assistantPanel=(FrameworkElement)window.FindName("RotationAssistant");
   foreach(var diagnosticName in new[]{"RotationNextSkill","RotationReason","RotationDiagnostic"})
   {
    var diagnosticElement=(FrameworkElement)window.FindName(diagnosticName);
    var diagnosticBounds=diagnosticElement.TransformToAncestor(assistantPanel)
        .TransformBounds(new Rect(0,0,diagnosticElement.ActualWidth,diagnosticElement.ActualHeight));
    if(!diagnosticElement.IsVisible||diagnosticBounds.Bottom>assistantPanel.ActualHeight+.5)
        throw new Exception(diagnosticStyle+" clips assistant diagnostic "+diagnosticName);
   }
  }
  Console.WriteLine("PASS: every overlay preset keeps assistant waiting diagnostics visible");
  window.Close();Console.WriteLine("PASS: WPF themes, styles, segment/category switching");app.Shutdown();
 }
}
