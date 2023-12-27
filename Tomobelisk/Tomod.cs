using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomobelisk
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Tomod : BaseUnityPlugin
    {
        public const string pluginGuid = "tomo.aot.tomobelisk";
        public const string pluginName = "Tomobelisk";
        public const string pluginVersion = "1.1.0.0";

        public void Awake()
        {
            UnityEngine.Debug.LogWarning($"[Tomobelisk] Loading plugin... Version {pluginVersion}");

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }
    }
}
