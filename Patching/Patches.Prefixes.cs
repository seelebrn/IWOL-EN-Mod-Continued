using EngTranslatorMod.Main;
using Fungus;
using HarmonyLib;
using UnityModularTranslator.Translation;

namespace EngTranslatorMod.Patching
{
    public static partial class Patches
    {
        [HarmonyPatch(typeof(ChuanYingManager), "ReadData")]
        public static class ChuanYingManager_ReadData_Patch
        {
            static void Prefix(ref JSONObject obj)
            {
                if (Translator.TryGetTranslation(obj["info"].str, out string translation))
                {
                    obj["info"].str = translation;
                }
                else
                {
                    MainScript.AddFailedStringToDict(obj["info"].str, "ChuanYingManager_ReadData_Patch");
                }
            }
        }

        //I think this is the messages you get from NPCs
        [HarmonyPatch(typeof(CyEmail), "GetContent")]
        public static class CyEmailr_GetContent
        {
            static void Prefix(ref string msg, EmailData emailData)
            {
                if (!Translator.TryGetTranslation(msg, out msg))
                {
                    MainScript.AddFailedStringToDict(msg, "CyEmailr_GetContent");
                }
            }
        }


        //Fungus dialog box
        [HarmonyPatch(typeof(Say), "OnEnter")]
        public static class Say_OnEnter_Patch_01
        {
            static AccessTools.FieldRef<Say, string> storyTextRef = AccessTools.FieldRefAccess<Say, string>("storyText");

            static void Prefix(Say __instance)
            {
                string storyText = storyTextRef(__instance);
                if (Translator.TryGetTranslation(Helpers.CustomEscape(storyText), out string translatedText))
                {
                    storyTextRef(__instance) = Helpers.CustomUnescape(translatedText);
                }
                else
                {
                    MainScript.AddFailedStringToDict(Helpers.CustomEscape(storyText), "Say_OnEnter_Patch");
                }
            }
        }

        //Fungus dialog box
        [HarmonyPatch(typeof(Flowchart), "SubstituteVariables")]
        public static class Flowchart_SubstituteVariables_Patch
        {
            static void Prefix(ref string input)
            {
                if (!Translator.TryGetTranslation(input, out input))
                {
                    MainScript.AddFailedStringToDict(input, "Flowchart_SubstituteVariables_Patch");
                }
            }
        }

    }
    // This is for active skills since they often get their values dynamically generated
    [HarmonyPatch(typeof(Tools), "getDesc")]
    static class Tools_getDesc_Patch
    {
        static void Prefix(ref string desstr)
        {
            if (!Translator.TryGetTranslation(desstr, out desstr))
            {
                MainScript.AddFailedStringToDict(desstr, "Tools_getDesc_Patch");
            }
        }
    }

    //I think this is related to stores
    [HarmonyPatch(typeof(USelectNum), "Show")]
    static class USelectNum_Show_Patch
    {
        static void Prefix(ref string desc)
        {
            if (Translator.TryGetTranslation(desc, out desc))
            {
                MainScript.AddFailedStringToDict(desc, " USelectNum_Show_Patch");
            }
        }
    }
}
