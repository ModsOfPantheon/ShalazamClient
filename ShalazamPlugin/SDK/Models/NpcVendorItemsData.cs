namespace ShalazamPlugin.SDK.Models;

public record NpcVendorItemsData(
    string NpcName,
    float LocX,
    float LocY,
    float LocZ,
    string Zone,
    IEnumerable<NpcVendorItemEntry> Items);
