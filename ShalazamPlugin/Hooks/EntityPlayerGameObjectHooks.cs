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
            Globals.LocalPlayer.Inventory.add_ItemAddedEvent(new Action<Item, InventoryWithPersyst.AddFlags>((item, _) => ItemCache.OnItemSeen(item)));
            // The stack-size args aren't unboxed correctly by il2cppinterop (they come back as memory
            // addresses), so we ignore them and just treat it as another sighting of the item.
            Globals.LocalPlayer.Inventory.add_ItemStackSizeChangedEvent(new Action<Item, int, int>((item, _, _) => ItemCache.OnItemSeen(item)));
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