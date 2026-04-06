using NSEipix.Base;
using NSMedieval;
using NSMedieval.Goap;
using NSMedieval.Roles;
using NSMedieval.State;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OPlag.GoingMedieval.MayorRolePlugin.Helpers
{
    public class MayorInspireHelper
    {
        public string[] MayorInspireEffector
        {
            get
            {
                return this.onLabourerProximityEnterEffectors;
            }
        }
        public IEnumerable<string> GetMayorInspireEffectors()
        {
            return this.LabourerProximityInteractEffectors;
        }
        public void OnMayorInspire(WorkerBehaviour workerBehaviour)
        {
            if (workerBehaviour == null)
            {
                return;
            }
            if (!workerBehaviour.HumanoidRoleOwner.HasRole("major"))
            {
                return;
            }
            if (workerBehaviour.WorkerGoapAgent.CurrentHourType == HourType.Working)
            {
                this.MayorLastVisitHelper.lastVisitedByMayor = GlobalSaveController.CurrentVillageData.DateAndTime.HoursTotal;
            }
            base.Humanoid.Stats.StartEffectors(MonoSingleton<RoleManager>.Instance.GetLabourerProximityInteractEffectors());
        }
        public List<RoleEffectorData> onMayorInspireEffectors
        {
            get
            {
                List<RoleEffectorData> list;
                if ((list = this.onMayorInspireEffectors) == null)
                {
                    list = (this.onMayorInspireEffectors = new List<RoleEffectorData>());
                }
                return list;
            }
            set
            {
                this.onMayorInspireEffectors = value;
            }
        }
    }
}
