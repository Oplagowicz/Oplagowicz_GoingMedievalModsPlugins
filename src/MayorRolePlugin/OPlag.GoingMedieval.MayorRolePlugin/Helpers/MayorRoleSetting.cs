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
        private string[] onMayorInspireEffectors;

        public string[] MayorInspireEffectors
        {
            get { return this.onMayorInspireEffectors; }
        }

        public override string GetID()
        {
            return "MayorRoleSettings";
        }
    }
}
