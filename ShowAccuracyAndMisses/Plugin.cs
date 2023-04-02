using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ShowAccuracyAndMisses
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static ManualLogSource logger;
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        private static GameObject missTextObject;
        private static Text missText;
        private static GameObject accuracyTextObject;
        private static Text accuracyText;

        private void Awake()
        {
            logger = this.Logger;
            this.harmony.PatchAll(typeof(Plugin));
            logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameScene), "Start")]
        public static void GameSceneStart(GameScene __instance)
        {
            logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} game scene started");
            GameObject ingameUiCanvas = __instance.ingameUICanvas;

            if (!missTextObject)
            {
                missTextObject = new GameObject
                {
                    transform = {
                        parent = ingameUiCanvas.transform,
                        localPosition = new Vector3(40, -20, 0),
                    },
                    layer = ingameUiCanvas.layer,
                    name = "Miss Text Middle",
                };

                missText = missTextObject.AddComponent<Text>();
                missText.rectTransform.sizeDelta = new Vector2(150, 18);
                missText.text = "Misses: " + (__instance.pbase.fullLevelData.mapData.maxLives - __instance.pbase.lives);
                missText.fontSize = 16;
                missText.font = Font.GetDefault();
            }

            missTextObject.SetActive(__instance.gameMode != GameScene.GameMode.Hardcore);

            if (!accuracyTextObject)
            {
                accuracyTextObject = new GameObject
                {
                    transform = {
                        parent = ingameUiCanvas.transform,
                        localPosition = new Vector3(60, 10, 0),
                    },
                    layer = ingameUiCanvas.layer,
                    name = "Accuracy Text Middle",
                };

                accuracyText = accuracyTextObject.AddComponent<Text>();
                accuracyText.rectTransform.sizeDelta = new Vector2(200, 30);
                accuracyText.text = "" + (Math.Floor(__instance.pbase.accuracyScore * 10000.0) / 10000.0 * 100.0).ToString("0.00") + "%";
                accuracyText.fontSize = 26;
                accuracyText.font = Font.GetDefault();
            }

            accuracyTextObject.SetActive(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameScene), "Update")]
        public static void GameSceneUpdate(GameScene __instance)
        {
            missText.text = "Misses: " + (__instance.pbase.fullLevelData.mapData.maxLives - __instance.pbase.lives);
            accuracyText.text = "" + (Math.Floor(__instance.pbase.accuracyScore * 10000.0) / 10000.0 * 100.0).ToString("0.00") + "%";
        }
    }
}