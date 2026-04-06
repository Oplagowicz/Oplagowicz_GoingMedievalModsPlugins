using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using UnityEngine;

namespace OPlag.GoingMedieval.MayorRolePlugin
{
    [BepInPlugin(plugGuid, plugName, version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string plugGuid = "com.oplag.goingmedieval.mayorroleplugin";
        public const string plugName = "Mayor Role Plugin";
        public const string version = "0.1.0";
        private void Awake()
        {
            Logger.LogInfo("!--> Mayor Role Plugin loaded. <--!");
        }
    }
}
