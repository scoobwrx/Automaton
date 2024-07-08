namespace Automaton.Features;

[Tweak(outdated: true)]
public unsafe class AutoNumerics : Tweak
{
    public override string Name => "Auto-Fill Numeric Dialogs";
    public override string Description => "This functionality has been replicated in YesAlready. Please migrate to that instead.";

    public override void DrawConfig()
    {
        ImGuiX.DrawLink("Puni.sh Repo", "Puni.sh Repo", "https://love.puni.sh/ment.json");
    }

    //private readonly string splitText = Svc.Data.GetExcelSheet<Addon>()!.First(x => x.RowId == 533).Text.RawString;

    //public class Configs : FeatureConfig
    //{
    //    public bool WorkOnTrading = false;
    //    public int TradeMinOrMax = -1;
    //    public bool TradeExcludeSplit = false;
    //    public bool TradeConfirm = false;

    //    public bool WorkOnFCChest = false;
    //    public int FCChestMinOrMax = 1;
    //    public bool FCExcludeSplit = true;
    //    public bool FCChestConfirm = false;

    //    public bool WorkOnRetainers = false;
    //    public int RetainersMinOrMax = -1;
    //    public bool RetainerExcludeSplit = false;
    //    public bool RetainersConfirm = false;

    //    public bool WorkOnInventory = false;
    //    public int InventoryMinOrMax = -1;
    //    public bool InventoryExcludeSplit = false;
    //    public bool InventoryConfirm = false;

    //    public bool WorkOnMail = false;
    //    public int MailMinOrMax = -1;
    //    public bool MailExcludeSplit = false;
    //    public bool MailConfirm = false;

    //    public bool WorkOnTransmute = false;
    //    public int TransmuteMinOrMax = 0;
    //    public bool TransmuteExcludeSplit = true;
    //    public bool TransmuteConfirm = true;

    //    public bool WorkOnVentures = false;
    //    public int VentureMinOrMax = 1;
    //    public bool VentureExcludeSplit = true;
    //    public bool VentureConfirm = false;
    //}

    //public override void Enable()
    //{
    //    Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "InputNumeric", FillRegularNumeric);
    //    Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ShopExchangeCurrencyDialog", FillVentureNumeric);
    //    Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "Bank", FillBankNumeric);

    //    base.Enable();
    //}

    //public override void Disable()
    //{
    //    Svc.AddonLifecycle.UnregisterListener(FillRegularNumeric);
    //    Svc.AddonLifecycle.UnregisterListener(FillVentureNumeric);
    //    Svc.AddonLifecycle.UnregisterListener(FillBankNumeric);
    //    base.Disable();
    //}

    //private void FillRegularNumeric(AddonEvent type, AddonArgs args)
    //{
    //    if (ImGui.GetIO().KeyShift) return;
    //    var addon = (AtkUnitBase*)args.Addon;

    //    try
    //    {
    //        var minValue = addon->AtkValues[2].Int;
    //        var maxValue = addon->AtkValues[3].Int;
    //        var numericTextNode = addon->UldManager.NodeList[4]->GetAsAtkComponentNode()->Component->UldManager.NodeList[4]->GetAsAtkTextNode();
    //        var numericResNode = addon->UldManager.NodeList[4]->GetAsAtkComponentNode()->Component->UldManager.NodeList[6];

    //        if (Config.WorkOnTrading && Svc.Condition[ConditionFlag.TradeOpen])
    //            TryFill(addon, minValue, maxValue, Config.TradeMinOrMax, Config.TradeExcludeSplit, Config.TradeConfirm);

    //        if (Config.WorkOnFCChest && InFcChest())
    //            TryFill(addon, minValue, maxValue, Config.FCChestMinOrMax, Config.FCExcludeSplit, Config.FCChestConfirm);

    //        if (Config.WorkOnRetainers && Svc.Condition[ConditionFlag.OccupiedSummoningBell] && !InFcChest())
    //            TryFill(addon, minValue, maxValue, Config.RetainersMinOrMax, Config.RetainerExcludeSplit, Config.RetainersConfirm);

