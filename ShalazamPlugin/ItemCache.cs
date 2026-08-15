using Il2Cpp;
using ShalazamPlugin.Extensions;

namespace ShalazamPlugin;

public static class ItemCache
{
    // Deduplicate across every source (inventory, vendors, loot, quest rewards) so an item we've already
    // uploaded from one place isn't re-sent when it shows up somewhere else.
    //
    // Keyed on ItemId *plus* the rolled stats rather than ItemId alone: uncommon+ items roll their own
    // stats per instance, so two copies of the same ItemId are genuinely different data and both need
    // uploading. See ItemExtensions.GetDedupeSignature.
    private static readonly HashSet<string> _seenItemSignatures = new();

    // Called for every Item we come across (inventory, vendors, loot windows, quest reward slots). Uploads
    // the first time we see a given item/roll combination and ignores it thereafter.
    public static void OnItemSeen(Item item)
    {
        if (item?.Template == null)
        {
            return;
        }

        if (!_seenItemSignatures.Add(item.GetDedupeSignature()))
        {
            return;
        }

        ModMain.ShalazamClient.PostItem(item);
    }
}
