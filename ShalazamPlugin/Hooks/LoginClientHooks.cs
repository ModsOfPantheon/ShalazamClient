using System.Net;
using HarmonyLib;
using Il2Cpp;
using Il2CppViNL;
using MelonLoader;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(Login_Client), nameof(Login_Client.OnConnectedToWorld))]
public class LoginClientOnConnectedToWorldHook
{
    private static void Prefix(Login_Client __instance, Peer peer)
    {
        var serverIp = ParseAddress(peer.RemoteEndPoint.Address.address1);
        
        foreach (var server in __instance.realmList)
        {
            if (server.ServerAddress.StartsWith(serverIp.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                Globals.IsPtr = server.ServerName.Contains("(Test Realm)");
            }
        }
    }

    private static IPAddress ParseAddress(ulong packedAddress)
    {
        // Packed address has IP address as last 4 bytes in little endian format,
        // so just slice the last 4 and call it a day
        var bytes = BitConverter.GetBytes(packedAddress);

        var ipBytes = bytes[4..];

        return new IPAddress(ipBytes);
    }
}
