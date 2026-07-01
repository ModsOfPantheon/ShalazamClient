using Il2Cpp;
using MelonLoader;

namespace ShalazamPlugin;

public static class ItemCache
{
    // Deduplicate by ItemId across every source (inventory, vendors, quest rewards) so a template we've
    // already uploaded from one place isn't re-sent when it shows up somewhere else.
    private static readonly HashSet<int> SeenItemIds = new();

    public static void OnItemAdded(Item item, InventoryWithPersyst.AddFlags flags)
    {
        if (!SeenItemIds.Add(item.Template.ItemId))
        {
            return;
        }

        ModMain.ShalazamClient.PostItem(item);
    }

    public static void OnItemStackSizeChanged(Item item, int arg2, int arg3)
    {
        // Do not use the stack size, it has not been unboxed correctly and will return the memory address of
        // the stack size instead... another bug with the il2cppinterop

        if (!SeenItemIds.Add(item.Template.ItemId))
        {
            return;
        }

        ModMain.ShalazamClient.PostItem(item);
    }

    // Like OnItemAdded, but for Item instances discovered outside inventory (e.g. quest reward slots),
    // with logging so we can watch which source captured what.
    public static void OnItemSeen(Item item, string source)
    {
        if (item?.Template == null)
        {
            return;
        }

        if (!SeenItemIds.Add(item.Template.ItemId))
        {
            MelonLogger.Msg($"[ShalazamItem] {source}: item {item.Template.ItemId} ({item.Template.ItemName}) already cached, skipping");
            return;
        }

        MelonLogger.Msg($"[ShalazamItem] {source}: uploading item {item.Template.ItemId} ({item.Template.ItemName})");
        ModMain.ShalazamClient.PostItem(item);
    }

    // Quest rewards only expose an ItemTemplate (no Item instance), so they come in through here.
    public static void OnItemTemplateSeen(ItemTemplate template, string source)
    {
        if (template == null)
        {
            return;
        }

        if (!SeenItemIds.Add(template.ItemId))
        {
            MelonLogger.Msg($"[ShalazamItem] {source}: item {template.ItemId} ({template.ItemName}) already cached, skipping");
            return;
        }

        MelonLogger.Msg($"[ShalazamItem] {source}: uploading item template {template.ItemId} ({template.ItemName})");
        ModMain.ShalazamClient.PostItemTemplate(template);
    }
}
