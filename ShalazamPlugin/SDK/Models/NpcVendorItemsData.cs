namespace ShalazamPlugin.SDK.Models;

public class NpcVendorItemsData
{
    public string NpcName { get; set; }
    public IEnumerable<NpcVendorItemEntry> Items { get; set; }
}
