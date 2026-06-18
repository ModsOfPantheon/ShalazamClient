using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ShalazamPlugin.SDK.Models;

namespace ShalazamPlugin.Hooks;

// Reads vendorItems as a field directly — avoids calling ForEach/GetVendorItems which can NRE.
[HarmonyPatch(typeof(UINpcInteractionMerchant), nameof(UINpcInteractionMerchant.RefreshVendorItems))]
public class VendorUIRefreshHook
{
    private static readonly HashSet<uint> SeenVendors = new();

    private static void Postfix(UINpcInteractionMerchant __instance, TieredMerchantTab unlockedTabs, IEntityNpc npc)
    {
        try
        {
            var npcGameObject = npc?.TryCast<EntityNpcGameObject>();
            var npcName = npcGameObject?.info?.DisplayName ?? "(unknown)";
            var items = npc?.Vendor?.vendorItems;

            if (items == null) return;

            var swItemCache = System.Diagnostics.Stopwatch.StartNew();
            foreach (var vi in items)
            {
                if (vi.Item != null)
                    ItemCache.OnItemAdded(vi.Item, default);
            }
            swItemCache.Stop();
            MelonLoader.MelonLogger.Msg($"[Vendor:UIRefresh] ItemCache loop took {swItemCache.ElapsedMilliseconds}ms");

            if (npcGameObject == null || !SeenVendors.Add(npcGameObject.NetworkId.Value)) return;

            var entries = items
                .Where(vi => vi.Item != null)
                .Select(vi => new NpcVendorItemEntry
                {
                    Id = vi.Item.Template.ItemId,
                    Name = vi.Item.Template.ItemName,
                    Tab = vi.Tab.ToString()
                })
                .ToList();

            ModMain.ShalazamClient.PostNpcVendorItems(npcGameObject.NetworkId.Value, npcName, entries);
        }
        catch (Exception ex)
        {
            MelonLoader.MelonLogger.Warning($"[Vendor:UIRefresh] Hook error: {ex.Message}");
        }
    }
}
