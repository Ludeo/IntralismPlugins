using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ChangeSongSpeed
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static ManualLogSource logger;
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        private static double speed = 1;
        private static string originalSubmitUrl;
        private static Text speedText;
        private static GameObject speedSelectorCanvas;

        private void Awake()
        {
            logger = this.Logger;
            this.harmony.PatchAll(typeof(Plugin));
            logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RanksSystem), "Init")]
        public static void RanksSystemStart(RanksSystem __instance)
        {
            logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID}: Rank system started. Saving original submit url");
            originalSubmitUrl = __instance.submitScoreUrl;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameScene), "OnStartRound")]
        public static void MapStart(GameScene __instance)
        {
            logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID}: Map started. Setting speed to " + speed);
            __instance.asampler.audioSources[1].pitch = (float)speed;
            Time.timeScale = (float)speed;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RanksSystem), "SubmitScore")]
        public static void SubmitScore(RanksSystem __instance)
        {
            if (speed is 1.0 or 1)
            {
                __instance.submitScoreUrl = originalSubmitUrl;
            }
            else
            {
                logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID}: Score submit disabled because rate is applied");
                __instance.submitScoreUrl = "";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapsListSelector), "Start")]
        public static void MapSelectorStart(MapsListSelector __instance)
        {
            logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID}: Map selector started");
            GameObject mapSelector = __instance.mapSelector;

            if (!speedSelectorCanvas)
            {
                speedSelectorCanvas = new GameObject
                {
                    transform = {
                        parent = mapSelector.transform,
                        localPosition = new Vector3(-870, -515, 0),
                    },
                    name = "Speed Selection",
                    layer = mapSelector.layer,
                };

                GameObject speedTextObject = new()
                {
                    transform =
                    {
                        parent = speedSelectorCanvas.transform,
                        localPosition = new Vector3(0, 0, 0),
                    },
                    name = "Speed Selection Text",
                    layer = speedSelectorCanvas.layer,
                };

                speedText = speedTextObject.AddComponent<Text>();
                speedText.text = "Speed: " + speed;
                speedText.rectTransform.sizeDelta = new Vector2(150, 25);
                speedText.fontSize = 20;
                speedText.font = Font.GetDefault();
                speedText.color = Color.black;

                GameObject raiseSpeedObject = new()
                {
                    transform =
                    {
                        parent = speedSelectorCanvas.transform,
                        localPosition = new Vector3(60, 0, 0),
                    },
                    name = "Speed Selection Raise",
                    layer = speedSelectorCanvas.layer,
                };

                Button raiseSpeedButton = raiseSpeedObject.AddComponent<Button>();
                raiseSpeedButton.name = "+";
                raiseSpeedButton.onClick.AddListener(RaiseSpeedButtonClicked);

                Text raiseSpeedButtonText = raiseSpeedObject.AddComponent<Text>();
                raiseSpeedButtonText.rectTransform.sizeDelta = new Vector2(25, 25);
                raiseSpeedButtonText.text = "+";
                raiseSpeedButtonText.fontSize = 20;
                raiseSpeedButtonText.font = Font.GetDefault();
                raiseSpeedButtonText.color = Color.black;

                GameObject lowerSpeedObject = new()
                {
                    transform =
                    {
                        parent = speedSelectorCanvas.transform,
                        localPosition = new Vector3(85, 0, 0),
                    },
                    name = "Speed Selection Raise",
                    layer = speedSelectorCanvas.layer,
                };

                Button lowerSpeedButton = lowerSpeedObject.AddComponent<Button>();
                lowerSpeedButton.name = "-";
                lowerSpeedButton.onClick.AddListener(LowerSpeedButtonClicked);

                Text lowerSpeedButtonText = lowerSpeedObject.AddComponent<Text>();
                lowerSpeedButtonText.rectTransform.sizeDelta = new Vector2(25, 25);
                lowerSpeedButtonText.text = "-";
                lowerSpeedButtonText.fontSize = 20;
                lowerSpeedButtonText.font = Font.GetDefault();
                lowerSpeedButtonText.color = Color.black;
            }
        }

        private static void RaiseSpeedButtonClicked()
        {
            if (speed < 1.5)
            {
                speed = Math.Round((speed + 0.05) * 100) / 100;
                speedText.text = "Speed: " + speed;
            }
        }

        private static void LowerSpeedButtonClicked()
        {
            if (speed > 0.5)
            {
                speed = Math.Round((speed - 0.05) * 100) / 100;
                speedText.text = "Speed: " + speed;
            }
        }
    }
}