    //        if (Config.WorkOnMail && InMail())
    //            TryFill(addon, minValue, maxValue, Config.MailMinOrMax, Config.MailExcludeSplit, Config.MailConfirm);

    //        if (Config.WorkOnTransmute && InTransmute())
    //            TryFill(addon, minValue, maxValue, Config.TransmuteMinOrMax, Config.TransmuteExcludeSplit, Config.TransmuteConfirm);
    //    }
    //    catch (Exception ex)
    //    {
    //        ex.Log();
    //    }
    //}

    //private void TryFill(AtkUnitBase* numeric, int minValue, int maxValue, int minOrMax, bool excludeSplit, bool autoConfirm)
    //{
    //    var numericTextNode = numeric->UldManager.NodeList[4]->GetAsAtkComponentNode()->Component->UldManager.NodeList[4]->GetAsAtkTextNode();
    //    var numericResNode = numeric->UldManager.NodeList[4]->GetAsAtkComponentNode()->Component->UldManager.NodeList[6];

    //    if (excludeSplit && IsSplitAddon()) return;
    //    if (minOrMax == 0)
    //    {
    //        TaskManager.Enqueue(() => numericTextNode->SetText(ConvertToByte(minValue)));
    //        if (autoConfirm)
    //            TaskManager.Enqueue(() => Callback.Fire(numeric, true, minValue));
    //    }
    //    if (minOrMax == 1)
    //    {
    //        TaskManager.Enqueue(() => numericTextNode->SetText(ConvertToByte(maxValue)));
    //        if (autoConfirm)
    //            TaskManager.Enqueue(() => Callback.Fire(numeric, true, maxValue));
    //    }
    //    if (minOrMax == -1)
    //    {
    //        var currentAmt = numericTextNode->NodeText.ToString();
    //        if (int.TryParse(currentAmt, out var num) && num > 0 && !numericResNode->IsVisible)
    //            TaskManager.Enqueue(() => Callback.Fire(numeric, true, int.Parse(currentAmt)));
    //    }
    //}

    //private void FillBankNumeric(AddonEvent type, AddonArgs args)
    //{
    //    if (ImGui.GetIO().KeyShift) return;
    //    if (!Config.WorkOnFCChest) return;
    //    var addon = (AtkUnitBase*)args.Addon;

    //    try
    //    {
    //        var bMinValue = addon->AtkValues[5].Int;
    //        var bMaxValue = addon->AtkValues[6].Int;
    //        var bNumericTextNode = addon->UldManager.NodeList[4]->GetAsAtkComponentNode()->Component->UldManager.NodeList[4]->GetAsAtkTextNode();

    //        if (Config.FCExcludeSplit && IsSplitAddon()) { return; }
    //        if (Config.FCChestMinOrMax == 0)
    //        {
    //            TaskManager.Enqueue(() => bNumericTextNode->SetText(ConvertToByte(bMinValue)));
    //            if (Config.FCChestConfirm)
    //                TaskManager.Enqueue(() => Callback.Fire(addon, true, 3, (uint)bMinValue));
    //        }
    //        if (Config.FCChestMinOrMax == 1)
    //        {
    //            TaskManager.Enqueue(() => bNumericTextNode->SetText(ConvertToByte(bMaxValue)));
    //            if (Config.FCChestConfirm)
    //                TaskManager.Enqueue(() => Callback.Fire(addon, true, 3, (uint)bMaxValue));
    //        }
    //        if (Config.FCChestMinOrMax == -1)
    //        {
    //            var currentAmt = bNumericTextNode->NodeText.ToString();
    //            if (int.TryParse(currentAmt, out var num) && num > 0 && addon->AtkValues[4].Int > 0)
    //                TaskManager.Enqueue(() => Callback.Fire(addon, true, 0));
    //        }
    //    }
    //    catch
    //    {
    //        return;
    //    }
    //}

    //private void FillVentureNumeric(AddonEvent type, AddonArgs args)
    //{
    //    if (ImGui.GetIO().KeyShift) return;
    //    if (!Config.WorkOnVentures) return;
    //    var addon = (AtkUnitBase*)args.Addon;

