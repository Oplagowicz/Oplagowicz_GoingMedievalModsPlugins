using NSEipix.Repository;
using NSMedieval.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace OPlag.GoingMedieval.MayorRolePlugin.Helpers
{


	public class MayorRoleSettingsData : DynamicSettingsData<MayorRoleSettingsData, MayorRoleSettings>
    {
        protected override string JsonFile()
        {
            return "Models/MayorRoleSettings.json";
        }
    }
}
