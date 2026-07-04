namespace ShalazamPlugin.SDK.Models;

public class BuffInfoPayload
{
    public int Id { get; set; }
    public string DesignerId { get; set; }
    public string? LoweredDesignerId { get; set; }
    public string DisplayName { get; set; }
    public string? LoweredDisplayName { get; set; }
    public string? Description { get; set; }
    public string? ClassName { get; set; }
    public string CategoryType { get; set; }
    public ulong Version { get; set; }
    public int LogicalGraphId { get; set; }
    public string? IconName { get; set; }
    public string? VisualEffect { get; set; }

    public bool Hidden { get; set; }
    public bool Dispellable { get; set; }
    public bool PersistsDeath { get; set; }
    public bool DoDiminishingReturns { get; set; }
    public bool IgnoreIfExists { get; set; }
    public bool OverwriteOthers { get; set; }
    public bool IsHardCC { get; set; }
    public bool HasOverwrites { get; set; }
    public bool HasAnyFearOrFleeStatus { get; set; }
    public bool IsBuffAssociatedWithAbility { get; set; }
    public int IsBuffAssociatedWithAbilityCastType { get; set; }

    public bool HasDuration { get; set; }
    public float Duration { get; set; }
    public int Ticks { get; set; }
    public float TickInterval { get; set; }
    public bool TickOnApply { get; set; }
    public bool TickOnFinish { get; set; }

    public int MaxStacks { get; set; }
    public bool LimitedInstances { get; set; }
    public int LimitedInstanceCount { get; set; }

    public string[] EntityStatus { get; set; }
    public string[] StatusTypes { get; set; }
    public BuffGroupData[] BuffGroups { get; set; }
    public int[] OverwriteBuffs { get; set; }
}
