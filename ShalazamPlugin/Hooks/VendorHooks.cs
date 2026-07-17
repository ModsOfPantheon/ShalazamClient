using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ShalazamPlugin.SDK.Models;

namespace ShalazamPlugin.Hooks;

// Reads vendorItems as a field directly — avoids calling ForEach/GetVendorItems which can NRE.
[HarmonyPatch(typeof(UINpcInteractionMerchant), nameof(UINpcInteractionMerchant.RefreshVendorItems))]
public class VendorUIRefreshHook
{
    private static readonly HashSet<uint> _seenVendors = new();

    private static void Postfix(UINpcInteractionMerchant __instance, TieredMerchantTab unlockedTabs, IEntityNpc npc)
    {
        try
        {
            var npcGameObject = npc?.TryCast<EntityNpcGameObject>();
            var npcName = npcGameObject?.info?.DisplayName ?? "(unknown)";
            var items = npc?.Vendor?.vendorItems;

            if (items == null)
            {
                return;
            }

            foreach (var vi in items)
            {
                if (vi.Item != null)
                {
                    ItemCache.OnItemSeen(vi.Item);
                }
            }

            if (npcGameObject == null || !_seenVendors.Add(npcGameObject.NetworkId.Value))
            {
                return;
            }

            var entries = items
                .Where(vi => vi.Item != null)
                .Select(vi => new NpcVendorItemEntry
                {
                    Id = vi.Item.Template.ItemId,
                    Name = vi.Item.Template.ItemName,
                    Tab = vi.Tab.ToString()
                })
                .ToList();

            var pos = npcGameObject.transform.position;
            ModMain.ShalazamClient.PostNpcVendorItems(
                npcGameObject.NetworkId.Value,
                npcName,
                MathF.Round(pos.x, 2),
                MathF.Round(pos.y, 2),
                MathF.Round(pos.z, 2),
                PantheonGameClient.currentZone.ToString(),
                entries);
        }
        catch (Exception ex)
        {
            MelonLoader.MelonLogger.Warning($"[Vendor:UIRefresh] Hook error: {ex.Message}");
        }
    }
}
