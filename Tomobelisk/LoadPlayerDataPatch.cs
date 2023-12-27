using HarmonyLib;
using System.Reflection;

namespace Tomobelisk
{
    [HarmonyPatch(typeof(GameManager), "LoadPlayerData")]
    public static class LoadPlayerDataPatch
    {
        static readonly MethodInfo BeginGameMethod = AccessTools.Method(typeof(GameManager), "BeginGame");

        public static bool Prefix(GameManager __instance)
        {
            PlayerData playerData = SaveManager.LoadPlayerData();
            if(playerData == null)
            {
                playerData = SaveManager.LoadPlayerData(fromBackup: true);
            }
            if(playerData != null)
            {
                SaveManager.RestorePlayerData(playerData);
                if(PlayerManager.Instance.UnlockedCards.Count != 33 && PlayerManager.Instance.UnlockedNodes.Count != 0 && !AtOManager.Instance.IsFirstGame())
                {
                    SaveManager.SavePlayerDataBackup();
                }
            }
            BeginGameMethod.Invoke(__instance, null);
            return false;
        }
    }
}
