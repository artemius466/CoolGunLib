using BepInEx;
using Photon.Pun;

using IisCoolGun.Important.Patching; // Ensure your patching scripts are in this namespace
using System;
using UnityEngine;
using Utilla;

namespace IisCoolGun
{
    //[BepInDependency("org.iidk.gorillatag.iimenu", "5.0.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public void OnEnable()
        {
            TogglePatches(true);
        }

        public void OnDisable()
        {
            TogglePatches(false);
        }

        private void TogglePatches(bool enable)
        {
            if (enable)
            {
                HarmonyPatches.ApplyHarmonyPatches();
            }
            else
            {
                HarmonyPatches.RemoveHarmonyPatches();
            }
        }
    }

    public class PluginInfo
    {
        internal const string
            GUID = "Artemius466.CoolGunLib",
            Name = "Cool and smooth gun lib for ii's stupid menu",
            Version = "1.0.0";
    }
}
