using NSEipix.Base;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OPlag.GoingMedieval.MayorRolePlugin.Helpers
{
    [Serializable]
    public class MayorRoleSettings : Model
    {
        [SerializeField]
        private string[] onMayorInspireEffectorsLvl1 = Array.Empty<string>();

        [SerializeField]
        private string[] onMayorInspireEffectorsLvl2 = Array.Empty<string>();

        [SerializeField]
        private string[] onMayorInspireEffectorsLvl3 = Array.Empty<string>();

        public string[] GetMayorInspireEffectorsForRuntimeLevel(int runtimeLevel)
        {
            return runtimeLevel switch
            {
                0 => onMayorInspireEffectorsLvl1,
                1 => onMayorInspireEffectorsLvl2,
                2 => onMayorInspireEffectorsLvl3,
                _ => onMayorInspireEffectorsLvl1
            };
        }

        public override string GetID()
        {
            return "MayorRoleSettings";
        }
    }
}
