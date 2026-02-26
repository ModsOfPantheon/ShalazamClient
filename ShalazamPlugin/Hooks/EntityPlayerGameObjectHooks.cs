using HarmonyLib;
using Il2Cpp;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(EntityPlayerGameObject))]
[HarmonyPatch(nameof(EntityPlayerGameObject.NetworkStart))]
public class PlayerNetworkStart
{
    private static void Postfix(EntityPlayerGameObject __instance)
    {
        // Fired in character select
        if (__instance.NetworkId.Value == 1)
        {
            return;
        }
        
        if (__instance.NetworkId.Value == EntityPlayerGameObject.LocalPlayerId.Value && Globals.LocalPlayer == null)
        {
            Globals.LocalPlayer = __instance;
            Globals.LocalPlayer.Inventory.add_ItemAddedEvent(new Action<Item, InventoryWithPersyst.AddFlags>(ItemCache.OnItemAdded));
            Globals.LocalPlayer.Inventory.add_ItemStackSizeChangedEvent(new Action<Item, int>(ItemCache.OnItemStackSizeChanged));
        }
        
        EntityManager.OnPlayerAdded(__instance);
    }
}

[HarmonyPatch(typeof(EntityPlayerGameObject), nameof(EntityPlayerGameObject.NetworkStop))]
public class PlayerNetworkStop
{
    private static void Prefix(EntityPlayerGameObject __instance)
    {
        if (__instance.NetworkId.Value == EntityPlayerGameObject.LocalPlayerId.Value)
        {
            Globals.LocalPlayer = null;
        }
    }
}