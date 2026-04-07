using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace OPlag.GoingMedieval.MayorRolePlugin.Helpers
{
    public static class MayorModSettingsLoader
    {
        private const string TargetModId = "oplagowicz.mayorrolemod";
        private const string SettingsRelativePath = "Data/Models/MayorRoleSettings.json";

        public static MayorRoleSettings? Load(ManualLogSource log)
        {
            try
            {
                string modsRoot = GetModsRootPath();
                if (string.IsNullOrWhiteSpace(modsRoot) || !Directory.Exists(modsRoot))
                {
                    log.LogError($"Mods folder not found: {modsRoot}");
                    return null;
                }

                string? modFolder = FindModFolderById(modsRoot, TargetModId, log);
                if (string.IsNullOrWhiteSpace(modFolder))
                {
                    log.LogError($"Could not find mod with id '{TargetModId}' in: {modsRoot}");
                    return null;
                }

                string settingsPath = Path.Combine(
                    modFolder,
                    SettingsRelativePath.Replace('/', Path.DirectorySeparatorChar)
                );

                if (!File.Exists(settingsPath))
                {
                    log.LogError($"MayorRoleSettings.json not found: {settingsPath}");
                    return null;
                }

                string json = File.ReadAllText(settingsPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    log.LogError($"MayorRoleSettings.json is empty: {settingsPath}");
                    return null;
                }

                MayorRoleSettings settings = JsonUtility.FromJson<MayorRoleSettings>(json);
                if (settings == null)
                {
                    log.LogError($"Failed to deserialize MayorRoleSettings.json: {settingsPath}");
                    return null;
                }

                log.LogInfo($"MayorRoleSettings loaded successfully from: {settingsPath}");
                return settings;
            }
            catch (Exception ex)
            {
                log.LogError($"Exception while loading MayorRoleSettings.json: {ex}");
                return null;
            }
        }

        private static string GetModsRootPath()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, "Foxy Voxel", "Going Medieval", "Mods");
        }

        private static string? FindModFolderById(string modsRoot, string targetModId, ManualLogSource log)
        {
            string[] modDirectories = Directory.GetDirectories(modsRoot);
            foreach (string modDirectory in modDirectories)
            {
                string modInfoPath = Path.Combine(modDirectory, "ModInfo.json");
                if (!File.Exists(modInfoPath))
                {
                    continue;
                }

                try
                {
                    string modInfoJson = File.ReadAllText(modInfoPath);
                    ModInfoData modInfo = JsonUtility.FromJson<ModInfoData>(modInfoJson);

                    if (modInfo == null)
                    {
                        continue;
                    }

                    if (string.Equals(modInfo.id, targetModId, StringComparison.OrdinalIgnoreCase))
                    {
                        log.LogInfo($"Found target mod folder: {modDirectory}");
                        return modDirectory;
                    }
                }
                catch (Exception ex)
                {
                    log.LogWarning($"Failed reading ModInfo.json in '{modDirectory}': {ex.Message}");
                }
            }

            return null;
        }

        [Serializable]
        private class ModInfoData
        {
            public string? id;
            public string? name;
            public string? version;
        }
    }
}
