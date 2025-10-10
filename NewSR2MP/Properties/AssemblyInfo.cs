
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// General Information
[assembly: AssemblyTitle("SR2MP")]
[assembly: AssemblyDescription("*NEW* Slime Rancher 2 Multiplayer")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("PinkTarr")] 
[assembly: AssemblyProduct("NewSR2MP")]
[assembly: AssemblyCopyright("PinkTarr 2025")]
[assembly: AssemblyCulture("")]

// Version information 
[assembly: AssemblyVersion("0.0.0.965")]
[assembly: AssemblyFileVersion("0.0.0.965")]
[assembly: NeutralResourcesLanguage( "en-US" )]

[assembly: MelonGame("MonomiPark", "SlimeRancher2")]
[assembly: MelonInfo(typeof(NewSR2MP.Main),"NewSR2MP", "965", "PuIIIoK / PinkTarr")]
[assembly: SR2E.Expansion.SR2EExpansion]
[assembly: MelonOptionalDependencies("RiptideNetworking.dll")]
