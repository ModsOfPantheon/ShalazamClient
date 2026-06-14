using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using ShalazamPlugin.SDK.Models;

namespace ShalazamPlugin.Hooks;

// Fires when the merchant UI renders its item list (every time the window opens or refreshes).
// Reads vendorItems as a field directly — avoids calling ForEach/GetVendorItems which can NRE.
[HarmonyPatch(typeof(UINpcInteractionMerchant), nameof(UINpcInteractionMerchant.RefreshVendorItems))]
public class VendorUIRefreshHook
{
    private static readonly HashSet<string> SeenVendors = new();

    private static void Postfix(UINpcInteractionMerchant __instance, TieredMerchantTab unlockedTabs, IEntityNpc npc)
    {
        try
        {
            var npcName = npc?.PetMaster?.Info?.DisplayName ?? "(unknown)";
            var vendorLogic = npc?.Vendor;
            var items = vendorLogic?.vendorItems;

            MelonLogger.Msg($"[Vendor:UIRefresh] NPC={npcName} UnlockedTabs={unlockedTabs} FactionId={vendorLogic?.PrimaryFactionId} ItemCount={items?.Count ?? 0}");

            if (items == null) return;
            VendorHookHelpers.LogVendorItems(items);
            VendorHookHelpers.LogTierRequirements(vendorLogic?.TierRequirements);

            foreach (var vi in items)
            {
                if (vi.Item != null)
                    ItemCache.OnItemAdded(vi.Item, default);
            }

            if (!SeenVendors.Add(npcName)) return;

            var entries = items
                .Where(vi => vi.Item != null)
                .Select(vi => new NpcVendorItemEntry
                {
                    Id = vi.Item.Template.ItemId,
                    Name = vi.Item.Template.ItemName,
                    Tab = vi.Tab.ToString()
                })
                .ToList();

            ModMain.ShalazamClient.PostNpcVendorItems(npcName, entries);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[Vendor:UIRefresh] Hook error: {ex.Message}");
        }
    }
}

// Fires when items are loaded onto Vendor.Logic — typically on NPC spawn, but kept for diagnostics.
// Internal_LoadItems has no ref-struct parameters so it's safe to patch.
[HarmonyPatch(typeof(Vendor.Logic), nameof(Vendor.Logic.Internal_LoadItems))]
public class VendorLoadItemsHook
{
    private static void Postfix(Vendor.Logic __instance, Il2CppReferenceArray<VendorItem> vendorItems)
    {
        try
        {
            var npcName = __instance.Entity?.PetMaster?.Info?.DisplayName ?? "(unknown)";
            MelonLogger.Msg($"[Vendor:LoadItems] NPC={npcName} ItemCount={vendorItems?.Count ?? 0}");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[Vendor:LoadItems] Hook error: {ex.Message}");
        }
    }
}

static class VendorHookHelpers
{
    public static void LogVendorItems(Il2CppReferenceArray<VendorItem> items)
    {
        foreach (var vi in items)
        {
            try
            {
                var template = vi.Item?.Template;
                if (template == null) { MelonLogger.Msg("[Vendor]   <null template>"); continue; }

                int? buyPrice = null;
                try { buyPrice = template.BuyPrice?.Unbox<int>(); } catch { }

                // instance-level stat modifiers (what ToItemPayload reads)
                var instanceStatMods = vi.Item?.statModifiers;
                var instanceStatModCount = instanceStatMods?.Count ?? -1;
                var instanceStatModStr = "none";
                if (instanceStatMods != null && instanceStatMods.Count > 0)
                {
                    var parts = new List<string>();
                    foreach (var sm in instanceStatMods)
                    {
                        try { parts.Add($"{sm.Item1}={sm.Item2.Value}({sm.Item2.ModifierType})"); } catch { parts.Add("?"); }
                    }
                    instanceStatModStr = string.Join(", ", parts);
                }

                // template-level stat modifiers (separate array on the template)
                var templateStatMods = template.StatModifiers;
                var templateStatModCount = templateStatMods?.Count ?? -1;

                MelonLogger.Msg(
                    $"[Vendor]   id={template.ItemId}" +
                    $" name={template.ItemName}" +
                    $" tab={vi.Tab}" +
                    $" count={vi.Count}" +
                    $" limited={vi.LimitedCount}" +
                    $" buyPrice={buyPrice?.ToString() ?? "null"}" +
                    $" buyPriceOrDefault={template.BuyPriceOrDefault}" +
                    $" coinValue={template.CoinValue}" +
                    $" rarity={template.RarityId}" +
                    $" itemType={template.ItemTypeId}" +
                    $" level={template.ItemLevel}" +
                    $" reqLevel={template.RequiredLevel}" +
                    $" itemKey={template.ItemKey}" +
                    $" iconKey={template.IconKey}" +
                    $" flags={template.ItemFlags}" +
                    $" maxStack={template.MaxStackSize}" +
                    $" stackSize={vi.Item?.StackSize}"
                );
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Vendor]   Error reading item: {ex.Message}");
            }
        }
    }

    public static void LogTierRequirements(Il2CppReferenceArray<TieredMerchantRequirements>? tierReqs)
    {
        if (tierReqs == null) return;
        foreach (var tier in tierReqs)
        {
            try
            {
                MelonLogger.Msg($"[Vendor] TierReq: vendorId={tier.VendorId} minReputation={tier.MinReputation} minPlayerLevel={tier.MinPlayerLevel} lifetimeCoinTraded={tier.LifetimeCoinTraded}");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Vendor] Error reading tier requirement: {ex.Message}");
            }
        }
    }
}
