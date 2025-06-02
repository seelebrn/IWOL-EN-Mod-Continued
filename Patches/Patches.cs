using Bag;
using CaiJi;
using EngTranslatorMod.Main;
using Fungus;
using GUIPackage;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using script.NewLianDan.LianDan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Tab;
using UnityEngine;
using UnityEngine.Events;
using XUnity.AutoTranslator.Plugin.BepInEx;
using YSGame.EquipRandom;
using YSGame.TuJian;
using UnityModularTranslator.Translation;
using UnityModularTranslator;

namespace EngTranslatorMod.Patches
{
    public static class Patches
    {
        [HarmonyPatch(typeof(AutoTranslatorPlugin), "ConfigPath", MethodType.Getter)]
        static class XUnityTweaks1
        {
            static void Postfix(AutoTranslatorPlugin __instance, ref string __result)
            {
                __result = MainScript.configDir;
                UMTLogger.Log(__result);
            }
        }

        [HarmonyPatch(typeof(AutoTranslatorPlugin), "TranslationPath", MethodType.Getter)]
        static class XUnityTweaks2
        {
            static void Postfix(AutoTranslatorPlugin __instance, ref string __result)
            {
                __result = Directory.GetParent(MainScript.sourceDir).ToString();
                UMTLogger.Log(__result);
            }
        }

