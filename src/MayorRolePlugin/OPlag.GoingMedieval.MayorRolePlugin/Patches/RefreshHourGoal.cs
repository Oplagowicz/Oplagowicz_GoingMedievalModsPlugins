using HarmonyLib;
using NSMedieval.Goap;
using NSMedieval.State;
using OPlag.GoingMedieval.MayorRolePlugin.Goals;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OPlag.GoingMedieval.MayorRolePlugin.Patches
{
    [HarmonyPatch(typeof(WorkerGoapAgent), "RefreshHourGoals")]
    internal static class WorkerGoapAgent_RefreshHourGoals_Patch
    {
        private static readonly FieldInfo HumanoidField =
            AccessTools.Field(typeof(WorkerGoapAgent), "humanoid");

        private static readonly PropertyInfo GoalSchedulerProperty =
            AccessTools.Property(typeof(Agent), "GoalScheduler");

        private static readonly MethodInfo ExistInPoolMethod =
            AccessTools.Method(typeof(GoalScheduler), "ExistInPool");

        private static readonly MethodInfo AddToPoolMethod =
            AccessTools.Method(typeof(GoalScheduler), "AddToPool");

        private static readonly MethodInfo SetBasePriorityMethod =
            AccessTools.Method(typeof(GoalScheduler), "SetBasePriority");

        private static readonly MethodInfo EnableGoalMethod =
            AccessTools.Method(typeof(GoalScheduler), "EnableGoal");

        private const string GoalId = "MayorRoleGoal";
        private const float GoalPriority = 0.55f;

        private static void Postfix(WorkerGoapAgent __instance)
        {
            try
            {
                HumanoidInstance? humanoid = HumanoidField.GetValue(__instance) as HumanoidInstance;
                if (humanoid == null || humanoid.HasDisposed || humanoid.HasDied)
                {
                    return;
                }

                if (!HasMayorRole(humanoid))
                {
                    return;
                }

                if (__instance.CurrentHourType != HourType.RoleJob)
                {
                    return;
                }

                object goalScheduler = GoalSchedulerProperty.GetValue(__instance);
                if (goalScheduler == null)
                {
                    MayorRolePlugin.Log?.LogError("Mayor schedule patch: GoalScheduler is null.");
                    return;
                }

                bool existsInPool = (bool)ExistInPoolMethod.Invoke(goalScheduler, new object[] { GoalId });
                if (!existsInPool)
                {
                    MayorRoleGoal goal = new MayorRoleGoal(__instance);

                    if (!goal.AgentTypeCheck())
                    {
                        MayorRolePlugin.Log?.LogError("Mayor schedule patch: AgentTypeCheck failed.");
                        return;
                    }

                    if (!goal.ShouldBeAdded())
                    {
                        MayorRolePlugin.Log?.LogInfo("Mayor schedule patch: ShouldBeAdded returned false.");
                        return;
                    }

                    GoalPriorityData goalPriorityData = new GoalPriorityData(goal, GoalPriority);
                    AddToPoolMethod.Invoke(goalScheduler, new object[] { goalPriorityData });

                    MayorRolePlugin.Log?.LogInfo("Mayor schedule patch: MayorRoleGoal added to GoalScheduler pool.");
                }

                SetBasePriorityMethod.Invoke(goalScheduler, new object[] { GoalId, GoalPriority });
                EnableGoalMethod.Invoke(goalScheduler, new object[] { GoalId });

                MayorRolePlugin.Log?.LogInfo("Mayor schedule patch: MayorRoleGoal enabled.");
            }
            catch (Exception ex)
            {
                MayorRolePlugin.Log?.LogError($"Mayor schedule patch failed: {ex}");
            }
        }

        private static bool HasMayorRole(HumanoidInstance humanoid)
        {
            if (humanoid == null)
            {
                return false;
            }

            var roleOwner = humanoid.ActiveBehaviour?.HumanoidRoleOwner;
            if (roleOwner == null || !roleOwner.AssignedRole)
            {
                return false;
            }

            var roleInstance = roleOwner.RoleInstance;
            if (roleInstance == null || roleInstance.Blueprint == null)
            {
                return false;
            }

            return string.Equals(roleInstance.Blueprint.GetID(), "mayor", StringComparison.OrdinalIgnoreCase);
        }
    }
}
