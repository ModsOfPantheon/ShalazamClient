using HarmonyLib;
using Il2Cpp;
using Il2CppPantheonPersist;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(EntityClientMessaging.Logic), nameof(EntityClientMessaging.Logic.SendChatMessage), typeof(string), typeof(ChatChannelType))]
public class SendChatMessageHook
{
    private static bool Prefix(EntityClientMessaging.Logic __instance, string message, ChatChannelType channel)
    {
        if (message == "/track")
        {
            ModMain.TrackOffensiveTarget();

            return false;
        }

        if (message == "/shalazamuploadabilities")
        {
            ModMain.UploadAllAbilitiesFromCache();

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(EntityClientMessaging.Logic), nameof(EntityClientMessaging.Logic.RequestWhisper))]
public class RequestWhisperHook
{
    private static bool Prefix(UIChatInput __instance, string targetPlayerName, string message)
    {
        if (message == "/track")
        {
            ModMain.TrackOffensiveTarget();

            return false;
        }

        return true;
    }
}