using ShalazamPlugin.SDK.Models;

namespace ShalazamPlugin.SDK.Models.Websockets;

public class NpcBody
{
    public uint Id { get; set; }
    public NpcInfoPayload Data { get; set; }
}
