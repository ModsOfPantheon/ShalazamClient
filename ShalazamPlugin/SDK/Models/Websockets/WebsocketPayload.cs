namespace ShalazamPlugin.SDK.Models.Websockets;

public class WebsocketPayload
{
    public uint Id { get; set; }
    public string Type { get; set; }
    public string? Message { get; set; }
}