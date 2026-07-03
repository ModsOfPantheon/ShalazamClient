using Il2Cpp;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.Extensions;

public static class BuffDataExtensions
{
    public static BuffPayload ToBuffPayload(this BuffData buffData)
    {
        var data = new BuffInfoPayload
        {
            Id = buffData.Id,
            DesignerId = buffData.DesignerId,
            LoweredDesignerId = buffData.loweredDesignerId,
            DisplayName = buffData.DisplayName,
            LoweredDisplayName = buffData.loweredDisplayName,
            Description = buffData.Description,
            ClassName = ExtractClassName(buffData),
            CategoryType = buffData.CategoryType.ToString(),
            Version = buffData.Version,
            LogicalGraphId = buffData.LogicalGraphId,
            IconName = buffData.Icon?.IconName,
            VisualEffect = buffData.VisualEffect,

            Hidden = buffData.Hidden,
            Dispellable = buffData.Dispellable,
            PersistsDeath = buffData.PersistsDeath,
            DoDiminishingReturns = buffData.DoDiminishingReturns,
            IgnoreIfExists = buffData.IgnoreIfExists,
            OverwriteOthers = buffData.OverwriteOthers,
            // Prefer the IsHardCC() method over the raw isHardCC Nullable<bool> field, which
            // throws an NRE via Il2CppInterop when its backing value is null.
            IsHardCC = buffData.IsHardCC(),
            HasOverwrites = buffData.HasOverwrites(),
            HasAnyFearOrFleeStatus = buffData.HasAnyFearOrFleeStatus(),
            IsBuffAssociatedWithAbility = buffData.IsBuffAssociatedWithAbility(),
            IsBuffAssociatedWithAbilityCastType = buffData.isBuffAssociatedWithAbilityCastType,

            HasDuration = buffData.HasDuration,
            Duration = buffData.Duration,
            Ticks = buffData.Ticks,
            TickInterval = buffData.TickInterval,
            TickOnApply = buffData.TickOnApply,
            TickOnFinish = buffData.TickOnFinish,

            MaxStacks = buffData.MaxStacks,
            LimitedInstances = buffData.LimitedInstances,
            LimitedInstanceCount = buffData.LimitedInstanceCount,

            EntityStatus = ToStringArray(buffData.EntityStatus),
            StatusTypes = ToStringArray(buffData.statusTypes),
            BuffGroups = GetBuffGroups(buffData.buffGroups),
            OverwriteBuffs = GetOverwriteBuffs(buffData.GetOverwriteBuffs())
        };

        return new BuffPayload
        {
            Id = (uint)buffData.Id,
            Type = "buff",
            Buff = new BuffBody
            {
                Id = buffData.Id,
                Data = data
            }
        };
    }

    // The owning class is encoded as the second dot-segment of the DesignerId,
    // e.g. "P4.Cleric.10.TomeofHealingLight.R1" -> "Cleric". Many buffs are not
    // class-scoped, so this returns null rather than falling back to the local player.
    private static string? ExtractClassName(BuffData buffData)
    {
        var designerId = buffData.DesignerId;
        if (!string.IsNullOrEmpty(designerId))
        {
            var parts = designerId.Split('.');
            if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                return parts[1];
            }
        }

        return null;
    }

    private static string[] ToStringArray(Il2CppSystem.Collections.Generic.List<EntityStatusType>? statuses)
    {
        var list = new List<string>();
        if (statuses != null)
        {
            foreach (var status in statuses) list.Add(status.ToString());
        }

        return list.ToArray();
    }

    private static string[] ToStringArray(Il2CppSystem.Collections.Generic.HashSet<EntityStatusType>? statuses)
    {
        var list = new List<string>();
        if (statuses != null)
        {
            foreach (var status in statuses) list.Add(status.ToString());
        }

        return list.ToArray();
    }

    private static BuffGroupData[] GetBuffGroups(Il2CppSystem.Collections.Generic.List<BuffGroup>? groups)
    {
        var list = new List<BuffGroupData>();
        if (groups != null)
        {
            foreach (var group in groups)
            {
                if (group == null) continue;
                list.Add(new BuffGroupData
                {
                    Id = group.Id,
                    DesignerId = group.DesignerId,
                    DisplayName = group.DisplayName
                });
            }
        }

        return list.ToArray();
    }

    private static int[] GetOverwriteBuffs(Il2CppSystem.Collections.Generic.HashSet<int>? buffIds)
    {
        var list = new List<int>();
        if (buffIds != null)
        {
            foreach (var id in buffIds) list.Add(id);
        }

        return list.ToArray();
    }
}
