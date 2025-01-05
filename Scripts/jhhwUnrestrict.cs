using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Items;
using static Kingmaker.EntitySystem.Stats.ModifiableValue;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Controllers;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Logging;
using Kingmaker.Modding;
using Kingmaker.Blueprints.Items.Weapons;
using System.Linq;

namespace jhhw
{
    [HarmonyPatch]
    public static class Main
    {
        internal static LogChannel log;
        internal static OwlcatModification jhhwbmod;

        [OwlcatModificationEnterPoint]
        public static void Load(OwlcatModification mod)
        {
            jhhwbmod = mod;
            //log = mod.Logger;



            Harmony harmony = new(jhhwbmod.Manifest.UniqueName);
            harmony.PatchAll();
            //log.Log("jhhw: Load");
        }

        [HarmonyPatch(typeof(ItemEntity), nameof(ItemEntity.CanBeEquippedBy))]
        public static class CanEquip
        {
            public static void Postfix(ref bool __result, ItemEntity __instance, MechanicEntity owner)
            {
                string hwTalent = "10c5e79937bb471196382e8a43f142ec";
                string[] factionTalents = {
                    "590cb5c394ee4b5eb213121c96063dec",
                    "c7a53b43ac5f4f11a1aef612dd3f4f71"
                };
                string[] profTalents = {
                    "19673fbc918241c6add6710025f67058",
                    "fc6d24dc538f45dc97f10f8b6486c043",
                    "2818d76d82b341adb2b792ed52f4c203",
                    "dddb6710ce25490e98608a2c07fbfc51"
                };

                // Assuming 'owner' represents the selected entity.
                var selected = owner;

                bool hasTalent = selected.Facts.List.Any(id => id.Blueprint.AssetGuidThreadSafe == hwTalent);
                if (!hasTalent) return;

                if (__instance.Blueprint.GetType() != typeof(BlueprintItemWeapon)) return;

                var item = __instance.Blueprint as BlueprintItemWeapon;
                if (item == null) return;

                // Check for faction talent restrictions
                if (item.GetComponents<EquipmentRestrictionHasFacts>().Any(r => r.Facts.Any(f => factionTalents.Contains(f.AssetGuidThreadSafe))))
                {
                    if (CheckStatRestrictions(item, selected) && CheckProfessionalTalents(item, selected, profTalents))
                    {
                        __result = true;
                    }
                }
            }

            private static bool CheckStatRestrictions(BlueprintItemWeapon item, MechanicEntity selected)
            {
                return item.GetComponents<EquipmentRestrictionStat>().All(restriction => restriction.CanBeEquippedBy(selected));
            }

            private static bool CheckProfessionalTalents(BlueprintItemWeapon item, MechanicEntity selected, string[] profTalents)
            {
                // Check if the item has any of the professional talents
                bool itemHasAnyProfTalent = item.GetComponents<EquipmentRestrictionHasFacts>().Any(r => r.Facts.Any(f => profTalents.Contains(f.AssetGuidThreadSafe)));

                // If the item does NOT have any element from profTalents, return true (no further checks needed)
                if (!itemHasAnyProfTalent)
                {
                    return true;
                }

                // If the item does have at least one profTalent, check if selected has the matching fact
                foreach (var profTalent in profTalents)
                {
                    if (item.GetComponents<EquipmentRestrictionHasFacts>().Any(r => r.Facts.Any(f => f.AssetGuidThreadSafe == profTalent)) &&
                        selected.Facts.List.Any(id => id.Blueprint.AssetGuidThreadSafe == profTalent))
                    {
                        return true;
                    }
                }

                // If none of the profTalents that are present in the item are found in selected, return false
                return false;
            }
        }
    }
}
//bool hasRestriction = item.GetComponents<EquipmentRestriction>().Aggregate(seed: true, (bool r, EquipmentRestriction restriction) => r && restriction.CanBeEquippedBy(selected));