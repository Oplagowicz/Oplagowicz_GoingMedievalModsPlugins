# Oplagowicz_GoingMedievalModsPlugins

# Mayor Mod

Mayor Mod is an experimental custom role mod for Going Medieval.

The goal of this project is to add a new Mayor role with its own progression, room requirements, effectors, and custom runtime behavior. The mod is built as a hybrid setup:
- a regular game mod with JSON data
- a BepInEx plugin with Harmony patches for runtime logic

## Current status

Work in progress.

The data side of the mod is implemented:
- custom `mayor` role
- role levels
- room requirements
- custom effectors
- role-related room support

The runtime behavior side is still experimental.  
The project currently explores custom GOAP role integration and runtime patching for settler-based role logic.

## Features

- New custom role: **Mayor**
- Role progression with multiple levels
- Dedicated room requirement: **bedroom_mayors**
- Custom role effectors and modifiers
- BepInEx plugin for runtime role logic experiments
- Harmony patches for GOAP integration

## Tech stack

- C#
- Unity modding
- BepInEx
- Harmony / HarmonyX
- JSON data modding

## Installation

This project currently uses two parts:

### 1. Mod data
Install the JSON mod into:

`Documents/Foxy Voxel/Going Medieval/Mods/Mayor Role Mod`

### 2. BepInEx plugin
Install the plugin DLL into:

`Going Medieval/BepInEx/plugins/`

## Repository structure

- `Mayor Role Mod` - JSON mod data
- `OPlag.GoingMedieval.MayorRolePlugin` - BepInEx plugin source code

## Notes

This is an experimental modding project focused on extending role behavior beyond the default game flow.  
Because Going Medieval does not fully expose all role systems for external extension, some parts of the custom runtime role logic may be unstable or incomplete.

## Credits

Created by Artur Demichev / Oplagowicz.
