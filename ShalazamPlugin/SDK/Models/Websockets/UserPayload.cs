namespace ShalazamPlugin.SDK.Models.Websockets;

public class UserPayload : WebsocketPayload
{
    public UserBody Me { get; set; }
}