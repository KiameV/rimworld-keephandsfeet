﻿using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using WeaponStorage;

namespace MendingWeaponStoragePatch
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(m => "MendAndRecycle".Equals(m.Name)))
            {
                var harmony = HarmonyInstance.Create("com.mendingweaponstoragepatch.rimworld.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Log.Message(
                    "MendingWeaponStoragePatch Harmony Patches:" + Environment.NewLine +
                    "  Postfix:" + Environment.NewLine +
                    "    WorkGiver_DoBill.TryFindBestBillIngredients - Priority Last");
            }
            else
            {
                Log.Message("MendingWeaponStoragePatch did not find MendAndRecycle. Will not load patch.");
            }
        }
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(MendAndRecycle.WorkGiver_DoBill), "TryFindBestBillIngredients")]
    static class Patch_WorkGiver_DoBill_TryFindBestBillIngredients
    {
        static void Postfix(ref bool __result, Bill bill, Pawn pawn, Thing billGiver, bool ignoreHitPoints, ref Thing chosen)
        {
            if (__result == false &&
                pawn != null && bill != null && bill.recipe != null &&
                bill.Map == pawn.Map &&
                bill.recipe.defName.IndexOf("Weapon") != -1)
            {
                IEnumerable<Building_WeaponStorage> dressers = WorldComp.GetWeaponStorages(bill.Map);
                if (dressers == null)
                {
                    Log.Message("MendingWeaponStoragePatch failed to retrieve WeaponStorages");
                    return;
                }

                foreach (Building_WeaponStorage ws in dressers)
                {
                    if ((float)(ws.Position - billGiver.Position).LengthHorizontalSquared < bill.ingredientSearchRadius * bill.ingredientSearchRadius)
                    {
                        foreach (ThingWithComps t in ws.StoredWeapons)
                        {
                            if (bill.ingredientFilter.Allows(t) && 
                                t.HitPoints != t.MaxHitPoints)
                            {
                                ws.Remove(t, false);
                                if (t.Spawned == false)
                                {
                                    Log.Error("Failed to spawn weapon-to-mend [" + t.Label + "] from weapon storage [" + ws.Label + "].");
                                    __result = false;
                                    chosen = null;
                                }
                                else
                                {
                                    __result = true;
                                    chosen = t;
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}