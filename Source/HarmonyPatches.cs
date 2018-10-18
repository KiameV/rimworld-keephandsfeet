using Harmony;
using RimWorld;
using System;
using System.Reflection;
using System.Text;
using Verse;

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
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("KeepHandsFeet trying to relocate hands and feet:");
                foreach (BodyDef d in DefDatabase<BodyDef>.AllDefs)
                {
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

                    bool handsMoved = false, feetMoved = false;
                    if (rightArm != null && rightHand != null &&
                        leftArm != null && leftHand != null &&
                        torso != null)
                    {
                        handsMoved = true;
                        if (rightArm.parts.Remove(rightHand))
                            torso.parts.Add(rightHand);
                        else
                            Log.Warning("[" + d.defName + "] Unable to change Right Foot location");

                        if (leftArm.parts.Remove(leftHand))
                            torso.parts.Add(leftHand);
                        else
                            Log.Warning("[" + d.defName + "] Unable to change Left Hand location");
                    }

                    if (rightLeg != null && rightFoot != null &&
                        leftLeg != null && leftFoot != null &&
                        torso != null)
                    {
                        feetMoved = true;
                        if (rightLeg.parts.Remove(rightFoot))
                            torso.parts.Add(rightFoot);
                        else
                            Log.Warning("[" + d.defName + "] Unable to change Right Foot location");

                        if (leftLeg.parts.Remove(leftFoot))
                            torso.parts.Add(leftFoot);
                        else
                            Log.Warning("[" + d.defName + "] Unable to change Left Foot location");
                    }

                    if (handsMoved || feetMoved)
                    {
                        sb.Append("    [");
                        sb.Append(d.defName);
                        sb.Append("]");
                        if (handsMoved)
                            sb.Append(" hands");
                        if (handsMoved && feetMoved)
                            sb.Append(" and");
                        if (feetMoved)
                            sb.Append(" feet");
                        sb.Append(" moved.");
                        sb.Append(Environment.NewLine);
                    }
                }
                Log.Message(sb.ToString());
            }
        }
    }
}