namespace ShalazamPlugin.SDK.Models.Websockets;

public class WebsocketPayload
{
    public bool IsTestRealm { get; set; }
    public uint Id { get; set; }
    public string Type { get; set; }
    public string? Message { get; set; }
}