//using Automaton.FeaturesSetup;
//using Automaton.FeaturesSetup.Attributes;

//namespace Automaton.Features;
//public partial class CommandsConfiguration
//{
//    [BoolConfig]
//    public bool EnableMoveSpeedCommand = false;
//}

//public partial class Commands : Tweak<CommandsConfiguration>
//{
//    [CommandHandler("/speed", "Set your movement speed", nameof(Config.EnableMoveSpeedCommand))]
//    private void OnMoveSpeedCommand(string command, string arguments)
//    {
//        if (float.TryParse(arguments, out var speed))
//            Utils.Debug.SetSpeed(speed);
//        else
//            Utils.Debug.SetSpeed();
//    }
//}
