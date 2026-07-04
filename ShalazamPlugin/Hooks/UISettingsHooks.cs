using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(UISettings))]
[HarmonyPatch(nameof(UISettings.Awake))]
public class UISettingsHooks
{
    private const string TabObjectName = "TabPage_Other";
    
    private static void Postfix(UISettings __instance)
    {
        // Fired in char select
        if (__instance.transform.childCount < 11)
        {
            Globals.HasSetUpUI = false;
            return;
        }
        
        if (Globals.HasSetUpUI)
        {
            return;
        }
        
        Globals.HasSetUpUI = true;
        
        var tabOther = __instance.transform.FindChild(TabObjectName);
        var otherLayoutGroup = tabOther.GetChild(0);
        var spacer = otherLayoutGroup.GetChild(1);
        var resetUiButton = otherLayoutGroup.GetChild(2);

        Object.Instantiate(spacer, spacer.position, spacer.rotation, otherLayoutGroup);
        
        var copy = Object.Instantiate(resetUiButton, resetUiButton.position, resetUiButton.rotation, otherLayoutGroup);
        copy.name = "Button_UploadItems";
        copy.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Upload items";

        var buttonComp = copy.GetComponent<Button>();
        buttonComp.onClick = new Button.ButtonClickedEvent();
        buttonComp.onClick.AddCall(new InvokableCall(new Action(() =>
        {
            foreach (var item in Globals.LocalPlayer.Inventory.items)
            {
                ItemCache.OnItemSeen(item.Value);
            }
        })));
    }
}