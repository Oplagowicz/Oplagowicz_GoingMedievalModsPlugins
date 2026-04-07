using NSEipix.Repository;
using NSMedieval.State;
using OPlag.GoingMedieval.MayorRolePlugin.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPlag.GoingMedieval.MayorRolePlugin.Services
{
    public static class MayorInspireService
    {
        private static MayorRoleSettings? _settings;

        private static MayorRoleSettings GetSettings()
        {
            if (_settings != null)
            {
                return _settings;
            }

            var log = MayorRolePlugin.Log ?? throw new InvalidOperationException("MayorRolePlugin.Log is not initialized. Ensure the plugin is initialized before calling GetSettings().");
            _settings = MayorModSettingsLoader.Load(log) ?? throw new InvalidOperationException("Failed to load MayorRoleSettings.");
            return _settings;
        }
    

        public static IEnumerable<string> GetMayorInspireEffectors(HumanoidInstance mayor)
        {
            if (mayor == null)
                return Array.Empty<string>();

            var roleOwner = mayor.ActiveBehaviour?.HumanoidRoleOwner;
            var roleInstance = roleOwner?.RoleInstance;

            if (roleInstance == null)
                return Array.Empty<string>();

            if (roleInstance.Blueprint == null || roleInstance.Blueprint.GetID() != "mayor")
                return Array.Empty<string>();

            var settings = GetSettings();
            if (settings == null)
                return Array.Empty<string>();

            return settings.GetMayorInspireEffectorsForRuntimeLevel(roleInstance.Level);
        }

        public static bool TryApplyMayorInspire(HumanoidInstance mayor, WorkerBehaviour target)
        {
            if (mayor == null || target == null || target.Humanoid == null)
                return false;

            var effectors = GetMayorInspireEffectors(mayor)?.ToArray();

            if (effectors == null || effectors.Length == 0)
                return false;

            target.Humanoid.Stats.StartEffectors(effectors);
            MayorLastVisitHelper.SetLastVisitHour(target, MayorLastVisitHelper.GetCurrentHour());

            return true;
        }
    }
}
