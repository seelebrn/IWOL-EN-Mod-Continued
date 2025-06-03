using EngTranslatorMod.Main;
using GUIPackage;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityModularTranslator;
using UnityModularTranslator.Translation;
using YSGame.TuJian;

namespace EngTranslatorMod.Patching
{
    public static partial class Patches
    {
        // A lot of things call this for generating descriptions, label values, and the like. This one gets called A LOT. So try to keep this as performant as possible.
        [HarmonyPatch(typeof(Tools), "Code64ToString")]
        public static class Tools_Code64ToString_Patch
        {
            static void Postfix(ref string __result)
            {
                if (!Translator.TryGetTranslation(__result, out __result))
                {
                    MainScript.AddFailedStringToDict(__result, "Tools_Code64ToString_Patch");
                }
            }
        }

        [HarmonyPatch(typeof(Tools), "Code64")]
        public static class Tools_Code64_Patch
        {
            static void Postfix(ref string __result)
            {
                if (!Translator.TryGetTranslation(__result, out __result))
                {
                    MainScript.AddFailedStringToDict(__result, "Tools_Code64ToString_Patch");
                }
            }
        }
        [HarmonyPatch(typeof(Skill), "skillInit")]
        public static class Tools_skillInit_Patch
        {
            static void Postfix(Skill __instance)
            {
                if (!Translator.TryGetTranslation(__instance.skill_Name, out __instance.skill_Name))
                {
                    MainScript.AddFailedStringToDict(__instance.skill_Name, " Tools_skillInit_Patch");
                }

                if (!Translator.TryGetTranslation(__instance.skill_Desc, out __instance.skill_Desc))
                {
                    MainScript.AddFailedStringToDict(__instance.skill_Desc, " Tools_skillInit_Patch");
                }
            }
        }


        [HarmonyPatch(typeof(Skill), "initStaticSkill")]
        public static class Tools_initStaticSkill_Patch
        {
            static void Postfix(Skill __instance)
            {
                if (!Translator.TryGetTranslation(__instance.skill_Name, out __instance.skill_Name))
                {
                    MainScript.AddFailedStringToDict(__instance.skill_Name, " initStaticSkill");
                }

                if (!Translator.TryGetTranslation(__instance.skill_Desc, out __instance.skill_Desc))
                {
                    MainScript.AddFailedStringToDict(__instance.skill_Desc, " initStaticSkill");
                }
            }
        }

        //Database Info
        //TODO: what the hell is this code.
        [HarmonyPatch(typeof(TuJianDB), nameof(TuJianDB.InitDB))]
        public static class TuJianDB_InitDB_Patch
        {
            static void Postfix()
            {
                List<Dictionary<string, string>> stringDictList = new List<Dictionary<string, string>>()
                {
                    AccessTools.StaticFieldRefAccess<Dictionary<string, string>>(typeof(TuJianDB), "strTextData"),
                    AccessTools.StaticFieldRefAccess<Dictionary<string, string>>(typeof(TuJianDB), "_MapIDNameDict")
                };

                List<Dictionary<int, string>> dictList = new List<Dictionary<int, string>>()
                {
                    AccessTools.StaticFieldRefAccess<Dictionary<int, string>>(typeof(TuJianDB), "_LQWuWeiTypeName"),
                    TuJianDB.YaoShouDescData,
                    TuJianDB.RuleTuJianTypeDescData,
                    TuJianDB.MiShuDesc2Data,
                    TuJianDB.GongFaDesc1Data,
                    TuJianDB.YaoCaoTypeData
                };

                List<Dictionary<string, string>> mergeStringDicts = new List<Dictionary<string, string>>();

                foreach (Dictionary<string, string> dict in stringDictList)
                {
                    Dictionary<string, string> mergeDict = new Dictionary<string, string>();

                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        if (Translator.TryGetTranslation(kvp.Value, out string valueTranslation))
                        {
                            mergeDict[kvp.Key] = valueTranslation;
                        }
                        else
                        {
                            MainScript.AddFailedStringToDict(kvp.Value, " TuJianDB_InitDB_Patch");
                        }
                    }
                    mergeStringDicts.Add(mergeDict);
                }

                List<Dictionary<int, string>> mergeIntDicts = new List<Dictionary<int, string>>();

                foreach (Dictionary<int, string> dict in dictList)
                {
                    Dictionary<int, string> mergeDict = new Dictionary<int, string>();
                    foreach (KeyValuePair<int, string> kvp in dict)
                    {
                        if (Translator.TryGetTranslation(kvp.Value, out string valueTranslation))
                        {
                            mergeDict[kvp.Key] = valueTranslation;
                        }
                        else
                        {
                            MainScript.AddFailedStringToDict(kvp.Value, " TuJianDB_InitDB_Patch");
                        }
                    }
                    mergeIntDicts.Add(mergeDict);
                }

