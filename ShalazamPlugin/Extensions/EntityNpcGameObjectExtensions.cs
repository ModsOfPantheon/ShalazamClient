using Il2Cpp;
using Il2CppPantheonPersist;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.Extensions;

public static class EntityNpcGameObjectExtensions
{
    public static NpcPayload ToNpcPayload(this EntityNpcGameObject entityNpcGameObject)
    {
        var info = entityNpcGameObject.Info;
        var pos = entityNpcGameObject.transform.position;
        var zone = PantheonGameClient.currentZone.ToString();

        return new NpcPayload
        {
            Id = StableHash(info.DisplayName),
            Type = "npc",
            Npc = new NpcBody
            {
                Id = StableHash(info.DisplayName),
                Data = new NpcInfoPayload
                {
                    Name = info.DisplayName,
                    Title = string.IsNullOrWhiteSpace(info.Title) ? null : info.Title,
                    Kind = info.Kind.ToString(),
                    Profession = entityNpcGameObject.Profession.ToString(),
                    Race = info.Race.ToString(),
                    Class = info.Class.ToString(),
                    Tier = info.Tier.ToString(),
                    Level = entityNpcGameObject.Experience.Level,
                    MaxHp = (int)entityNpcGameObject.Pools.GetPool(PoolType.Health).Max,
                    TrackRadius = MathF.Round(entityNpcGameObject.Tracking.GetTrackingRadius(), 2),
                    Zone = zone,
                    LocX = MathF.Round(pos.x, 2),
                    LocY = MathF.Round(pos.y, 2),
                    LocZ = MathF.Round(pos.z, 2),
                }
            }
        };
    }

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
                MaxHp = (int)entityNpcGameObject.Pools.GetPool(PoolType.Health).Max
            }
        };
    }

    private static uint StableHash(string s, int x = 0, int z = 0)
    {
        const uint prime = 16777619;
        uint hash = 2166136261;
        foreach (char c in s)
        {
            hash ^= c;
            hash *= prime;
        }
        hash ^= (uint)x;
        hash *= prime;
        hash ^= (uint)z;
        hash *= prime;
        return hash;
    }
}
