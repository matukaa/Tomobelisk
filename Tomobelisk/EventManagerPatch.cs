using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tomobelisk
{
    [HarmonyPatch(typeof(EventManager), "SelectOption")]
    public static class SelectOptionPatch
    {
        public static MethodInfo SelectOptionCoMethod = AccessTools.Method(typeof(EventManager), "SelectOptionCo");

        public static bool Prefix(int _index, EventManager __instance, ref Reply[] ___replys, ref PhotonView ___photonView)
        {
            UnityEngine.Debug.LogWarning($"[Tomobelisk] SelectOption {_index}");
            __instance.optionSelected = _index;
            if(GameManager.Instance.IsMultiplayer())
            {
                ___photonView.RPC("NET_Event_OptionSelected", RpcTarget.All, NetworkManager.Instance.GetPlayerNick(), _index);
            }
            else
            {
                __instance.StartCoroutine(SelectOptionCoMethod.Invoke(__instance, null) as IEnumerator);
            }

            return false;
        }
    }

    public static class ReplyExtensions
    {
        public static FieldInfo SelectedField = AccessTools.Field(typeof(Reply), "selected");

        public static FieldInfo ReplyChar0Spr = AccessTools.Field(typeof(Reply), "replyChar0Spr");
        public static FieldInfo ReplyChar1Spr = AccessTools.Field(typeof(Reply), "replyChar1Spr");
        public static FieldInfo ReplyChar2Spr = AccessTools.Field(typeof(Reply), "replyChar2Spr");
        public static FieldInfo ReplyChar3Spr = AccessTools.Field(typeof(Reply), "replyChar3Spr");

        public static FieldInfo ColorText = AccessTools.Field(typeof(Reply), "colorText");
        public static FieldInfo ColorOff = AccessTools.Field(typeof(Reply), "colorOff");

        public static void Deselect(this Reply reply, string playerNick)
        {
            SelectedField.SetValue(reply, false);

            reply.replyText.color = (Color)ColorText.GetValue(reply);
            reply.replyBgButton.color = (Color)ColorOff.GetValue(reply);

            Color clr = Functions.HexToColor(NetworkManager.Instance.GetColorFromNick(playerNick));
            SpriteRenderer spr = null;
            if(reply.replyChar3.gameObject.activeSelf)
            {
                spr = ReplyChar3Spr.GetValue(reply) as SpriteRenderer;
                if (spr != null && spr.color == clr)
                {
                    reply.replyChar3.gameObject.SetActive(false);
                    return;
                }
            }
            if(reply.replyChar2.gameObject.activeSelf)
            {
                spr = ReplyChar2Spr.GetValue(reply) as SpriteRenderer;
                if(spr != null && spr.color == clr)
                {
                    reply.replyChar2.gameObject.SetActive(false);
                    return;
                }
            }
            if(reply.replyChar1.gameObject.activeSelf)
            {
                spr = ReplyChar1Spr.GetValue(reply) as SpriteRenderer;
                if(spr != null && spr.color == clr)
                {
                    reply.replyChar1.gameObject.SetActive(false);
                    return;
                }
            }
            if(reply.replyChar0.gameObject.activeSelf)
            {
                spr = ReplyChar0Spr.GetValue(reply) as SpriteRenderer;
                if(spr != null && spr.color == clr)
                {
                    reply.replyChar0.gameObject.SetActive(false);
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(EventManager), "SelectOptionMP")]
    public static class SelectOptionMPPatch
    {
        public static MethodInfo SelectMultiplayerAnswerMethod = AccessTools.Method(typeof(EventManager), "SelectMultiplayerAnswer");
        

        public static bool Prefix(string _playerNick, int _option, EventManager __instance, ref Reply[] ___replys, ref PhotonView ___photonView)
        {
            UnityEngine.Debug.LogWarning($"[Tomobelisk] SelectOptionMP {_option}");
            __instance.MultiplayerPlayerSelection.Remove(_playerNick);
            __instance.MultiplayerPlayerSelection.Add(_playerNick, _option);
            for(int i = 0; i < ___replys.Length; i++)
            {
                if(___replys[i] != null && ___replys[i].GetOptionIndex() == _option)
                {
                    ___replys[i].SelectedByMultiplayer(_playerNick);
                }
                if(___replys[i] != null && ___replys[i].GetOptionIndex() != _option)
                {
                    ___replys[i].Deselect(_playerNick);
                }
            }
            if(NetworkManager.Instance.IsMaster() && __instance.MultiplayerPlayerSelection.Count == NetworkManager.Instance.GetNumPlayers())
            {
                SelectMultiplayerAnswerMethod.Invoke(__instance, null);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Reply), "OnMouseUp")]
    public static class ReplyMouseUpPatch
    {
        public static bool Prefix(Reply __instance, ref bool ___blocked)
        {
            if(!AlertManager.Instance.IsActive() && !GameManager.Instance.IsTutorialActive() && !SettingsManager.Instance.IsActive() && !DamageMeterManager.Instance.IsActive() && (!MapManager.Instance.characterWindow.gameObject.activeSelf || !MapManager.Instance.characterWindow.IsActive()) && !___blocked && (!GameManager.Instance.IsMultiplayer() || NetworkManager.Instance.IsMaster() || !AtOManager.Instance.followingTheLeader) && Functions.ClickedThisTransform(__instance.transform))
            {
                __instance.SelectThisOption();
            }

            return false;
        }
    }
}
