using BossAuras.Core.Commands;
using BossAuras.Core.Extensions;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.DevConsole.ConsoleCommands;


public class AuraConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "aura";
    public override string Args => "[add|remove] <aura-id:string>";
    public override string Description => "Adds/Removes an Aura to the run. Screaming snake case ('BODY_SLAM', not 'Body Slam').";
    public override bool IsNetworked => true;
    
    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length < 1)
            return new CmdResult(success: false, $"'{CmdName}' requires an aura name!");
        if (issuingPlayer == null)
            return new CmdResult(success: false, "A run is currently not in progress!");
        
        string text;
        if (args[0].ToLowerInvariant().Equals("add") || args[0].ToLowerInvariant().Equals("remove"))
        {
            if (args.Length < 2) return new CmdResult(success: false, "You need to specify an aura!");
            text = args[1];
        }
        else
        {
            text = args[0];
        }
        
        var canonicalAura = GetAuraById(text);
        if (canonicalAura == null)
            return new CmdResult(success: false, "Unable to find aura '" + text + "'.");
        
        if (args[0].ToLowerInvariant().Equals("remove"))
        {
            var mutableAura = issuingPlayer.RunState.GetAuraById(canonicalAura.Id);
            if (mutableAura == null) return new CmdResult(success: false, "Unable to find aura in run!");
            TaskHelper.RunSafely(AuraCmd.Remove(mutableAura));
            return new CmdResult(success: true, "Aura removed!");
        }

        var mutableAura2 = canonicalAura.ToMutable();
        mutableAura2.AddInternal(issuingPlayer.RunState);
        mutableAura2.DebugOnlySetup();
        TaskHelper.RunSafely(AuraCmd.Add(mutableAura2));
        return new CmdResult(success: true, "Added Rura '" + text + "'");
    }
    
    private static AuraModel? GetAuraById(string id)
    {
        id = id.ToUpperInvariant();
        var source = AuraDb.AllAuras.Where(a => a.Id.Entry.Contains(id)).ToList();
        var auraModel = source.FirstOrDefault(a => a.Id.Entry == id);
        if (auraModel != null) return auraModel;
        var auraModel2 = source.FirstOrDefault(a => a.Id.Entry.StartsWith(id));
        return auraModel2 ?? source.FirstOrDefault();
    }

}