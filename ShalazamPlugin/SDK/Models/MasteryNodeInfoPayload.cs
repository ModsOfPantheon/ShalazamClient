namespace ShalazamPlugin.SDK.Models;

public class MasteryNodeInfoPayload
{
    public string NodeGuid { get; set; }
    public string DisplayName { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; }
    public string Quadrant { get; set; }
    public int RingLevel { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public int AbilityId { get; set; }
    public int BaseAbilityId { get; set; }
    public bool IsMasteryUpgrade { get; set; }
    public int MasteryIndex { get; set; }
    public string IconName { get; set; }
    public string Color { get; set; }
    public string PotencyUnlock { get; set; }
    public int SkillPointCost { get; set; }
    public int MinLevel { get; set; }
    public int MinSkillLevel { get; set; }
    public int RequiredTotalPointsSpent { get; set; }
    public string[] RequiredNodes { get; set; }
    public string[] RequiredAnyNodes { get; set; }
    public string[] ExclusiveWithNodes { get; set; }
    public MasteryQuadrantPointRequirement[] RequiredQuadrantPoints { get; set; }
    public MasteryCharacterFlagRequirement[] RequiredCharacterFlags { get; set; }
    public MasteryAbilityGrantInfo[] AbilityGrants { get; set; }
    public int[] RewardItemIds { get; set; }
}

public class MasteryQuadrantPointRequirement
{
    public string MasteryQuadrant { get; set; }
    public int MinPoints { get; set; }
}

public class MasteryCharacterFlagRequirement
{
    public int FlagId { get; set; }
    public int RequiredValue { get; set; }
}

public class MasteryAbilityGrantInfo
{
    public int AbilityId { get; set; }
    public int BaseAbilityId { get; set; }
    public bool IsMasteryUpgrade { get; set; }
    public int MasteryIndex { get; set; }
}
