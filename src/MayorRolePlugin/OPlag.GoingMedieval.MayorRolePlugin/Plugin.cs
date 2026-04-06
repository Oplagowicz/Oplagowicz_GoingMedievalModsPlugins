using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPlag.GoingMedieval.MayorRolePlugin.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OPlag.GoingMedieval.MayorRolePlugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MayorRolePlugin : BaseUnityPlugin
    {
        public const string pluginGuid = "oplag.goingmedieval.mayorrole";
        public const string pluginName = "Mayor Role Plugin";
        public const string pluginVersion = "0.9.5";
        internal static ManualLogSource? Log;
        private Harmony? _harmony;

        private void Awake()
        {
            Log = Logger;

            _harmony = new Harmony(pluginGuid);
            _harmony.PatchAll();

            Log.LogInfo("--> !Mayor Role Plugin loaded.! <--");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
