using Automaton.Features;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using System.Drawing;

namespace Automaton.UI;
public class AchievementTrackerNative : NodeBase<AtkResNode>
{
    private readonly TextNode nameNode;
    private readonly ProgressBarNode progressNode;

    public AchievementTrackerNative(uint nodeId, AchievementTracker.Achv achievement) : base(NodeType.Res)
    {
        NodeID = nodeId;
        Width = 64.0f + 300.0f + 48.0f + 64.0f;
        Height = 64.0f;
        IsVisible = true;

        nameNode = new TextNode
        {
            NodeID = 210000 + nodeId,
            Position = new Vector2(64.0f, 0.0f),
            Size = new Vector2(300.0f, 32.0f),
            TextColor = KnownColor.White.Vector(),
            TextOutlineColor = KnownColor.Black.Vector(),
            IsVisible = true,
            FontSize = 26,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            TextFlags2 = TextFlags2.Ellipsis,
            Text = achievement.Name,
        };

        P.NativeController.AttachToNode(nameNode, this, NodePosition.AsLastChild);

        progressNode = new ProgressBarNode(nodeId)
        {
            NodeID = 220000 + nodeId,
            Progress = achievement.CurrentProgress / achievement.MaxProgress,
            IsVisible = true,
        };

        P.NativeController.AttachToNode(progressNode, this, NodePosition.AsLastChild);
    }
}
