using HarmonyLib;
using NSMedieval.Goap;
using NSMedieval.State;
using OPlag.GoingMedieval.MayorRolePlugin.Goals;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OPlag.GoingMedieval.MayorRolePlugin.Patches
{
    [HarmonyPatch(typeof(WorkerGoapAgent), "Tick")]
    internal static class WorkerGoapAgent_Tick_MayorForcePatch
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
        private const float GoalPriority = 999f;
        private const float RetrySeconds = 3f;

        private static readonly Dictionary<int, float> LastForceTimeByWorkerId = new Dictionary<int, float>();

        private static void Postfix(WorkerGoapAgent __instance, float deltaTime)
        {
            try
            {
                HumanoidInstance? humanoid = HumanoidField.GetValue(__instance) as HumanoidInstance;
                if (humanoid == null || humanoid.HasDisposed || humanoid.HasDied)
                {
                    return;
                }

                var roleOwner = humanoid.ActiveBehaviour?.HumanoidRoleOwner;
                var roleInstance = roleOwner?.RoleInstance;
                string roleId = roleInstance?.Blueprint?.GetID() ?? "<null>";

                if (roleOwner == null || !roleOwner.AssignedRole || !string.Equals(roleId, "mayor", StringComparison.OrdinalIgnoreCase))
                {
                    LastForceTimeByWorkerId.Remove(humanoid.UniqueId);
                    return;
                }

                object goalScheduler = GoalSchedulerProperty.GetValue(__instance);
                if (goalScheduler == null)
                {
                    MayorRolePlugin.Log?.LogError($"MayorForcePatch: GoalScheduler is null for {humanoid.GetFullName()}");
                    return;
                }

                bool existsInPool = (bool)ExistInPoolMethod.Invoke(goalScheduler, new object[] { GoalId });
                if (!existsInPool)
                {
                    MayorRoleGoal goal = new MayorRoleGoal(__instance);

                    if (!goal.AgentTypeCheck())
                    {
                        MayorRolePlugin.Log?.LogError($"MayorForcePatch: AgentTypeCheck failed for {humanoid.GetFullName()}");
                        return;
                    }

                    if (!goal.ShouldBeAdded())
                    {
                        MayorRolePlugin.Log?.LogInfo($"MayorForcePatch: ShouldBeAdded returned false for {humanoid.GetFullName()}");
                        return;
                    }

                    GoalPriorityData goalPriorityData = new GoalPriorityData(goal, GoalPriority);
                    AddToPoolMethod.Invoke(goalScheduler, new object[] { goalPriorityData });

                    MayorRolePlugin.Log?.LogInfo($"MayorForcePatch: added MayorRoleGoal to pool for {humanoid.GetFullName()}");
                }

                SetBasePriorityMethod.Invoke(goalScheduler, new object[] { GoalId, GoalPriority });
                EnableGoalMethod.Invoke(goalScheduler, new object[] { GoalId });

                float now = Time.time;
                if (!LastForceTimeByWorkerId.TryGetValue(humanoid.UniqueId, out float lastForce) || now - lastForce >= RetrySeconds)
                {
                    LastForceTimeByWorkerId[humanoid.UniqueId] = now;

                    MayorRolePlugin.Log?.LogInfo(
                        $"MayorForcePatch: forcing MayorRoleGoal for {humanoid.GetFullName()}, roleId={roleId}");

                    __instance.ForceNextGoalExclusive(GoalId);
                }
            }
            catch (Exception ex)
            {
                MayorRolePlugin.Log?.LogError($"MayorForcePatch failed: {ex}");
            }
        }
    }
}
