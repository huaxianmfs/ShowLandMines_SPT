using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShowLandMines
{
    [BepInPlugin("me.test.plugin.ShowLandMines", "ShowLandMines", "1.0.0")]
    public class ShowLandMines : BasePlugin
    {
        private static bool _minefieldsVisible;
        private static bool _sniperZonesVisible;
        private static bool _disableZones;

        // 保存被我们禁用的对象，方便恢复
        private static readonly List<GameObject> _disabledZoneObjects = new List<GameObject>();

        public override void Load()
        {
            UnityEngine.Debug.Log("BepInEx : ShowLandMines Loaded!");

            Harmony harmony = new Harmony("me.test.plugin.ShowLandMines");

            // 只保留按键 patch，其他 patch 对 IL2CPP 原生逻辑无效，删除
            TryPatch(harmony, typeof(SimpleCharacterController), "Move",
                nameof(SimpleCharacterController_Move_Patch), null);
        }

        private static void TryPatch(Harmony harmony, Type type, string methodName,
                                     string prefixName, string postfixName)
        {
            try
            {
                var method = AccessTools.Method(type, methodName);
                if (method == null)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[ShowLandMines] Method not found: {type.Name}.{methodName}");
                    return;
                }

                var prefix = prefixName != null
                    ? AccessTools.Method(typeof(ShowLandMines), prefixName)
                    : null;

                var postfix = postfixName != null
                    ? AccessTools.Method(typeof(ShowLandMines), postfixName)
                    : null;

                harmony.Patch(
                    method,
                    prefix != null ? new HarmonyMethod(prefix) : null,
                    postfix != null ? new HarmonyMethod(postfix) : null);

                UnityEngine.Debug.Log($"[ShowLandMines] Patched OK: {type.Name}.{methodName}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(
                    $"[ShowLandMines] Failed to patch {type.Name}.{methodName}: {ex}");
            }
        }

        // ---------------- 按键监听 ----------------

        public static void SimpleCharacterController_Move_Patch(SimpleCharacterController __instance)
        {
            if (!IsLocalPlayer(__instance))
                return;

            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrlPressed)
                return;

            // Ctrl + 小键盘7：雷区可视化
            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                ToggleMinefields();
            }
            // Ctrl + 小键盘8：狙击区可视化
            else if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                ToggleSniperZones();
            }
            // Ctrl + 小键盘9：雷区 + 狙击区失效
            else if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                _disableZones = !_disableZones;
                UnityEngine.Debug.Log("[ShowLandMines] Zones disabled = " + _disableZones);

                if (_disableZones)
                    DisableAllZones();
                else
                    RestoreAllZones();
            }
        }

        // ---------------- 失效/恢复：直接操作场景对象 ----------------

        private static void DisableAllZones()
        {
            _disabledZoneObjects.Clear();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                var roots = scene.GetRootGameObjects();
                for (int j = 0; j < roots.Length; j++)
                {
                    DisableZonesRecursive(roots[j].transform);
                }
            }

            UnityEngine.Debug.Log($"[ShowLandMines] Disabled {_disabledZoneObjects.Count} zone objects.");
        }

        private static void DisableZonesRecursive(Transform t)
        {
            // 处理 Minefield
            var minefield = t.GetComponent<Minefield>();
            if (minefield != null && minefield.enabled)
            {
                minefield.enabled = false;
                _disabledZoneObjects.Add(minefield.gameObject);
            }

            // 处理 MineDirectionalColliders（某些雷区是这种）
            var mineDir = t.GetComponent<MineDirectionalColliders>();
            if (mineDir != null && mineDir.enabled)
            {
                mineDir.enabled = false;
                _disabledZoneObjects.Add(mineDir.gameObject);
            }

            // 处理 SniperFiringZone
            var sniper = t.GetComponent<SniperFiringZone>();
            if (sniper != null && sniper.enabled)
            {
                sniper.enabled = false;
                _disabledZoneObjects.Add(sniper.gameObject);
            }

            // 同时禁用这个对象自身的 Collider（防止触发 OnTriggerEnter）
            var col = t.GetComponent<Collider>();
            if (col != null && col.enabled &&
                (minefield != null || sniper != null || mineDir != null))
            {
                col.enabled = false;
            }

            for (int i = 0; i < t.childCount; i++)
            {
                DisableZonesRecursive(t.GetChild(i));
            }
        }

        private static void RestoreAllZones()
        {
            int restored = 0;
            foreach (var go in _disabledZoneObjects)
            {
                if (go == null) continue;

                var minefield = go.GetComponent<Minefield>();
                if (minefield != null) minefield.enabled = true;

                var sniper = go.GetComponent<SniperFiringZone>();
                if (sniper != null) sniper.enabled = true;

                var mineDir = go.GetComponent<MineDirectionalColliders>();
                if (mineDir != null) mineDir.enabled = true;

                var col = go.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                restored++;
            }
            _disabledZoneObjects.Clear();
            UnityEngine.Debug.Log($"[ShowLandMines] Restored {restored} zone objects.");
        }

        // ---------------- 本地玩家判断 ----------------

        private static bool IsLocalPlayer(SimpleCharacterController controller)
        {
            var player = controller.GetComponentInParent<Player>();
            if (player == null)
                return false;

            return player.IsYourPlayer;
        }

        // ---------------- 可视化逻辑（保持不变） ----------------

        private static void ToggleMinefields()
        {
            _minefieldsVisible = !_minefieldsVisible;
            ClearObjects(Creator.MinefieldObjectName);
            if (_minefieldsVisible)
                SearchAllSceneForMinefields();
        }

        private static void ToggleSniperZones()
        {
            _sniperZonesVisible = !_sniperZonesVisible;
            ClearObjects(Creator.SniperZoneObjectName);
            if (_sniperZonesVisible)
                SearchAllSceneForSniperZones();
        }

        private static void SearchAllSceneForMinefields()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                var roots = scene.GetRootGameObjects();
                for (int j = 0; j < roots.Length; j++)
                    FindAndAddLandmine(roots[j].transform);
            }
        }

        private static void SearchAllSceneForSniperZones()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                var roots = scene.GetRootGameObjects();
                for (int j = 0; j < roots.Length; j++)
                    FindAndAddSniper(roots[j].transform);
            }
        }

        private static void FindAndAddLandmine(Transform child)
        {
            if (child.name == Creator.MinefieldObjectName)
                return;

            BoxCollider box = child.GetComponent<BoxCollider>();
            Minefield minefield = child.GetComponent<Minefield>();
            MineDirectionalColliders mineDirectional = child.GetComponent<MineDirectionalColliders>();

            if (box != null && (minefield != null || mineDirectional != null))
                Creator.CreateBox(child, box.size, box.center, 0, Creator.MinefieldObjectName);

            for (int i = 0; i < child.childCount; i++)
                FindAndAddLandmine(child.GetChild(i));
        }

        private static void FindAndAddSniper(Transform child)
        {
            if (child.name == Creator.SniperZoneObjectName)
                return;

            BoxCollider box = child.GetComponent<BoxCollider>();
            SniperFiringZone sniperZone = child.GetComponent<SniperFiringZone>();

            if (box != null && sniperZone != null)
                Creator.CreateBox(child, box.size, box.center, 1, Creator.SniperZoneObjectName);

            for (int i = 0; i < child.childCount; i++)
                FindAndAddSniper(child.GetChild(i));
        }

        private static void ClearObjects(string objectName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                var roots = scene.GetRootGameObjects();
                for (int j = 0; j < roots.Length; j++)
                    ClearObjectsRecursive(roots[j].transform, objectName);
            }
        }

        private static void ClearObjectsRecursive(Transform transform, string objectName)
        {
            if (transform.name == objectName)
            {
                UnityEngine.Object.Destroy(transform.gameObject);
                return;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
                ClearObjectsRecursive(transform.GetChild(i), objectName);
        }
    }
}