using Il2Cpp;

namespace ShalazamPlugin;

public static class ItemCache
{
    private static readonly Dictionary<int, Item> Cache = new();

    public static void OnItemAdded(Item item, InventoryWithPersyst.AddFlags flags)
    {
        if (!Cache.TryAdd(item.Template.ItemId, item))
        {
            return;
        }
        
        ModMain.ShalazamClient.PostItem(item);
    }

    public static void OnItemStackSizeChanged(Item item, int arg2, int arg3)
    {
        // Do not use the stack size, it has not been unboxed correctly and will return the memory address of
        // the stack size instead... another bug with the il2cppinterop

        if (!Cache.TryAdd(item.Template.ItemId, item))
        {
            return;
        }

        ModMain.ShalazamClient.PostItem(item);
    }
}