using NSMedieval.Goap;
using NSMedieval.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace OPlag.GoingMedieval.MayorRolePlugin.Helpers
{
    public static class MayorTargetHelper
    {
        public static bool IsValidWorkerTarget(WorkerBehaviour worker, HumanoidInstance mayor)
        {
            if (worker == null || worker.Humanoid == null)
                return false;

            if (worker.Humanoid == mayor)
                return false;

            if (worker.Humanoid.HasDisposed || worker.Humanoid.HasDied || worker.Humanoid.HasFainted)
                return false;

            return true;
        }

        public static bool IsValidHourType(HourType hourType)
        {
            return hourType == HourType.Any || hourType == HourType.Working;
        }
    }
}
