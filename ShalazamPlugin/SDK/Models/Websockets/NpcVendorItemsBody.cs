using ShalazamPlugin.SDK.Models;

namespace ShalazamPlugin.SDK.Models.Websockets;

public class NpcVendorItemsBody
{
    public uint Id { get; set; }
    public NpcVendorItemsData Data { get; set; }
}