        [HarmonyPatch(typeof(ChuanYingManager), "ReadData")]
        static class ChuanYingManager_ReadData_Patch
        {
            static void Prefix(ref JSONObject obj)
            {
                if (Translator.TryGetTranslation(obj["info"].str, out string translation))
                {
                    UMTLogger.Log($"Found matching string!: {translation}");
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
        static class CyEmailr_GetContent
        {
            static void Prefix(ref string msg, EmailData emailData)
            {
                if (Translator.TryGetTranslation(msg, out string translation))
                {
                    UMTLogger.Log($"Found matching string!: {translation}");
                    msg = translation;
                }
                else
                {
                    MainScript.AddFailedStringToDict(msg, "CyEmailr_GetContent");
                }
            }
        }


        //Fungus dialog box
        [HarmonyPatch(typeof(Say), "OnEnter")]
        static class Say_OnEnter_Patch_01
        {
            static AccessTools.FieldRef<Say, string> storyTextRef =
                AccessTools.FieldRefAccess<Say, string>("storyText");

            static void Prefix(Say __instance)
            {
                string storyText = storyTextRef(__instance);
                if (Translator.TryGetTranslation(Helpers.CustomEscape(storyText), out string translatedText))
                {
                    UMTLogger.Log($"Found matching string!: {translatedText}");
                    storyTextRef(__instance) = Helpers.CustomUnescape(translatedText);
                    UMTLogger.Log($"Updated String: {storyTextRef(__instance)}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(Helpers.CustomEscape(storyText), "Say_OnEnter_Patch");
                }
            }
        }

        //Fungus dialog box
        [HarmonyPatch(typeof(Flowchart), "SubstituteVariables")]
        static class Flowchart_SubstituteVariables_Patch
        {
            static void Prefix(ref string input)
            {
                if (Translator.TryGetTranslation(input, out string translation))
                {
                    UMTLogger.Log($"Found matching string!: {translation}");
                    input = translation;
                    UMTLogger.Log($"Updated String: {input}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(input, "Flowchart_SubstituteVariables_Patch");
                }
            }
        }

        // A lot of things call this for generating descriptions, label values, and the like. This one gets called A LOT. So try to keep this as performant as possible.
        [HarmonyPatch(typeof(Tools), "Code64ToString")]
        static class Tools_Code64ToString_Patch
        {

            static void Postfix(ref string __result)
            {
                try
                {
                    if (Translator.TryGetTranslation(__result, out string translation))
                    {
                        UMTLogger.Log($"Found matching string!: {translation}");
                        __result = translation;
                    }
                    else
                    {
                        MainScript.AddFailedStringToDict(__result, "Tools_Code64ToString_Patch");
                    }
                }
                catch (Exception e)
                {
                    UMTLogger.Log(e.ToString());
                }
            }
        }

        [HarmonyPatch(typeof(Tools), "Code64")]
        static class Tools_Code64_Patch
        {
            static void Postfix(ref string __result)
            {
                if (Translator.TryGetTranslation(__result, out string translation))
                {
                    UMTLogger.Log($"Found matching string!: {translation}");
                    __result = translation;
                }
                else
                {
                    MainScript.AddFailedStringToDict(__result, "Tools_Code64ToString_Patch");
                }
            }
        }

        // This is for active skills since they often get their values dynamically generated
        [HarmonyPatch(typeof(Tools), "getDesc")]
        static class Tools_getDesc_Patch
        {
            static void Prefix(ref string desstr)
            {
                if (Translator.TryGetTranslation(desstr, out string translation))
                {
                    UMTLogger.Log($"Found matching string!: {translation}");
                    desstr = translation;
                    UMTLogger.Log($"Updated String: {desstr}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(desstr, "Tools_getDesc_Patch");
                }
            }
        }


        [HarmonyPatch(typeof(Skill), "skillInit")]
        static class Tools_skillInit_Patch
        {
            static void Postfix(Skill __instance)
            {
                if (Translator.TryGetTranslation(__instance.skill_Name, out string skillNameTranslation))
                {
                    UMTLogger.Log($"Found matching string!: {skillNameTranslation}");
                    __instance.skill_Name = skillNameTranslation;
                    UMTLogger.Log($"Updated String: {__instance.skill_Name}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(__instance.skill_Name, " Tools_skillInit_Patch");
                }

                if (Translator.TryGetTranslation(__instance.skill_Desc, out string skillDescTranslation))
                {
                    UMTLogger.Log($"Found matching string!: {skillDescTranslation}");
                    __instance.skill_Desc = skillDescTranslation;
                    UMTLogger.Log($"Updated String: {__instance.skill_Desc}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(__instance.skill_Desc, " Tools_skillInit_Patch");
                }
            }
        }


        [HarmonyPatch(typeof(Skill), "initStaticSkill")]
        static class Tools_initStaticSkill_Patch
        {
            static void Postfix(Skill __instance)
            {
                if (Translator.TryGetTranslation(__instance.skill_Name, out string skillNameTranslation))
                {
                    UMTLogger.Log($"Found matching string!: {skillNameTranslation}");
                    __instance.skill_Name = skillNameTranslation;
                    UMTLogger.Log($"Updated String: {__instance.skill_Name}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(__instance.skill_Name, " initStaticSkill");
                }

                if (Translator.TryGetTranslation(__instance.skill_Desc, out string skillDescTranslation))
                {
                    UMTLogger.Log($"Found matching string!: {skillDescTranslation}");
                    __instance.skill_Desc = skillDescTranslation;
                    UMTLogger.Log($"Updated String: {__instance.skill_Desc}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(__instance.skill_Desc, " initStaticSkill");
                }
            }
        }

        //Database Info
        [HarmonyPatch(typeof(TuJianDB), nameof(TuJianDB.InitDB))]
        static class TuJianDB_InitDB_Patch
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
                        UMTLogger.Log($"Trying to translate: {kvp.Value}");
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
                        UMTLogger.Log($"Trying to translate: {kvp.Value}");
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

        //I think this is related to stores
        [HarmonyPatch(typeof(USelectNum), "Show")]
        static class USelectNum_Show_Patch
        {
            static void Prefix(ref string desc)
            {
                //              UMTLogger.Log($"Trying to translate: {desc}");
                if (Translator.TryGetTranslation(desc, out string descTranslation))
                {
                    //                 UMTLogger.Log($"Found matching string!: {MainScript.translationDict[desc]}");
                    desc = descTranslation;
                    //                  UMTLogger.Log($"Updated String: {desc}");
                }
                else
                {
                    MainScript.AddFailedStringToDict(desc, " USelectNum_Show_Patch");
                }
            }
        }


        //This patch is for Items Trading Interest 
        [HarmonyPatch(typeof(jsonData), "InitLogic")]
        static class jsonData_InitLogic
        {
            static AccessTools.FieldRef<jsonData, JObject> AllItemLeiXinRef =
            AccessTools.FieldRefAccess<jsonData, JObject>("AllItemLeiXin");

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

                    }
                }
            }

        }
        /*
            [HarmonyPatch]
            static class UIInput_ValidatePatch
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


        [HarmonyPatch]

        static class Transpiler1_patch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(ToolTipsMag), "CreateShuXing", new Type[] { typeof(Bag.BaseSkill) });
                yield return AccessTools.Method(typeof(ToolTipsMag), "CreateShuXing", new Type[] { typeof(Bag.BaseItem) });
                yield return AccessTools.Method(typeof(Bag.BaseSkill), "GetPinJieName");
                yield return AccessTools.Method(typeof(Bag.BaseSkill), "GetPinJie");
                yield return AccessTools.Method(typeof(Skill_UIST), "Show_Tooltip");
                yield return AccessTools.Method(typeof(ShenTongInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(KuangShiInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(CaoYaoInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(YaoShouInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(DanYaoInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(GongFaInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(MiShuInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(YaoShouCaiLiaoInfoPanel), "RefreshPanelData");

            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && Translator.TryGetTranslation(codes[i].operand.ToString(), out string translation))
                    {
                        codes[i].operand = translation;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch]

        static class Transpiler2_patch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(item), "StudyTiaoJian", null, null);
                yield return AccessTools.Method(typeof(WuDaoTooltip), "Show", new Type[]
                {
                typeof(Sprite),
                typeof(int),
                typeof(UnityAction)
                }, null);
                yield return AccessTools.Method(typeof(WuDaoCellTooltip), "open", null, null);
                yield return AccessTools.Method(typeof(SiXuData), "Init", null, null);
                yield return AccessTools.Method(typeof(SiXuManager), "initLingGuan", null, null);
                yield return AccessTools.Method(typeof(BiGuanYinfo), "getTaskNextTime", null, null);
                yield return AccessTools.Method(typeof(BiGuanYinfo), "Update", null, null);
                yield return AccessTools.Method(typeof(TaskCell), "getTaskNextTime", null, null);
                yield return AccessTools.Method(typeof(MainUIDataCell), "Init", null, null);
                yield return AccessTools.Method(typeof(Tools), "getStr", null, null);
                yield return AccessTools.Method(typeof(UIBiGuan), "OK", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanXiuLianPanel), "OnStartBiGuanClick", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanPanel), "RefreshKeFangTime", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanXiuLianPanel), "RefreshSpeedUI", null, null);
                yield return AccessTools.Method(typeof(createAvatarChoice), "setValue", null, null);
                yield return AccessTools.Method(typeof(createTianfu), "Awake", null, null);
                yield return AccessTools.Method(typeof(MainUISelectTianFu), "NextPage", null, null);
                yield return AccessTools.Method(typeof(DanLuBag), "GetQualityData", null, null);
                yield return AccessTools.Method(typeof(LianDanPanel), "GetCostTime", null, null);
                yield return AccessTools.Method(typeof(LianDanResultManager), "getCostTime", null, null);
                yield return AccessTools.Method(typeof(LianDanResultManager), "lianDanJieSuan", null, null);
                yield return AccessTools.Method(typeof(LianQiController), "GetItemDesc", null, null);
                yield return AccessTools.Method(typeof(LianQiResultManager), "setEquipNameClick", null, null);
                yield return AccessTools.Method(typeof(Tools), "TimeToShengYuTime", null, null);
                yield return AccessTools.Method(typeof(PaiMaiHang), "JiMai", null, null);
                yield return AccessTools.Method(typeof(UIMiniTaskPanel), "RefreshChuanWen", null, null);
                yield return AccessTools.Method(typeof(XiuLian), "Update", null, null);
                yield return AccessTools.Method(typeof(NPCEx), "AddFavor", null, null);
                yield return AccessTools.Method(typeof(NPCEx), "AddQingFen", null, null);
                yield return AccessTools.Method(typeof(NPCEx), "ZengLiToNPC", null, null);
                yield return AccessTools.Method(typeof(LingHeCaiJiUIMag), "OpenCaiJi", null, null);
                yield return AccessTools.Method(typeof(showCaiLiaoImage), "Click", null, null);
                yield return AccessTools.Method(typeof(JianLingManager), "UnlockXianSuo", null, null);
                yield return AccessTools.Method(typeof(ShowEquipCell), "getEquipDesc", null, null);
                yield return AccessTools.Method(typeof(RandomEquip), "GetEquipQualityDesc", null, null);
                yield return AccessTools.Method(typeof(ShowEquipCell), "setEquipPingJie", null, null);
                yield return AccessTools.Method(typeof(ShowEquipCell), "updateEquipPingJie", null, null);
                //yield return AccessTools.Method(typeof(UIJianLingQingJiaoPanel), "Refresh", null, null); TODO: figure out why this doesn't get recognized by compiler
                yield return AccessTools.Method(typeof(DanGeDanFang_UI), "init", null, null);
                yield return AccessTools.Method(typeof(headUIMag), "showHeadUI", null, null);
                yield return AccessTools.Method(typeof(WuDaoDianPanel), "Show", null, null);
                yield return AccessTools.Method(typeof(Tools), "getSkillText", null, null);
                yield return AccessTools.Method(typeof(LingGuangMag), "ClickLingGuangCell", null, null);
                yield return AccessTools.Method(typeof(LingGuangMag), "GanWu", null, null);
                yield return AccessTools.Method(typeof(LingGuangMag), "GetShengYuShiJian", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanTuPoPanel), "RefreshInventory", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanTuPoPanel), "SetNull", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanTuPoPanel), "TuPo", null, null);
                yield break;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && Translator.TryGetTranslation(codes[i].operand.ToString(), out string translation))
                    {
                        codes[i].operand = translation;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch]

        static class LoadAndSaveMenus
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(MainUIDataCell), "Click");
                yield return AccessTools.Method(typeof(AvatarInfoCell), "click");
                yield return AccessTools.Method(typeof(Tab.TabDataBase), "Load");
                yield return AccessTools.Method(typeof(Tab.TabDataBase), "Save");

            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {

                    }

                    if (codes[i].opcode == OpCodes.Ldstr && Translator.TryGetTranslation(codes[i].operand.ToString(), out string translation))
                    {
                        codes[i].operand = translation;


                    }

                }
                return codes.AsEnumerable();


            }
        }



        [HarmonyPatch(typeof(JSONObject), "Str", MethodType.Getter)]
        static class JSONObject_Patch
        {
            static AccessTools.FieldRef<JSONObject, string> strRef =
            AccessTools.FieldRefAccess<JSONObject, string>("str");
            static void Postfix(JSONObject __instance, ref string __result)
            {
                try
                {
                    // UMTLogger.Log($"Trying to translate: {__result}");
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
                catch (Exception e)
                {
                    UMTLogger.Log(e.ToString());
                }
            }
            static void Postfix(ref string __result)
            {
                try
                {
                    // UMTLogger.Log($"Trying to translate: {__result}");
                    if (Translator.TryGetTranslation(__result, out string translation))
                    {
                        //     UMTLogger.Log($"Found matching string!: {MainScript.translationDict[__result]}");
                        __result = translation;
                        //__instance.str = MainScript.translationDict[__result];

                        //     UMTLogger.Log($"Updated String: {__result}");
                    }
                    else
                    {
                        //    MainScript.AddFailedStringToDict(__result, " USelectNum_Show_Patch");
                    }
                }
                catch (Exception e)
                {
                    UMTLogger.Log(e.ToString());
                }
            }

        }
    }
}
