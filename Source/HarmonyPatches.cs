using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace WeaponStorage
{
    [StaticConstructorOnStartup]
    partial class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("com.keephandsfeet.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message(
                "KeepHandsFeet Harmony Patches:" + Environment.NewLine +
                "  Prefix:" + Environment.NewLine +
                "    MainMenuDrawer.MainMenuOnGUI");
        }
    }

    [HarmonyPatch(typeof(MainMenuDrawer), "MainMenuOnGUI")]
    static class Patch_MainMenuDrawer_MainMenuOnGUI
    {
        private static bool initialized = false;
        static void Prefix()
        {
            if (!initialized)
            {
                initialized = true;
                foreach (BodyDef d in DefDatabase<BodyDef>.AllDefs)
                {
                    if (d.defName.ToLower().Equals("monkey"))
                        continue;

                    BodyPartRecord rightArm = null,
                                   leftArm = null,
                                   rightHand = null,
                                   leftHand = null,
                                   leftLeg = null,
                                   leftFoot = null,
                                   rightLeg = null,
                                   rightFoot = null,
                                   torso = null;
                    foreach (BodyPartRecord part in d.AllParts)
                    {
                        switch (part.Label.ToLower())
                        {
                            case "right arm":
                                rightArm = part;
                                break;
                            case "left arm":
                                leftArm = part;
                                break;
                            case "right hand":
                                rightHand = part;
                                break;
                            case "left hand":
                                leftHand = part;
                                break;
                            case "right leg":
                                rightLeg = part;
                                break;
                            case "right foot":
                                rightFoot = part;
                                break;
                            case "left leg":
                                leftLeg = part;
                                break;
                            case "left foot":
                                leftFoot = part;
                                break;
                            case "torso":
                                torso = part;
                                break;
                        }
                    }

                    if (rightArm != null && rightHand != null &&
                        leftArm != null && leftHand != null &&
                        rightLeg != null && rightFoot != null &&
                        leftLeg != null && leftFoot != null &&
                        torso != null)
                    {
                        Log.Message("[" + d.defName + "] Moving Hands and Feet so they're not be tied to Arms and Legs");

                        if (rightArm.parts.Remove(rightHand))
                            torso.parts.Add(rightHand);
                        else
                            Log.Warning("[" + d.defName + "] Unable to change Right Foot location");

                        if (leftArm.parts.Remove(leftHand))
                            torso.parts.Add(leftHand);
                        else
                            Log.Warning("[" + d.defName + "] Unable to change Left Hand location");

                        if (rightLeg.parts.Remove(rightFoot))
                            torso.parts.Add(rightFoot);
                        else
                            Log.Warning("[" + d.defName + "] Unable to change Right Foot location");

                        if (leftLeg.parts.Remove(leftFoot))
                            torso.parts.Add(leftFoot);
                        else
                            Log.Warning("[" + d.defName + "] Unable to change Left Foot location");
                    }
                }
            }
        }
    }
}