namespace ShalazamPlugin.SDK.Models.Websockets;

public class UserBody
{
    public string Username { get; set; }
    public string Role { get; set; }
    public string[] Permissions { get; set; }
}
