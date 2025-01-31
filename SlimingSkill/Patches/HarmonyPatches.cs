using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BirbShared;
using HarmonyLib;
using SpaceCore;
using StardewValley;
using StardewValley.Monsters;

namespace SlimingSkill
{
    /// <summary>
    /// Gain XP from killing slimes
    /// </summary>
    [HarmonyPatch]
    class Slime_TakeDamage_Transpiler
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(BigSlime), nameof(BigSlime.takeDamage), new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) });
            yield return AccessTools.Method(typeof(GreenSlime), nameof(GreenSlime.takeDamage), new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) });
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instr in instructions)
            {
                if (instr.Is(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Stats), nameof(Stats.SlimesKilled))))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Slime_TakeDamage_Transpiler), nameof(Slime_TakeDamage_Transpiler.DoDeathXp)));
                }
                yield return instr;
            }
        }

        public static void DoDeathXp(Monster monster)
        {
            int xp = ModEntry.Config.ExperienceFromSlimeKill;
            if (monster is BigSlime bigSlime)
            {
                xp = ModEntry.Config.ExperienceFromToughSlimeKill;
            }
            else if (monster is GreenSlime slime)
            {
                if (slime.hasSpecialItem.Value || slime.prismatic.Value)
                {
                    xp = ModEntry.Config.ExperienceFromRareSlimeKill;
                }
                else if (slime.Name == "Tiger Slime" || slime.Name == "Sludge")
                {
                    xp = ModEntry.Config.ExperienceFromToughSlimeKill;
                }
            }
            // TODO: iron/lime/black slimes could be considered tough

            Skills.AddExperience(Game1.player, SlimingSkill.ID, xp);
        }
    }

    /// <summary>
    /// Gain XP from incubating slime eggs
    /// Rancher
    ///   - reduce incubation time
    /// </summary>
    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.performObjectDropInAction))]
    class Object_PerformObjectDropInAction
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int index = instructions.FindBCloseToA(new CodeInstruction(OpCodes.Ldstr, "coin"), new CodeInstruction(OpCodes.Ldc_I4, 4000));
            instructions = instructions.InsertAfterIndex(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Object_PerformObjectDropInAction), nameof(Object_PerformObjectDropInAction.IncubateSlimeEgg))),
            }, index + 1);

            index = instructions.FindBCloseToA(new CodeInstruction(OpCodes.Ldstr, "slimeHit"), new CodeInstruction(OpCodes.Ldstr, "bubbles"));
            return instructions.InsertAfterIndex(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Object_PerformObjectDropInAction), nameof(Object_PerformObjectDropInAction.PressSlimeEgg))),
            }, index);
        }

        public static void IncubateSlimeEgg(StardewValley.Object incubator)
        {
            Skills.AddExperience(Game1.player, SlimingSkill.ID, ModEntry.Config.ExperienceFromSlimeEgg);
            // TODO: Reduce incubation time;  Maybe undo vanilla (and MARGO?) professions decrease of incubation time;
        }

        public static void PressSlimeEgg(StardewValley.Object press)
        {
            Skills.AddExperience(Game1.player, SlimingSkill.ID, ModEntry.Config.ExperienceFromSlimeEgg);
        }
    }

    /// <summary>
    /// Gain XP from opening a slime ball
    /// Hatcher Profession
    ///   - gain extra drops
    /// </summary>
    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.checkForAction))]
    class Object_CheckForAction
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instr in instructions)
            {
                if (instr.Is(OpCodes.Ldstr, "slimedead"))
                {
                    
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Object_CheckForAction), nameof(Object_CheckForAction.OpenSlimeball)));
                }
                yield return instr;
            }
        }

        public static void OpenSlimeball(StardewValley.Object slimeball)
        {
            Skills.AddExperience(Game1.player, SlimingSkill.ID, ModEntry.Config.ExperienceFromSlimeBall);
            // TODO: Hatcher profession bonuses
        }
    }
}
