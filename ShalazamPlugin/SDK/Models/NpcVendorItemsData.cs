namespace ShalazamPlugin.SDK.Models;

public record NpcVendorItemsData(string NpcName, IEnumerable<NpcVendorItemEntry> Items);
