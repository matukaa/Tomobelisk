using HarmonyLib;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Tomobelisk
{
    [HarmonyPatch(typeof(MapManager), "PlayerSelectedNode")]
    public static class PlayerSelectedNodePatch
    {
        public static MethodInfo FollowCoroutineMethod = AccessTools.Method(typeof(MapManager), "FollowCoroutine");

        public static bool Prefix(MapManager __instance, Node _node, ref Coroutine ___followingCo, bool _fromFollowTheLeader = false)
        {
            Debug.LogWarning($"[Tomobelisk] PlayerSelectedNode");
            if((GameManager.Instance.IsMultiplayer() && !NetworkManager.Instance.IsMaster() && AtOManager.Instance.followingTheLeader && !_fromFollowTheLeader))
            {
                return false;
            }
            __instance.selectedNode = false;
            if(!GameManager.Instance.IsMultiplayer())
            {
                __instance.TravelToThisNode(_node);
                return false;
            }
            if(___followingCo != null)
            {
                __instance.StopCoroutine(___followingCo);
            }

            ___followingCo = __instance.StartCoroutine(FollowCoroutineMethod.Invoke(__instance, new object[] { NetworkManager.Instance.GetPlayerNick(), _node.nodeData.NodeId }) as IEnumerator);

            return false;
        }
    }

    [HarmonyPatch(typeof(MapManager), "NET_PlayerSelectedNode")]
    public static class NET_PlayerSelectedNodePatch
    {
        public static MethodInfo TravelToThisNodeMethod = AccessTools.Method(typeof(MapManager), "TravelToThisNodeCo");

        public static bool Prefix(MapManager __instance, string _nick, string _nodeId, ref Dictionary<string, string> ___playerSelectedNodesDict, ref PhotonView ___photonView)
        {
            Debug.LogWarning($"[Tomobelisk] NET_PlayerSelectedNode");
            if (___playerSelectedNodesDict.Remove(_nick))
            {
                Debug.LogWarning($"[Tomobelisk] Removing {_nick} from selected nodes.");
            }

            ___playerSelectedNodesDict.Add(_nick, _nodeId);
            string[] array = new string[___playerSelectedNodesDict.Count];
            ___playerSelectedNodesDict.Keys.CopyTo(array, 0);
            string[] array2 = new string[___playerSelectedNodesDict.Count];
            ___playerSelectedNodesDict.Values.CopyTo(array2, 0);
            string text = JsonHelper.ToJson(array);
            string text2 = JsonHelper.ToJson(array2);
            ___photonView.RPC("NET_SharePlayerSelectedNode", RpcTarget.All, text, text2);
            if(___playerSelectedNodesDict.Count != NetworkManager.Instance.GetNumPlayers())
            {
                return false;
            }
            bool flag = true;
            string text3 = "";
            foreach(KeyValuePair<string, string> item in ___playerSelectedNodesDict)
            {
                if(text3 == "")
                {
                    text3 = item.Value;
                }
                else if(item.Value != text3)
                {
                    flag = false;
                    break;
                }
            }
            if(!flag)
            {
                ___photonView.RPC("NET_DoConflict", RpcTarget.All);
            }
            else
            {
                __instance.StartCoroutine(TravelToThisNodeMethod.Invoke(__instance, new object[] { text3 }) as IEnumerator);
            }

            return false;
        }
    }
}
