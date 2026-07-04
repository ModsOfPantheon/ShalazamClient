using Il2Cpp;

namespace ShalazamPlugin;

public static class ItemCache
{
    // Deduplicate by ItemId across every source (inventory, vendors, loot, quest rewards) so an item we've
    // already uploaded from one place isn't re-sent when it shows up somewhere else.
    private static readonly HashSet<int> _seenItemIds = new();

    // Called for every Item we come across (inventory, vendors, loot windows, quest reward slots). Uploads
    // the first time we see a given ItemId and ignores it thereafter.
    public static void OnItemSeen(Item item)
    {
        if (item?.Template == null)
        {
            return;
        }

        if (!_seenItemIds.Add(item.Template.ItemId))
        {
            return;
        }

        ModMain.ShalazamClient.PostItem(item);
    }
}
