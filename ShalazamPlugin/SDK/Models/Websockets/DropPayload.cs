using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.SDK.Models;

public sealed class DropPayload : WebsocketPayload
{
    public DropBody Drop { get; set; }    
}