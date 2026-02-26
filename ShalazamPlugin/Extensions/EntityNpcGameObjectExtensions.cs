using Il2Cpp;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.Extensions;

public static class EntityNpcGameObjectExtensions
{
    public static MonsterPayload ToMonsterPayload(this EntityNpcGameObject entityNpcGameObject)
    {
        return new MonsterPayload
        {
            Id = entityNpcGameObject.NetworkId.Value,
            Type = "monster",
            Monster = new MonsterBody
            {
                Level = entityNpcGameObject.Experience.Level,
                Zone = PantheonGameClient.currentZone.ToString(),
                Hour = TimeController.previousHour,
                Name = entityNpcGameObject.Nameplate.nameText.text,
                LocX = MathF.Round(entityNpcGameObject.transform.position.x, 2),
                LocY = MathF.Round(entityNpcGameObject.transform.position.y, 2),
                LocZ = MathF.Round(entityNpcGameObject.transform.position.z, 2),
                Difficulty = GetDifficultyLabelForTier(entityNpcGameObject.info.Tier)
            }
        };
    }
    
    private static string GetDifficultyLabelForTier(NpcTier tier)
    {
        return tier switch
        {
            NpcTier.None => "solo",
            NpcTier.Tier1 => "group",
            NpcTier.Tier2 => "raid",
            NpcTier.Tier3 => "gigachad",
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
        };
    }
}