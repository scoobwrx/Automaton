using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;
using ECommons.Automation;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Text;
using System.Text.RegularExpressions;
using static Dalamud.Game.Text.XivChatType;

namespace Automaton.Features;
public class HuntRelayHelperConfiguration
{
    public List<(XivChatType Channel, string Command, bool IsLocal, bool Enabled)> Channels =
        [
            (Ls1, "l1", true, false),
            (Ls2, "l2", true, false),
            (Ls3, "l3", true, false),
            (Ls4, "l4", true, false),
            (Ls5, "l5", true, false),
            (Ls6, "l6", true, false),
            (Ls7, "l7", true, false),
            (Ls8, "l8", true, false),
            (FreeCompany, "fc", true, false),
            (NoviceNetwork, "n", true, false),
            (CrossLinkShell1, "cwl1", false, false),
            (CrossLinkShell2, "cwl2", false, false),
            (CrossLinkShell3, "cwl3", false, false),
            (CrossLinkShell4, "cwl4", false, false),
            (CrossLinkShell5, "cwl5", false, false),
            (CrossLinkShell6, "cwl6", false, false),
            (CrossLinkShell7, "cwl7", false, false),
            (CrossLinkShell8, "cwl8", false, false)
        ];

    [BoolConfig] public bool OnlySendLocalHuntsToLocalChannels = true;
}

[Tweak(disabled: true)]
public class HuntRelayHelper : Tweak<HuntRelayHelperConfiguration>
{
    public override string Name => "Hunt Relay Helper";
    public override string Description => "Appends a clickable icon to messages with a MapLinkPayload to relay them to other channels.";

    private DalamudLinkPayload RelayLinkPayload = null!;

    public override void Enable()
    {
        Svc.Chat.CheckMessageHandled += OnChatMessage;
        RelayLinkPayload = Svc.PluginInterface.AddChatLinkHandler(0, HandleRelayLink);
    }

    public override void Disable()
    {
        Svc.Chat.CheckMessageHandled -= OnChatMessage;
        Svc.PluginInterface.RemoveChatLinkHandler(0);
    }

    public override void DrawConfig()
    {
        base.DrawConfig();

        foreach (var channel in Config.Channels.ToList())
        {
            var i = Config.Channels.IndexOf(channel);
            var tmp = channel;
            if (ImGui.Checkbox(channel.Channel.ToString(), ref tmp.Enabled))
                Config.Channels[i] = (Config.Channels[i].Channel, Config.Channels[i].Command, Config.Channels[i].IsLocal, tmp.Enabled);
        }

        ImGuiX.Checkbox("Only send local hunts to local channels", ref Config.OnlySendLocalHuntsToLocalChannels);
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var maplink = message.Payloads.FirstOrDefault(x => x is MapLinkPayload, null);
        if (maplink is null) return;

        try
        {
            if (maplink is MapLinkPayload mlp)
            {
                var (world, instance) = DetectWorldAndInstance(message, Player.Object.CurrentWorld.GameData);
                Svc.Log.Verbose($"Detected world {world} and instance {instance} in message with {nameof(MapLinkPayload)}. Appending {nameof(RelayLinkPayload)}");
                message.Payloads.AddRange([RelayLinkPayload, new IconPayload(BitmapFontIcon.NotoriousMonster), new RelayPayload(mlp, world.RowId, instance), RawPayload.LinkTerminator]);
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex.Message, ex);
        }
    }

    private void HandleRelayLink(uint _, SeString link)
    {
        var payload = link.Payloads.OfType<RawPayload>().Select(RelayPayload.Parse).FirstOrDefault(x => x != default);
        if (payload == default) { Svc.Log.Info($"Failed to parse {nameof(RelayPayload)}"); return; }
        foreach (var (channel, command, islocal, enabled) in Config.Channels)
        {
            if (!enabled) continue;
            // TODO: add a check to see if the player is in novice network before sending
            if (channel.GetAttribute<XivChatTypeInfoAttribute>()!.FancyName.StartsWith("Linkshell") && Player.CurrentWorld != Player.HomeWorld) continue;
            if (islocal && Player.Object.CurrentWorld.GameData != payload.World && Config.OnlySendLocalHuntsToLocalChannels) continue;

            var x = payload.MapLink.Map.Id;
            var sb = new SeStringBuilder().AddText($"「{payload.World!.Name}」S  ");
            if (payload.Instance != default)
                sb.Append(SeString.CreateMapLinkWithInstance(payload.MapLink.TerritoryType.RowId, payload.MapLink.Map.RowId, (int?)payload.Instance, payload.MapLink.RawX, payload.MapLink.RawY));
            else
                sb.Append(SeString.CreateMapLink(payload.MapLink.TerritoryType.RowId, payload.MapLink.Map.RowId, payload.MapLink.RawX, payload.MapLink.RawY));

            TaskManager.EnqueueDelay(500);
            TaskManager.Enqueue(() => Chat.Instance.SendMessageUnsafe(Encoding.UTF8.GetBytes($"/{command} {sb.BuiltString}")));
        }
    }

