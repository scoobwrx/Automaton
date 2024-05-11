//using Automaton.FeaturesSetup;
//using Automaton.FeaturesSetup.Attributes;
//using Automaton.Helpers;
//using Automaton.Utils;

//namespace Automaton.Features;
//public partial class CommandsConfiguration
//{
//    [BoolConfig]
//    public bool EnableTeleportFlagCommand = false;
//}

//[Tweak]
//public partial class Commands : Tweak<CommandsConfiguration>
//{
//    [CommandHandler("/tpf", "Teleport to the aetheryte nearest your flag", nameof(Config.EnableMoveSpeedCommand))]
//    private void OnTeleportFlagCommmand(string command, string arguments)
//    {
//        Coords.TeleportToAetheryte(Coords.GetNearestAetheryte(Player.MapFlag));
//    }
//}