                for (int i = 0; i < mergeStringDicts.Count; i++)
                {
                    Dictionary<string, string> dict = mergeStringDicts[i];
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        stringDictList[i][kvp.Key] = kvp.Value;
                    }
                }
                for (int i = 0; i < mergeIntDicts.Count; i++)
                {
                    Dictionary<int, string> dict = mergeIntDicts[i];
                    foreach (KeyValuePair<int, string> kvp in dict)
                    {
                        dictList[i][kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        //This patch is for Items Trading Interest 
        [HarmonyPatch(typeof(jsonData), "InitLogic")]
        public static class jsonData_InitLogic
        {
            static AccessTools.FieldRef<jsonData, JObject> AllItemLeiXinRef = AccessTools.FieldRefAccess<jsonData, JObject>("AllItemLeiXin");

            static void Postfix(jsonData __instance)
            {
                JObject AllItemLeiXin = AllItemLeiXinRef(__instance);

                foreach (KeyValuePair<string, JToken> kvp in AllItemLeiXin)
                {
                    if (Translator.TryGetTranslation((string)AllItemLeiXin[kvp.Key]["name"], out string keyTranslation))
                    {
                        AllItemLeiXin[kvp.Key]["name"] = keyTranslation;
                    }
                    else
                    {
                        MainScript.AddFailedStringToDict((string)AllItemLeiXin[kvp.Key]["name"], " jsonData_InitLogic");
                    }
                }
            }

        }

        //TODO: check if needed. Implement if yes, delete if no.
        /*
            [HarmonyPatch]
            public static class UIInput_ValidatePatch
            {
                // static AccessTools.FieldRef<UIInput, int> AllItemLeiXinRef =
                // AccessTools.FieldRefAccess<jsonData, JObject>("AllItemLeiXin");
                static IEnumerable<MethodBase> TargetMethods()
                {
                    yield return AccessTools.Method(typeof(UIInput), "Validate", new Type[] { typeof(string )});
                    yield return AccessTools.Method(typeof(UIInput), "Validate", new Type[] {typeof(string), typeof(int), typeof(char)});
                    yield return AccessTools.Method(typeof(UIInput), "Insert");    

                }

                static void Postfix(UIInput __instance)
                {
                    //var AllItemLeiXin = AllItemLeiXinRef(__instance);
                    Console.Write("Initial Character Limit = " + __instance.characterLimit);
                        __instance.characterLimit = 2;             
                }


                }
        */


        //TODO: clean up comments, refactor if needed.
        [HarmonyPatch(typeof(JSONObject), "Str", MethodType.Getter)]
        public static class JSONObject_Patch
        {
            static AccessTools.FieldRef<JSONObject, string> strRef = AccessTools.FieldRefAccess<JSONObject, string>("str");
            static void Postfix(JSONObject __instance, ref string __result)
            {
                if (Translator.TryGetTranslation(__result, out string translation))
                {
                    //     UMTLogger.Log($"Found matching string!: {MainScript.translationDict[__result]}");
                    //        __result = MainScript.translationDict[__result];
                    __instance.str = translation;
                    //     UMTLogger.Log($"Updated String: {__result}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(__result, " USelectNum_Show_Patch");
                    string FailedRegistry = Path.Combine(BepInEx.Paths.PluginPath, "MissedStrings.txt");
                    //Logging only TextAssets in .\BepinEx\Plugin\MissedStrings.txt

                    if (Helpers.IsChinese(__result))
                    {
                        IDictionary<string, string> map = new Dictionary<string, string>()
                            {
                            {"\n","\\n"},
                            {"\r","\\r"},
                            };

                        //Regexing to put it in the correct TA format

                        using (StreamWriter sw = File.AppendText(FailedRegistry))
                        {
                            if (!FailedRegistry.Contains(__result))
                            {

                                __result = Regex.Replace(__result, "\n", "\\n");
                                __result = Regex.Replace(__result, "\r", "\\r");

                                sw.WriteLine(__result);
                            }
                        }

                    }
                }
            }
            static void Postfix(ref string __result)
            {
                if (!Translator.TryGetTranslation(__result, out __result))
                {
                    //MainScript.AddFailedStringToDict(__result, " USelectNum_Show_Patch");
                }
            }
        }

        [HarmonyPatch(typeof(UILabel), "text", MethodType.Setter)]
        public static class UILabel_TextSetter_Patch
        {
            static void Postfix(UILabel __instance, ref string value)
            {
                if (Translator.TryGetTranslation(value, out string translation))
                {
                    UMTLogger.Log($"UILabel_TextSetter_Patch: Found matching string!: {value}:{translation}, uilabel");
                    __instance.text = translation;
                }
                else
                {
                    MainScript.AddFailedStringToDict(value, " UILabel_TextSetter_Patch");
                }
            }
        }

        [HarmonyPatch(typeof(UILabel), "OnInit")]
        public static class UILabel_OnEnable_Patch
        {
            static void Postfix(UILabel __instance)
            {
                if (Translator.TryGetTranslation(__instance.text, out string translation))
                {
                    UMTLogger.Log($"UILabel_OnInit_Patch: Found matching string!: {__instance.text}:{translation}, uilabel");
                    __instance.text = translation;
                }
                else
                {
                    MainScript.AddFailedStringToDict(__instance.text, " UILabel_OnEnable_Patch");
                }
            }
        }

        [HarmonyPatch(typeof(Text), "text", MethodType.Setter)]
        public static class Text_TextSetter_Patch
        {
            static void Postfix(Text __instance, ref string value)
            {
                if (Translator.TryGetTranslation(value, out string translation))
                {
                    UMTLogger.Log($"Text_TextSetter_Patch: Found matching string!: {value}:{translation}, text");
                    __instance.text = translation;
                }
                else
                {
                    MainScript.AddFailedStringToDict(value, " Text_TextSetter_Patch");
                }
            }
        }

        [HarmonyPatch(typeof(Text), "OnEnable")]
        public static class Text_OnEnable_Patch
        {
            static void Postfix(Text __instance)
            {
                if (Translator.TryGetTranslation(__instance.text, out string translation))
                {
                    UMTLogger.Log($"Text_OnEnable_Patch: Found matching string!: {__instance.text}:{translation}, uilabel");
                    __instance.text = translation;
                }
                else
                {
                    MainScript.AddFailedStringToDict(__instance.text, " Text_OnEnable_Patch");
                }
            }
        }
    }
}