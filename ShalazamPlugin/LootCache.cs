using Il2Cpp;
using Il2CppViNL;
using MelonLoader;

namespace ShalazamPlugin;

public static class LootCache
{
    private static HashSet<NetworkId> SeenNonSkinnedLoot = new();
    private static HashSet<NetworkId> SeenSkinnedLoot = new();

    public static void OnLootReceived(EntityNpcGameObject entityNpcGameObject, bool isSkinning, IEnumerable<Item> itemsDropped)
    {
        // Upload the full template of every item shown in the loot window, not just the ones the player
        // picks up (those already flow through the inventory ItemAddedEvent). This runs before the per-NPC
        // dedup below so we never skip the item scan; ItemCache dedups by ItemId, so repeat loads are cheap.
        UploadLootItemTemplates(itemsDropped);

        // IsSkinning will only ever be true for the first skinning loot attempt, there is no way to reliably determine
        // the source of loot that I can find, other than checking whether the mob is skinnable at the moment the loot is generated
        if (!isSkinning && SeenNonSkinnedLoot.Contains(entityNpcGameObject.NetworkId) || SeenSkinnedLoot.Contains(entityNpcGameObject.NetworkId))
        {
            // If IsSkinning is false, it's definitely not the first time we've seen this loot. We might not have seen the
            // unskinned loot e.g., if another player has skinned it, so we check both collections
            MelonLogger.Msg("Skipping as we've seen this loot before");
            return;
        }
        
        if (isSkinning)
        {
            SeenSkinnedLoot.Add(entityNpcGameObject.NetworkId);
        }
        else
        {
            SeenNonSkinnedLoot.Add(entityNpcGameObject.NetworkId);
        }
        
        ModMain.ShalazamClient.PostDrops(entityNpcGameObject, isSkinning || SeenSkinnedLoot.Contains(entityNpcGameObject.NetworkId), itemsDropped);
    }

    private static void UploadLootItemTemplates(IEnumerable<Item> itemsDropped)
    {
        if (itemsDropped == null)
        {
            MelonLogger.Msg("[ShalazamItem] Loot: itemsDropped was null, nothing to scan");
            return;
        }

        try
        {
            var scanned = 0;
            foreach (var item in itemsDropped)
            {
                if (item?.Template == null)
                {
                    MelonLogger.Msg("[ShalazamItem] Loot: skipping loot entry with null Item/Template");
                    continue;
                }

                scanned++;
                ItemCache.OnItemSeen(item, "Loot");
            }

            MelonLogger.Msg($"[ShalazamItem] Loot: scanned {scanned} item(s) in the loot window");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[ShalazamItem] Loot: error scanning loot items: {ex.Message}");
        }
    }

    public static void OnNpcDeleted(EntityNpcGameObject entityNpcGameObject)
    {
        SeenNonSkinnedLoot.Remove(entityNpcGameObject.NetworkId);
        SeenSkinnedLoot.Remove(entityNpcGameObject.NetworkId);
    }
}