using NSEipix.Base;
using NSMedieval;
using NSMedieval.Controllers;
using NSMedieval.Goap;
using NSMedieval.Goap.Actions;
using NSMedieval.Goap.Goals;
using NSMedieval.Manager;
using NSMedieval.Pathfinding;
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
         yield break;
        }

        private GoapAction IdleAction()
        {
            return new GoapAction("IdleAction");
        }

        private GoapAction GotoAction()
        {
            GoapAction goapAction = GoToActions.GoToTarget(TargetIndex.A, PathCompleteMode.ExactPosition, default(Vector3)).FailIfTargetDisposedOrNull(TargetIndex.A).FailAtCondition(new Func<bool>(base.RoleNotAssigned), false);
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
                base.HumanoidInstance.FaceObject(base.GetTarget(TargetIndex.A).ObjectInstance.GetPosition());
            };
            return goapAction;
        }

        public override bool CanStart(bool isForced = false)
        {
            return base.CanStart(false) && this.ValidateNextTarget();
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
                if (this.workerTarget != null && this.workerTarget.Humanoid != null)
                {
                    MayorInspireService.TryApplyMayorInspire(base.HumanoidInstance, this.workerTarget);
                }
            };

            goapAction.OnComplete = delegate (ActionCompletionStatus state)
            {
                MonoSingleton<AnimationController>.Instance.ForceQuitAgentAnimation(base.AgentOwner);
                MonoSingleton<AnimationController>.Instance.SetAnimatorParameter(base.AgentOwner, "MayorIdle", false);
                base.EquipProp(false);
            };

            return goapAction;
        }

        private bool ValidateNextTarget()
        {
            if (base.GetTarget(TargetIndex.A).ObjectInstance != null)
            {
                return true;
            }

            this.workerTarget = null;
            uint threshold = GlobalSaveController.CurrentVillageData.DateAndTime.HoursTotal - VisitHoursCooldown;

            using (PooledList<WorkerBehaviour> npcsPooled =
                   MonoSingleton<NPCManager>.Instance.GetNPCsPooled<WorkerBehaviour>(null))
            {
                npcsPooled.Sort((a, b) =>
                    MayorLastVisitHelper.GetLastVisitHour(a).CompareTo(MayorLastVisitHelper.GetLastVisitHour(b)));

                foreach (WorkerBehaviour w in npcsPooled)
                {
                    if (w == null)
                    {
                        continue;
                    }

                    if (w.Humanoid == null)
                    {
                        continue;
                    }

                    if (w.Humanoid == this.HumanoidInstance)
                    {
                        continue;
                    }

                    if (w.Humanoid.HasDisposed || w.Humanoid.HasDied || w.Humanoid.HasFainted)
                    {
                        continue;
                    }

                    if (MayorLastVisitHelper.GetLastVisitHour(w) >= threshold)
                    {
                        continue;
                    }

                    if (w.WorkerGoapAgent.CurrentHourType != HourType.Any &&
                        w.WorkerGoapAgent.CurrentHourType != HourType.Working)
                    {
                        continue;
                    }

                    this.workerTarget = w;
                    break;
                }

                if (this.workerTarget == null)
                {
                    return false;
                }

                base.SetTarget(TargetIndex.A, new TargetObject(this.workerTarget.Humanoid), false);
                return true;
            }
        }

        private const uint VisitHoursCooldown = 3U;

        private WorkerBehaviour? workerTarget;


    }
}
