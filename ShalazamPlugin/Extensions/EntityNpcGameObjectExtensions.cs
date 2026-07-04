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
            Id = entityNpcGameObject.NetworkId.Value,
            Type = "npc",
            Npc = new NpcBody
            {
                Id = HashHelper.StableHash(info.DisplayName),
                Data = new NpcInfoPayload
                {
                    Name = info.DisplayName,
                    Title = string.IsNullOrWhiteSpace(info.Title) ? null : info.Title,
                    Kind = info.Kind.ToString(),
                    Profession = entityNpcGameObject.Profession.ToString(),
                    Role = info.Role.ToString(),
                    Gender = info.Gender.ToString(),
                    Race = info.Race.ToString(),
                    Class = info.Class.ToString(),
                    Tier = info.Tier.ToString(),
                    PetMaster = entityNpcGameObject.PetMaster != null,
                    SubNameOn = entityNpcGameObject.Nameplate.subNameText.isActiveAndEnabled,
                    SubNameText = string.IsNullOrWhiteSpace(entityNpcGameObject.Nameplate.subNameText.text)
                        ? null
                        : entityNpcGameObject.Nameplate.subNameText.text,
                    Level = entityNpcGameObject.Experience.Level,
                    Hour = TimeController.previousHour,
                    MaxHp = (int)entityNpcGameObject.Pools.GetPool(PoolType.Health).Max,
                    TrackRadius = MathF.Round(entityNpcGameObject.Tracking.GetTrackingRadius(), 2),
                    Zone = zone,
                    LocX = MathF.Round(pos.x, 2),
                    LocY = MathF.Round(pos.y, 2),
                    LocZ = MathF.Round(pos.z, 2),
                    PrimaryFactionId = entityNpcGameObject.Factions.PrimaryFactionId,
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
                MaxHp = (int)entityNpcGameObject.Pools.GetPool(PoolType.Health).Max,
                PrimaryFactionId = entityNpcGameObject.Factions.PrimaryFactionId,
            }
        };
    }


}
