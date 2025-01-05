using HarmonyLib;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Items;
using Kingmaker.EntitySystem.Entities;
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

                var selected = owner;

                bool hasTalent = selected.Facts.List.Any(id => id.Blueprint.AssetGuidThreadSafe == hwTalent);
                if (!hasTalent) return;

                if (__instance.Blueprint.GetType() != typeof(BlueprintItemWeapon)) return;

                var item = __instance.Blueprint as BlueprintItemWeapon;
                if (item == null) return;

                if (item.GetComponents<EquipmentRestrictionHasFacts>().Any(r => r.Facts.Any(f => factionTalents.Contains(f.AssetGuidThreadSafe))))
                {
                    if (CheckStatRestrictions(item, selected) && CheckProfTalents(item, selected, profTalents))
                    {
                        __result = true;
                    }
                }
            }

            private static bool CheckStatRestrictions(BlueprintItemWeapon item, MechanicEntity selected)
            {
                return item.GetComponents<EquipmentRestrictionStat>().All(restriction => restriction.CanBeEquippedBy(selected));
            }

            private static bool CheckProfTalents(BlueprintItemWeapon item, MechanicEntity selected, string[] profTalents)
            {
                bool itemHasAnyProfTalent = item.GetComponents<EquipmentRestrictionHasFacts>().Any(r => r.Facts.Any(f => profTalents.Contains(f.AssetGuidThreadSafe)));

                if (!itemHasAnyProfTalent)
                {
                    return true;
                }

                foreach (var profTalent in profTalents)
                {
                    if (item.GetComponents<EquipmentRestrictionHasFacts>().Any(r => r.Facts.Any(f => f.AssetGuidThreadSafe == profTalent)) &&
                        selected.Facts.List.Any(id => id.Blueprint.AssetGuidThreadSafe == profTalent))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}