    //    try
    //    {
    //        var minValue = 1;
    //        var maxAvailable = addon->AtkValues[5].UInt - addon->AtkValues[4].UInt;
    //        var maxAfford = addon->AtkValues[1].UInt / addon->AtkValues[2].UInt;
    //        var maxValue = maxAvailable > maxAfford ? maxAfford : maxAvailable;

    //        var numericTextNode = addon->UldManager.NodeList[8]->GetAsAtkComponentNode()->Component->UldManager.NodeList[4]->GetAsAtkTextNode();

    //        if (Config.VentureMinOrMax == 0)
    //        {
    //            TaskManager.Enqueue(() => numericTextNode->SetText(ConvertToByte(minValue)));
    //            if (Config.VentureConfirm)
    //                TaskManager.Enqueue(() => Callback.Fire(addon, true, 0, minValue));
    //        }
    //        if (Config.VentureMinOrMax == 1)
    //        {
    //            TaskManager.Enqueue(() => numericTextNode->SetText(ConvertToByte((int)maxValue)));
    //            if (Config.VentureConfirm)
    //                TaskManager.Enqueue(() => Callback.Fire(addon, true, 0, maxValue));
    //        }

    //        // No way to detect manually entered amounts
    //    }
    //    catch (Exception ex)
    //    {
    //        ex.Log();
    //    }
    //}

    //private bool IsSplitAddon()
    //{
    //    var numeric = (AtkUnitBase*)Svc.GameGui.GetAddonByName("InputNumeric");
    //    var numericTitleText = numeric->UldManager.NodeList[5]->GetAsAtkTextNode()->NodeText.ToString();
    //    return numericTitleText == splitText;
    //}

    //private bool InFcChest()
    //{
    //    var fcChest = (AtkUnitBase*)Svc.GameGui.GetAddonByName("FreeCompanyChest");
    //    return fcChest != null && fcChest->IsVisible;
    //}

    //private bool InFcBank()
    //{
    //    var fcBank = (AtkUnitBase*)Svc.GameGui.GetAddonByName("Bank");
    //    return fcBank != null && fcBank->IsVisible;
    //}

    //private bool InMail()
    //{
    //    var mail = (AtkUnitBase*)Svc.GameGui.GetAddonByName("LetterList");
    //    return mail != null && mail->IsVisible;
    //}

    //private bool InTransmute()
    //{
    //    var trans = (AtkUnitBase*)Svc.GameGui.GetAddonByName("TradeMultiple");
    //    return trans != null && trans->IsVisible;
    //}

    //private unsafe byte* ConvertToByte(int x)
    //{
    //    var bArray = Encoding.Default.GetBytes(x.ToString());
    //    byte* ptr;
    //    fixed (byte* tmpPtr = bArray) { ptr = tmpPtr; }
    //    return ptr;
    //}

    //private static void DrawConfigsForAddon(string addonName, ref bool workOnAddon, ref int minOrMax, ref bool excludeSplit, ref bool autoConfirm)
    //{
    //    ImGui.Checkbox($"Work on {addonName}", ref workOnAddon);
    //    if (workOnAddon)
    //    {
    //        ImGui.PushID(addonName);
    //        ImGui.Indent();
    //        if (ImGui.RadioButton($"Auto fill highest amount possible", minOrMax == 1))
    //        {
    //            minOrMax = 1;
    //        }
    //        if (ImGui.RadioButton($"Auto fill lowest amount possible", minOrMax == 0))
    //        {
    //            minOrMax = 0;
    //        }
    //        if (addonName != "Venture Purchase")
    //        {
    //            if (ImGui.RadioButton($"Auto OK on manually entered amounts", minOrMax == -1))
    //            {
    //                minOrMax = -1;
    //            }

    //            ImGui.Checkbox("Exclude Split Dialog", ref excludeSplit);
    //        }
    //        if (minOrMax != -1) ImGui.Checkbox("Auto Confirm", ref autoConfirm);
    //        ImGui.Unindent();
    //        ImGui.PopID();
    //    }
    //}
}