    private (World, uint) DetectWorldAndInstance(SeString message, World? currentWorld)
    {
        var text = string.Join(" ", message.Payloads.OfType<TextPayload>().Select(x => x.Text));
        var heuristicInstance = 0;
        var mapInstance = text.Select(ReplaceSeIconIntanceNumber).OfType<int>().FirstOrDefault(0);

        // trim texts within MapLinkPayload
        const string linkPattern = ".*?\\)";
        var rgx = new Regex(linkPattern);
        text = rgx.Replace(text, "");
        // replace Boxed letters with alphabets
        text = string.Join(string.Empty, text.Select(ReplaceSeIconChar));

        return (FindRow<World>(x => x!.IsPublic && text.Contains(x.Name.RawString, StringComparison.OrdinalIgnoreCase)) ?? currentWorld!, heuristicInstance != 0 ? (uint)heuristicInstance : (uint)mapInstance);
    }

    private char ReplaceSeIconChar(char c)
    {
        return c switch
        {
            (char)SeIconChar.BoxedLetterA => 'A',
            (char)SeIconChar.BoxedLetterB => 'B',
            (char)SeIconChar.BoxedLetterC => 'C',
            (char)SeIconChar.BoxedLetterD => 'D',
            (char)SeIconChar.BoxedLetterE => 'E',
            (char)SeIconChar.BoxedLetterF => 'F',
            (char)SeIconChar.BoxedLetterG => 'G',
            (char)SeIconChar.BoxedLetterH => 'H',
            (char)SeIconChar.BoxedLetterI => 'I',
            (char)SeIconChar.BoxedLetterJ => 'J',
            (char)SeIconChar.BoxedLetterK => 'K',
            (char)SeIconChar.BoxedLetterL => 'L',
            (char)SeIconChar.BoxedLetterM => 'M',
            (char)SeIconChar.BoxedLetterN => 'N',
            (char)SeIconChar.BoxedLetterO => 'O',
            (char)SeIconChar.BoxedLetterP => 'P',
            (char)SeIconChar.BoxedLetterQ => 'Q',
            (char)SeIconChar.BoxedLetterR => 'R',
            (char)SeIconChar.BoxedLetterS => 'S',
            (char)SeIconChar.BoxedLetterT => 'T',
            (char)SeIconChar.BoxedLetterU => 'U',
            (char)SeIconChar.BoxedLetterV => 'V',
            (char)SeIconChar.BoxedLetterW => 'W',
            (char)SeIconChar.BoxedLetterX => 'X',
            (char)SeIconChar.BoxedLetterY => 'Y',
            (char)SeIconChar.BoxedLetterZ => 'Z',
            _ => c,
        };
    }

    private int? ReplaceSeIconIntanceNumber(char c)
    {
        return c switch
        {
            (char)SeIconChar.Instance1 => 1,
            (char)SeIconChar.Instance2 => 2,
            (char)SeIconChar.Instance3 => 3,
            (char)SeIconChar.Instance4 => 4,
            (char)SeIconChar.Instance5 => 5,
            (char)SeIconChar.Instance6 => 6,
            (char)SeIconChar.Instance7 => 7,
            (char)SeIconChar.Instance8 => 8,
            (char)SeIconChar.Instance9 => 9,
            _ => null
        };
    }

    private int ReplaceSeIconCharNumber(char c)
    {
        return c switch
        {
            (char)SeIconChar.Number1 => 1,
            (char)SeIconChar.BoxedNumber1 => 1,
            (char)SeIconChar.Number2 => 2,
            (char)SeIconChar.BoxedNumber2 => 2,
            (char)SeIconChar.Number3 => 3,
            (char)SeIconChar.BoxedNumber3 => 3,
            (char)SeIconChar.Number4 => 4,
            (char)SeIconChar.BoxedNumber4 => 4,
            (char)SeIconChar.Number5 => 5,
            (char)SeIconChar.BoxedNumber5 => 5,
            (char)SeIconChar.Number6 => 6,
            (char)SeIconChar.BoxedNumber6 => 6,
            (char)SeIconChar.Number7 => 7,
            (char)SeIconChar.BoxedNumber7 => 7,
            (char)SeIconChar.Number8 => 8,
            (char)SeIconChar.BoxedNumber8 => 8,
            (char)SeIconChar.Number9 => 9,
            (char)SeIconChar.BoxedNumber9 => 9,
            _ => c,
        };
    }
}
