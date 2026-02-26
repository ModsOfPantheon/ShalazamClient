using Il2Cpp;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.Extensions;

public static class NetworkWorldItemExtensions
{
    public static LocationPayload ToPostLocationPayload(this NetworkWorldItem networkWorldItem)
    {
        return new LocationPayload
        {
            Id = networkWorldItem.NetworkId.Value,
            Type = "location",
            Location = new LocationBody
            {
                Name = networkWorldItem.displayName,
                LocX = MathF.Round(networkWorldItem.transform.position.x, 2),
                LocY = MathF.Round(networkWorldItem.transform.position.y, 2),
                LocZ = MathF.Round(networkWorldItem.transform.position.z, 2),
                Zone = PantheonGameClient.currentZone.ToString()
            }
        };
    }

    public static ResourcePayload ToPostResourcePayload(this NetworkWorldItem networkWorldItem)
    {
        return new ResourcePayload
        {
            Id = networkWorldItem.NetworkId.Value,
            Type = "resource",
            Resource = new ResourceBody
            {
                Name = networkWorldItem.displayName,
                LocX = MathF.Round(networkWorldItem.transform.position.x, 2),
                LocY = MathF.Round(networkWorldItem.transform.position.y, 2),
                LocZ = MathF.Round(networkWorldItem.transform.position.z, 2),
                Zone = PantheonGameClient.currentZone.ToString()
            }
        };
    }
}