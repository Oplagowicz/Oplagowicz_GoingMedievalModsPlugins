using NSMedieval;
using NSMedieval.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace OPlag.GoingMedieval.MayorRolePlugin.Helpers
{
    public static class MayorLastVisitHelper
    {
        private static readonly Dictionary<string, uint> lastVisitedByMayor = new Dictionary<string, uint>();

        public static uint GetCurrentHour()
        {
            return GlobalSaveController.CurrentVillageData.DateAndTime.HoursTotal;
        }

        public static uint GetLastVisitHour(WorkerBehaviour worker)
        {
            if (worker == null || worker.Humanoid == null)
            {
                return 0;
            }

            string key = GetWorkerKey(worker);

            if (lastVisitedByMayor.TryGetValue(key, out uint value))
            {
                return value;
            }

            return 0;
        }

        public static void SetLastVisitHour(WorkerBehaviour worker, uint hour)
        {
            if (worker == null || worker.Humanoid == null)
            {
                return;
            }

            string key = GetWorkerKey(worker);
            lastVisitedByMayor[key] = hour;
        }

        public static string GetWorkerKey(WorkerBehaviour worker)
        {
            return worker.Humanoid.GetFullName();
        }
    }
}

