using Automaton.FeaturesSetup;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.SimpleGui;
using System.Numerics;

namespace Automaton.UI;
internal class BasicWindow : Window
{
    private Feature Feature { get; set; }
    public BasicWindow(Feature t) : base($"{Name} - {t.Name}")
    {
        Feature = t;
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        EzConfigGui.WindowSystem.AddWindow(this);
    }

    public static void Dispose()
    {

    }
    public override void Draw() => Feature.Draw();

    public override bool DrawConditions() => Svc.ClientState.IsLoggedIn;
}
