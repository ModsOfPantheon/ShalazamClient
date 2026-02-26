namespace ShalazamPlugin.SDK.Models.Websockets;

public class LocationPayload : WebsocketPayload
{
    public LocationBody Location { get; set; }
}