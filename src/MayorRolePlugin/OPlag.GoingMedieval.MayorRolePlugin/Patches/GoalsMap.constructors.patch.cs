using HarmonyLib;
using NSMedieval.Goap;
using OPlag.GoingMedieval.MayorRolePlugin.Goals;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OPlag.GoingMedieval.MayorRolePlugin.Patches
{
    [HarmonyPatch(typeof(GoalsMap), nameof(GoalsMap.Constuctors), MethodType.Getter)]
    internal static class GoalsMap_Constructors_Patch
    {
        private static void Postfix(ref Dictionary<string, ConstructorInfo> __result)
        {
            if (__result == null)
            {
                MayorRolePlugin.Log?.LogError("GoalsMap.Constuctors returned null.");
                return;
            }

            const string goalId = "MayorRoleGoal";

            if (__result.ContainsKey(goalId))
            {
                return;
            }

            ConstructorInfo ctor = AccessTools.Constructor(
                typeof(MayorRoleGoal),
                new[] { typeof(Agent) }
            );

            if (ctor == null)
            {

                MayorRolePlugin.Log?.LogError("Constructor for MayorRoleGoal(Agent) not found.");
                return;
            }

            __result.Add(goalId, ctor);
            MayorRolePlugin.Log?.LogInfo("MayorRoleGoal registered in GoalsMap.Constuctors.");
        }
    }
}
