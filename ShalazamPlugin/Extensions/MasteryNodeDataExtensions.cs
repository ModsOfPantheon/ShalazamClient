using Il2CppAdventuringMastery;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.Extensions;

public static class MasteryNodeDataExtensions
{
    public static MasteryPayload ToMasteryPayload(
        this IEnumerable<MasteryNodeData> nodes, string characterClass)
    {
        return new MasteryPayload
        {
            Id   = HashHelper.StableHash(characterClass),
            Type = "mastery",
            Mastery = new MasteryBody
            {
                Class = characterClass,
                Nodes = nodes.Select(ToMasteryNodeInfoPayload).ToArray(),
            }
        };
    }

    public static MasteryNodeInfoPayload ToMasteryNodeInfoPayload(this MasteryNodeData node)
    {
        return new MasteryNodeInfoPayload
        {
            NodeGuid                 = node.NodeGuid,
            DisplayName              = node.DisplayName,
            Description              = node.Description,
            Type                     = node.Type.ToString(),
            Quadrant                 = node.Quadrant.ToString(),
            RingLevel                = node.RingLevel,
            PositionX                = node.PositionX,
            PositionY                = node.PositionY,
            AbilityId                = node.AbilityId,
            BaseAbilityId            = node.BaseAbilityId,
            IsMasteryUpgrade         = node.IsMasteryUpgrade,
            MasteryIndex             = node.MasteryIndex,
            IconName                 = node.IconName,
            Color                    = node.Color,
            PotencyUnlock            = node.PotencyUnlock.ToString(),
            SkillPointCost           = node.SkillPointCost,
            MinLevel                 = node.MinLevel,
            MinSkillLevel            = node.MinSkillLevel,
            RequiredTotalPointsSpent = node.RequiredTotalPointsSpent,

            RequiredNodes      = ToArray(node.RequiredNodes),
            RequiredAnyNodes   = ToArray(node.RequiredAnyNodes),
            ExclusiveWithNodes = ToArray(node.ExclusiveWithNodes),

            RequiredQuadrantPoints = node.RequiredQuadrantPoints?.Length > 0
                ? node.RequiredQuadrantPoints.Select(q => new MasteryQuadrantPointRequirement
                  {
                      MasteryQuadrant = q.MasteryQuadrant.ToString(),
                      MinPoints       = q.MinPoints,
                  }).ToArray()
                : Array.Empty<MasteryQuadrantPointRequirement>(),

            RequiredCharacterFlags = node.RequiredCharacterFlags?.Length > 0
                ? node.RequiredCharacterFlags.Select(f => new MasteryCharacterFlagRequirement
                  {
                      FlagId        = f.FlagId,
                      RequiredValue = f.RequiredValue,
                  }).ToArray()
                : Array.Empty<MasteryCharacterFlagRequirement>(),

            AbilityGrants = node.AbilityGrants?.Length > 0
                ? node.AbilityGrants.Select(g => new MasteryAbilityGrantInfo
                  {
                      AbilityId        = g.AbilityId,
                      BaseAbilityId    = g.BaseAbilityId,
                      IsMasteryUpgrade = g.IsMasteryUpgrade,
                      MasteryIndex     = g.MasteryIndex,
                  }).ToArray()
                : Array.Empty<MasteryAbilityGrantInfo>(),

            RewardItemIds = node.RewardItemIds?.Length > 0
                ? node.RewardItemIds.ToArray()
                : Array.Empty<int>(),
        };
    }

    private static string[] ToArray(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray? arr)
        => arr?.Length > 0 ? arr.ToArray() : Array.Empty<string>();
}
