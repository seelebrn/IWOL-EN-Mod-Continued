using EngTranslatorMod.Main;
using HarmonyLib;
using System.IO;
using UnityModularTranslator;
using XUnity.AutoTranslator.Plugin.BepInEx;

namespace EngTranslatorMod.Patching
{
    public static partial class Patches
    {
        [HarmonyPatch(typeof(AutoTranslatorPlugin), "ConfigPath", MethodType.Getter)]
        public static class XUnityTweaks1
        {
            static void Postfix(AutoTranslatorPlugin __instance, ref string __result)
            {
                __result = MainScript.configDir;
            }
        }

        [HarmonyPatch(typeof(AutoTranslatorPlugin), "TranslationPath", MethodType.Getter)]
        public static class XUnityTweaks2
        {
            static void Postfix(AutoTranslatorPlugin __instance, ref string __result)
            {
                __result = Directory.GetParent(MainScript.sourceDir).ToString();
            }
        }
    }
}