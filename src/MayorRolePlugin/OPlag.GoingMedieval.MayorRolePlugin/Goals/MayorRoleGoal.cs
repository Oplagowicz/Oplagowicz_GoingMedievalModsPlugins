using NSEipix.Base;
using NSMedieval;
using NSMedieval.Controllers;
using NSMedieval.Goap;
using NSMedieval.Goap.Actions;
using NSMedieval.Goap.Goals;
using NSMedieval.Manager;
using NSMedieval.Pathfinding;
using NSMedieval.Roles;
using NSMedieval.RoomDetection;
using NSMedieval.State;
using NSMedieval.Utils.Pool.Janitors;
using OPlag.GoingMedieval.MayorRolePlugin.Helpers;
using OPlag.GoingMedieval.MayorRolePlugin.Services;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OPlag.GoingMedieval.MayorRolePlugin.Goals
{
    public class MayorRoleGoal : RoleGoal
    {
        private const uint VisitHoursCooldown = 3U;
        private WorkerBehaviour? workerTarget;

        public MayorRoleGoal(Agent selfAgent)
            : base("MayorRoleGoal", selfAgent, GoalInterruptMode.HigherPriority)
        {
            base.AllowedRoleId = "mayor";
        }

        protected override IEnumerable<GoapAction> GetNextAction()
        {
            yield return this.GotoAction();
            yield return this.MayorInspire();
            yield return this.IdleAction();
        }

        public override bool CanStart(bool isForced = false)
        {
            return base.CanStart(isForced) && this.ValidateNextTarget();
        }

        protected override bool PrepareData()
        {
            CreatureBase creatureBase = (CreatureBase)base.AgentOwner;
            if (creatureBase is AnimalInstance)
            {
                MayorRolePlugin.Log?.LogInfo("MayorRoleGoal.PrepareData: agent is animal, abort.");
                return false;
            }

            HumanoidInstance? humanoidInstance = creatureBase as HumanoidInstance;
            if (humanoidInstance == null)
            {
                MayorRolePlugin.Log?.LogInfo("MayorRoleGoal.PrepareData: humanoid is null.");
                return false;
            }

            if (!humanoidInstance.WorkerBehaviour.HumanoidRoleOwner.AssignedRole)
            {
                MayorRolePlugin.Log?.LogInfo("MayorRoleGoal.PrepareData: no assigned role.");
                return false;
            }

            RoleInstance roleInstance = humanoidInstance.ActiveBehaviour.HumanoidRoleOwner.RoleInstance;
            if (roleInstance == null)
            {
                MayorRolePlugin.Log?.LogInfo("MayorRoleGoal.PrepareData: roleInstance is null.");
                return false;
            }

            if (roleInstance.Blueprint == null || !roleInstance.Blueprint.GetID().Equals(this.AllowedRoleId))
            {
                MayorRolePlugin.Log?.LogInfo($"MayorRoleGoal.PrepareData: role mismatch, got '{roleInstance.Blueprint?.GetID()}'.");
                return false;
            }

            bool valid = this.ValidateNextTarget();
            MayorRolePlugin.Log?.LogInfo($"MayorRoleGoal.PrepareData: target valid = {valid}");
            return valid;
        }

        private GoapAction IdleAction()
        {
            return new GoapAction("IdleAction");
        }

        private GoapAction GotoAction()
        {
            GoapAction goapAction = GoToActions
                .GoToTarget(TargetIndex.A, PathCompleteMode.ExactPosition, default(Vector3))
                .FailIfTargetDisposedOrNull(TargetIndex.A)
                .FailAtCondition(new Func<bool>(base.RoleNotAssigned), false);

            goapAction.OnInit = delegate
            {
                base.EquipProp(true);
            };

            goapAction.OnComplete = delegate (ActionCompletionStatus state)
            {
                MonoSingleton<AnimationController>.Instance.ForceQuitAgentAnimation(base.AgentOwner);
                base.EquipProp(false);

                if (state != ActionCompletionStatus.Success)
                {
                    return;
                }

                if (base.GetTarget(TargetIndex.A).ObjectInstance != null)
                {
                    base.HumanoidInstance.FaceObject(base.GetTarget(TargetIndex.A).ObjectInstance.GetPosition());
                }
            };

            return goapAction;
        }

        private GoapAction MayorInspire()
        {
            GoapAction goapAction = new GoapAction("MayorInspire");
            float duration = UnityEngine.Random.value * 10f + 5f;

            goapAction
                .TriggerAnimation("MayorIdle", ActionAnimationMode.Interrupt, false)
                .CompleteAfterTimeExpires(duration)
                .FailAtCondition(new Func<bool>(base.RoleNotAssigned), false);

            goapAction.OnInit = delegate
            {
                if (!this.ValidateNextTarget())
                {
                    MayorRolePlugin.Log?.LogInfo("MayorRoleGoal.MayorInspire: target validation failed before inspire.");
                    return;
                }

                if (this.workerTarget == null || this.workerTarget.Humanoid == null)
                {
                    MayorRolePlugin.Log?.LogInfo("MayorRoleGoal.MayorInspire: workerTarget is null after validation.");
                    return;
                }

                bool applied = MayorInspireService.TryApplyMayorInspire(base.HumanoidInstance, this.workerTarget);
                MayorRolePlugin.Log?.LogInfo(
                    $"MayorRoleGoal.MayorInspire: applied = {applied}, target = {this.workerTarget.Humanoid.GetFullName()}");
            };

            goapAction.OnComplete = delegate (ActionCompletionStatus state)
            {
                MonoSingleton<AnimationController>.Instance.ForceQuitAgentAnimation(base.AgentOwner);
                MonoSingleton<AnimationController>.Instance.SetAnimatorParameter(base.AgentOwner, "MayorIdle", false);
                base.EquipProp(false);
            };

            return goapAction;
        }

        protected override void AssignGoToTarget(Room room)
        {
            this.ValidateNextTarget();
        }

        private bool IsValidWorkerTarget(WorkerBehaviour? worker)
        {
            if (worker == null || worker.Humanoid == null)
            {
                return false;
            }

            if (worker.Humanoid == this.HumanoidInstance)
            {
                return false;
            }

            if (worker.Humanoid.HasDisposed || worker.Humanoid.HasDied || worker.Humanoid.HasFainted)
            {
                return false;
            }

            if (worker.WorkerGoapAgent == null)
            {
                return false;
            }

            if (worker.WorkerGoapAgent.CurrentHourType != HourType.Any &&
                worker.WorkerGoapAgent.CurrentHourType != HourType.Working)
            {
                return false;
            }

            uint threshold = GlobalSaveController.CurrentVillageData.DateAndTime.HoursTotal - VisitHoursCooldown;
            if (MayorLastVisitHelper.GetLastVisitHour(worker) >= threshold)
            {
                return false;
            }

            return true;
        }

        private WorkerBehaviour? ResolveWorkerTargetFromTargetA()
        {
            var target = base.GetTarget(TargetIndex.A);
            if (!target.IsInitialized || target.ObjectInstance == null)
            {
                return null;
            }

            HumanoidInstance? targetHumanoid = target.ObjectInstance as HumanoidInstance;
            if (targetHumanoid == null)
            {
                return null;
            }

            using (PooledList<WorkerBehaviour> npcsPooled =
                MonoSingleton<NPCManager>.Instance.GetNPCsPooled<WorkerBehaviour>(null))
            {
                foreach (WorkerBehaviour w in npcsPooled)
                {
                    if (w != null && w.Humanoid == targetHumanoid)
                    {
                        return w;
                    }
                }
            }

            return null;
        }

        protected bool ValidateNextTarget()
        {
            WorkerBehaviour? existingWorker = this.ResolveWorkerTargetFromTargetA();
            if (this.IsValidWorkerTarget(existingWorker))
            {
                this.workerTarget = existingWorker;
                MayorRolePlugin.Log?.LogInfo(
                    $"MayorRoleGoal.ValidateNextTarget: reusing existing target = {this.workerTarget.Humanoid.GetFullName()}");
                return true;
            }

            this.workerTarget = null;
            base.SetTarget(TargetIndex.A, default(TargetObject), false);

            uint threshold = GlobalSaveController.CurrentVillageData.DateAndTime.HoursTotal - VisitHoursCooldown;

            using (PooledList<WorkerBehaviour> npcsPooled =
                MonoSingleton<NPCManager>.Instance.GetNPCsPooled<WorkerBehaviour>(null))
            {
                npcsPooled.Sort((a, b) =>
                    MayorLastVisitHelper.GetLastVisitHour(a).CompareTo(MayorLastVisitHelper.GetLastVisitHour(b)));

                foreach (WorkerBehaviour w in npcsPooled)
                {
                    if (!this.IsValidWorkerTarget(w))
                    {
                        continue;
                    }

                    this.workerTarget = w;
                    break;
                }

                if (this.workerTarget == null)
                {
                    MayorRolePlugin.Log?.LogInfo("MayorRoleGoal.ValidateNextTarget: no valid worker target found.");
                    return false;
                }

                base.SetTarget(TargetIndex.A, new TargetObject(this.workerTarget.Humanoid), false);
                MayorRolePlugin.Log?.LogInfo(
                    $"MayorRoleGoal.ValidateNextTarget: assigned target = {this.workerTarget.Humanoid.GetFullName()}");
                return true;
            }
        }
    }
}