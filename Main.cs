using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using Fungus;
using GUIPackage;
using UnityEngine.Events;
using YSGame.TuJian;
using System.Globalization;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using XUnity.AutoTranslator.Plugin.BepInEx;




namespace EngTranslatorMod
{
    public class StripedWhiteSpaceCompare : IEqualityComparer<string>
    {
        public static string RegexPurify(string s1)
        {
            //return Regex.Replace(Regex.Unescape(s1), @"\s*(\n)", string.Empty);
            return Regex.Replace(s1, @"\s*(\n)", string.Empty);
        }
        public bool CompareString(string s1, string s2)
        {
            //Debug.Log("1");
            return RegexPurify(s1) == RegexPurify(s2);
        }
        bool IEqualityComparer<string>.Equals(string x, string y)
        {
            //Debug.Log("2");
            return CompareString(x, y);
        }

        int IEqualityComparer<string>.GetHashCode(string obj)
        {
            return RegexPurify(obj).GetHashCode();
        }
    }

    public static class Helpers
    {
        public static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
        public static bool IsChinese(string s)
        {
            return cjkCharRegex.IsMatch(s);
        }
    }






    public class TranslatorKun : MonoBehaviour
    {

        List<Text> knowTexts = new List<Text>();

        public TranslatorKun Init()
        {
            DontDestroyOnLoad(gameObject);


            Scene scene = SceneManager.GetActiveScene();
            Debug.Log("Scene Name = " + scene.name);

            gameObject.SetActive(true);
            Debug.Log(gameObject.activeInHierarchy.ToString());
            return this;
        }


        public void Awake()
        {
            Debug.Log("Translator Kun is alive");
            Debug.Log("Source Dir Check = " + Main.sourceDir);
            Debug.Log("Parent Source Dir Check = " + Directory.GetParent(Main.sourceDir));
            Debug.Log("Source Config Dir Check" + Main.configDir);

        }

        public void LogCurrentSceneName()
        {
            Scene scene = SceneManager.GetActiveScene();
            Debug.Log(scene.name);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.F10) == true)
            {
                LogCurrentSceneName();
            }
            if (Input.GetKeyUp(KeyCode.F9) == true)
            {
                Debug.Log("--- Loggin failed strings ---");
                foreach (var kvp in Main.FailedStringsDict)
                {
                    Debug.Log($"'{kvp.Key}' in: {kvp.Value}");
                    Debug.Log($"'{Regex.Replace(kvp.Key, @"\s*(\n)", string.Empty)}'");
                }
                Debug.Log("--- Finished logging failed strings ---");
            }
        }


        private void LateUpdate()
        {
            knowTexts = knowTexts.Where(x => x != null).ToList();
            TextTranslator();
        }

        private void TextTranslator()
        {

            List<GameObject> rootObjectsInScene = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjectsInScene);

            Text[] allTextComponents = FindObjectsOfType<Text>();
            UILabel[] allUILabelComponents = FindObjectsOfType<UILabel>();
            foreach (Text text in allTextComponents)
            {
                if (!knowTexts.Contains(text))
                {
                    knowTexts.Add(text);
                    text.fontSize = text.fontSize * 3 / 4;
                }

                if (Helpers.IsChinese(text.text))
                {

                    if (Main.TextAssetDict.ContainsKey(text.text))
                    {
                        Debug.Log("Found");
                        text.text = Main.TextAssetDict[text.text];
                    }
                    else
                    {
                        Main.AddFailedStringToDict(text.text, " TranslatorKun - TextAsset");
                    }
                }
            }

            foreach (UILabel text in allUILabelComponents)
            {
                if (Helpers.IsChinese(text.text))
                {
                    if (Main.UILabelsDict.ContainsKey(text.text))
                    {
                        text.text = Main.UILabelsDict[text.text];
                    }
                    else
                    {
                        Main.AddFailedStringToDict(text.text, " TranslatorKun - UILabel");
                    }
                }
            }
        }

    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {


        public const string pluginGuid = "Cadenza.IWOL.EnMod";
        public const string pluginName = "ENMod Continued";
        public const string pluginVersion = "0.5";
        public static bool enabled;
        public static bool enabledDebugLogging = false;
        public static StripedWhiteSpaceCompare comparer = new StripedWhiteSpaceCompare();
        public static Dictionary<string, string> translationDict;
        public static string sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString();
        public static string parentDir = Directory.GetParent(Main.sourceDir).ToString();
        public static string configDir = Path.Combine(parentDir, "config");
        public static Dictionary<string, string> UILabelsDict;
        public static Dictionary<string, string> TextAssetDict;
        public static Dictionary<string, string> TextAssetDict1;
        public static Dictionary<string, string> TextAssetDict2;
        public static Dictionary<string, string> FungusSayDict;
        public static Dictionary<string, string> FungusMenuDict;
        public static Dictionary<string, string> etcDict;
        public static TranslatorKun translatorKun;




        public static Dictionary<string, string> FailedStringsDict = new Dictionary<string, string>(); //String Name, Location; no comparer passed to avoid fuzzy matching invalid strings

        public static void TranslateDictionary<T1>(Dictionary<T1, JSONObject> dict, List<string> fields)
        {
            foreach (var kvp in dict)
            {
                JSONObject jsonObject = kvp.Value;
                foreach (var field in fields)
                {
                    if (jsonObject.HasField(field))
                    {
                        Debug.Log($"Trying to translate: {jsonObject["field"].Str}");
                    }
                }


            }
        }

        public static void AddFailedStringToDict(string s, string location)
        {


            if (FailedStringsDict.ContainsKey(s))
            {

                return;
            }
            FailedStringsDict.Add(s, location);

        }

        public static Dictionary<string, string> FileToDictionary(string dir)
        {
            string ExcludePattern1 = "^神秘铁剑.*$";
            string ExcludePattern2 = "^昔日身份.*$";
            string ExcludePattern3 = "^魔道踪影.*$";
            string ExcludePattern4 = "^御剑门之谜.*$";
            string ExcludePattern5 = "^往昔追忆开局$";
            string ExcludePattern6 = "^为神秘铁剑$";
            string ExcludePattern7 = "^神秘铁剑$";
            string ExcludePattern8 = "^御剑门传闻$";
            string ExcludePattern9 = "^御剑门倪家传闻$";
            string ExcludePattern10 = "^(剑门传闻|英杰会冠军|英杰会冠军|拜入星河|突破筑基|御剑门金虹传闻|御剑门离火门传闻|御剑门竹山宗传闻|御剑门公孙家传闻|御剑门化尘传闻|突破金丹|拜入金虹|加入内门|宗门大比夺魁|猎魔冠军|结为道侣|拜入化尘|首次出海|龙族供奉|突破元婴|天机大比冠军|大道感悟|大道感悟|渡劫飞升|天道福泽|逆天造化)$";
            string ExcludePattern11 = "^(神秘铁剑|神秘铁剑杀戮剑灵|神秘铁剑天魔眼|神秘铁剑戮仙剑|神秘铁剑天道树|往昔追忆开局|昔日身份异常剑灵|昔日身份御剑门|昔日身份古迹|昔日身份玄清|昔日身份戮仙剑|魔道踪影天魔道一|魔道踪影天魔道二|魔道踪影天魔道三|魔道踪影天魔道四|魔道踪影古神教一|魔道踪影古神教二|魔道踪影古神教三|魔道踪影古神教四|魔道踪影血剑宫初|魔道踪影血剑宫一|魔道踪影血剑宫二|魔道踪影血煞符|御剑门传闻|御剑门倪家传闻|御剑门公孙家传闻|御剑门竹山宗传闻|御剑门离火门传闻|御剑门金虹传闻|御剑门星河传闻|御剑门化尘传闻|御剑门之战真相)$";
            string ExcludePattern12 = "^(资质|灵根|天道化身|悟性|遁速|神识|心境|灵石|气血|寿元|灵感|丹毒|神秘铁剑|神秘铁剑杀戮剑灵|神秘铁剑天魔眼|神秘铁剑戮仙剑|神秘铁剑天道树|往昔追忆开局|昔日身份异常剑灵|昔日身份御剑门|昔日身份古迹|昔日身份玄清|昔日身份戮仙剑|魔道踪影天魔道一|魔道踪影天魔道二|魔道踪影天魔道三|魔道踪影天魔道四|魔道踪影古神教一|魔道踪影古神教二|魔道踪影古神教三|魔道踪影古神教四|魔道踪影血剑宫初|魔道踪影血剑宫一|魔道踪影血剑宫二|魔道踪影血煞符|御剑门传闻|御剑门倪家传闻|御剑门公孙家传闻|御剑门竹山宗传闻|御剑门离火门传闻|御剑门金虹传闻|御剑门星河传闻|御剑门化尘传闻|御剑门之战真相|自定义生平|突破筑基|突破金丹|突破元婴|突破化神|渡劫飞升|拜入竹山|拜入离火|拜入金虹|拜入星河|拜入化尘|英杰会冠军|天机大比冠军|宗门大比夺魁|五行剑诀传承|登仙殿传承|击败九幽|击败浪方|击败吞云|请教吞云|阴魂岛见证|加入内门|结为道侣|疏远毕业|拉住师尊|任他离开|猎魔冠军|首次出海|龙族供奉|通天灵宝|大道感悟|白帝飞升|天字杀手|特殊飞升见证|普通飞升见证|特殊陨落见证|普通陨落见证)$";
            Dictionary<string, string> dict = new Dictionary<string, string>();

            IEnumerable<string> lines = File.ReadLines(Path.Combine(sourceDir, "Translations", dir));

            foreach (string line in lines)
            {

                var arr = line.Split('¤');
                if (arr[0] != arr[1])
                {
                    var pair = new KeyValuePair<string, string>(Regex.Replace(arr[0], @"\t|\n|\r", ""), arr[1]);
                    if (!Regex.IsMatch(arr[0], ExcludePattern1) && !Regex.IsMatch(arr[0], ExcludePattern2) && !Regex.IsMatch(arr[0], ExcludePattern3) && !Regex.IsMatch(arr[0], ExcludePattern4) && !Regex.IsMatch(arr[0], ExcludePattern5) && !Regex.IsMatch(arr[0], ExcludePattern6) && !Regex.IsMatch(arr[0], ExcludePattern7) && !Regex.IsMatch(arr[0], ExcludePattern8) && !Regex.IsMatch(arr[0], ExcludePattern9) && !Regex.IsMatch(arr[0], ExcludePattern10) && !Regex.IsMatch(arr[0], ExcludePattern11) && !Regex.IsMatch(arr[0], ExcludePattern12))
                    {
                        if (!dict.ContainsKey(pair.Key))
                            dict.Add(pair.Key, pair.Value);
                        else
                            Debug.Log($"Found a duplicated line while parsing {dir}: {pair.Key}");
                    }
                    else
                    {
                        Debug.Log("Not touching this with a 10ft pole : " + arr[0] + " - Translation would be : " + arr[1]);
                    }
                }
            }

            return dict;

            //return File.ReadLines(Path.Combine(BepInEx.Paths.PluginPath, "Translations", dir))
            //    .Select(line =>
            //    {
            //        var arr = line.Split('¤');
            //        return new KeyValuePair<string, string>(Regex.Replace(arr[0], @"\t|\n|\r", ""), arr[1]);
            //    })
            //    .GroupBy(kvp => kvp.Key)
            //    .Select(x => x.First())
            //    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, comparer);
        }



        public void Awake()
        {
            //Preparing FailedRegistry.txt - Step 1

            string FailedRegistry = Path.Combine(BepInEx.Paths.PluginPath, "MissedStrings.txt");
            using (FileStream fs = File.Open(FailedRegistry, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                lock (fs)
                {
                    fs.SetLength(0);
                }

            }
            try
            {
                //Preparing FailedRegistry.txt - Step 2 
                using (StreamWriter sw = File.AppendText(FailedRegistry))
                {
                    string Hello = "Hey ! Below, you'll find any untranslated lines in the game .json files !";
                    sw.Write(Hello);
                    sw.Write(Environment.NewLine);
                    sw.Write("---");
                    sw.Write(Environment.NewLine);
                    sw.Write(Environment.NewLine);
                }

                //We merge these two dictionaries, we also do it on TextAsset cause it's larger so this is more performant
                var _UITextDict = FileToDictionary("UIText.txt");

                TextAssetDict = FileToDictionary("TextAsset.txt");
                etcDict = FileToDictionary("etc.txt");
                TextAssetDict = TextAssetDict.MergeLeft(etcDict);

                /*
                File.WriteAllLines(
"C:\\Program Files (x86)\\Steam\\steamapps\\common\\觅长生\\BepInEx\\plugins\\TextAssetBeforeMergeLeft.txt",
TextAssetDict.Select(kvp => string.Format("{0};{1}", kvp.Key, kvp.Value)));*/
                TextAssetDict = _UITextDict.MergeLeft(TextAssetDict);
                /*                File.WriteAllLines(
                "C:\\Program Files (x86)\\Steam\\steamapps\\common\\觅长生\\BepInEx\\plugins\\TextAssetAfterMergeLeft.txt",
                TextAssetDict.Select(kvp => string.Format("{0};{1}", kvp.Key, kvp.Value)));
                                */

                TextAssetDict = new Dictionary<string, string>(TextAssetDict, comparer);
                /*
                                File.WriteAllLines(
                "C:\\Program Files (x86)\\Steam\\steamapps\\common\\觅长生\\BepInEx\\plugins\\TextAssetAftercomparer.txt",
                TextAssetDict.Select(kvp => string.Format("{0};{1}", kvp.Key, kvp.Value)));
                */
                UILabelsDict = FileToDictionary("UILabel.txt");

                FungusSayDict = FileToDictionary("FungusSay.txt");
                FungusMenuDict = FileToDictionary("FungusMenu.txt");

                translationDict = new Dictionary<string, string>().MergeLeft(TextAssetDict, UILabelsDict);
                translationDict = new Dictionary<string, string>().MergeLeft(translationDict, FungusMenuDict);
                /*
                                File.WriteAllLines(
                "C:\\Program Files (x86)\\Steam\\steamapps\\common\\觅长生\\BepInEx\\plugins\\TextAsset.txt",
                TextAssetDict.Select(kvp => string.Format("{0};{1}", kvp.Key, kvp.Value)));*/
                /*
                File.WriteAllLines(
"C:\\Program Files (x86)\\Steam\\steamapps\\common\\觅长生\\BepInEx\\plugins\\TranslationDict.txt",
translationDict.Select(kvp => string.Format("{0};{1}", kvp.Key, kvp.Value)));*/
                /*                File.WriteAllLines(
                "C:\\Program Files (x86)\\Steam\\steamapps\\common\\觅长生\\BepInEx\\plugins\\UILabel.txt",
                UILabelsDict.Select(kvp => string.Format("{0};{1}", kvp.Key, kvp.Value)));
                */
                translationDict = new Dictionary<string, string>(translationDict, comparer);


                string s1 = "自古以来，人们从来没有停止过对长生之道的探索...\n上古时代的道家修士们,\n以自身灵根为根基，沟通天地灵气，\n辅以太极五行，阴阳八卦之力，外炼丹药，内修元神，\n留下了无数强大的修真法门。\n传说修为高深者，便能够引动天劫，破碎虚空，\n飞升到仙界，真正成为与天地同寿的“仙人”";
                string s1_1 = Regex.Replace(s1, @"\s*(\n)", string.Empty);
                string s2 = "自古以来，人们从来没有停止过对长生之道的探索...上古时代的道家修士们,以自身灵根为根基，沟通天地灵气，辅以太极五行，阴阳八卦之力，外炼丹药，内修元神，留下了无数强大的修真法门。传说修为高深者，便能够引动天劫，破碎虚空，飞升到仙界，真正成为与天地同寿的“仙人”";
                /*
                Debug.Log(s1_1);
                Debug.Log(s2);
                Debug.Log((s1_1==s2).ToString());
                Debug.Log(TextAssetDict.Comparer.GetType().FullName);
                Debug.Log($"Hello KeyCheck:{TextAssetDict.ContainsKey(s2)}");
                */

            }
            catch (Exception e)
            {
                Debug.Log("Error in generating dicts");
                Debug.LogException(e);
            }

            try
            {

                //UnityExplorer.ExplorerStandalone.CreateInstance();
                translatorKun = CreateTranslatorObject();

                var harmony = new Harmony("Cadenza.IWOL.EnMod");
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            }
            catch (Exception e)
            {
                Debug.Log("Error in applying harmony patches");
                Debug.LogException(e);
            }




        }


        static TranslatorKun CreateTranslatorObject()
        {
            GameObject translatorObject = new GameObject("TranslatorKun");
            return GameObject.Instantiate(translatorObject).AddComponent<TranslatorKun>().Init();
        }
        public static void PrintDict(Dictionary<string, string> dictionary)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                sb.AppendLine($"Key = {kvp.Key}, Value = {kvp.Value}");
            }
            Debug.Log(sb.ToString());
        }



    }

    [HarmonyPatch(typeof(XUnity.AutoTranslator.Plugin.BepInEx.AutoTranslatorPlugin), "ConfigPath", MethodType.Getter)]
    static class XUnityTweaks1
    {
        static void Postfix(XUnity.AutoTranslator.Plugin.BepInEx.AutoTranslatorPlugin __instance, ref string __result)
        {

            __result = Main.configDir;
            Debug.Log(__result);

        }
    }
    [HarmonyPatch(typeof(XUnity.AutoTranslator.Plugin.BepInEx.AutoTranslatorPlugin), "TranslationPath", MethodType.Getter)]
    static class XUnityTweaks2
    {
        static void Postfix(XUnity.AutoTranslator.Plugin.BepInEx.AutoTranslatorPlugin __instance, ref string __result)
        {

            __result = Directory.GetParent(Main.sourceDir).ToString();
            Debug.Log(__result);

        }
    }

    [HarmonyPatch(typeof(ChuanYingManager), "ReadData")]
    static class ChuanYingManager_ReadData_Patch
    {
        static void Prefix(ref JSONObject obj)
        {
            try
            {
                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {obj["info"].str}");
                if (Main.translationDict.ContainsKey(obj["info"].str))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[obj["info"].str]}");
                    obj["info"].str = Main.translationDict[obj["info"].str];
                }
                else
                {
                    Main.AddFailedStringToDict(obj["info"].str, "ChuanYingManager_ReadData_Patch");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    //I think this is the messages you get from NPCs
    [HarmonyPatch(typeof(CyEmail), "GetContent")]
    static class CyEmailr_GetContent
    {
        static void Prefix(ref string msg, EmailData emailData)
        {

            try
            {
                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {msg}");
                if (Main.translationDict.ContainsKey(msg))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[msg]}");
                    msg = Main.translationDict[msg];
                }
                else
                {
                    Main.AddFailedStringToDict(msg, " CyEmailr_GetContent");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
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
            var storyText = storyTextRef(__instance);
            try
            {
                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {storyText}");
                if (Main.FungusSayDict.ContainsKey(storyText))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.FungusSayDict[storyText]}");
                    storyTextRef(__instance) = Main.FungusSayDict[storyText];
                    if (Main.enabledDebugLogging) Debug.Log($"Updated String: {storyTextRef(__instance)}");
                }
                else
                {
                    Main.AddFailedStringToDict(storyText, " Say_OnEnter_Patch");
                }

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    //Fungus dialog box
    [HarmonyPatch(typeof(Flowchart), "SubstituteVariables")]
    static class Flowchart_SubstituteVariables_Patch
    {
        static void Prefix(ref string input)
        {
            try
            {
                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {input}");
                if (Main.FungusMenuDict.ContainsKey(input))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.FungusMenuDict[input]}");
                    input = Main.FungusMenuDict[input];
                    if (Main.enabledDebugLogging) Debug.Log($"Updated String: {input}");
                }
                else
                {
                    Main.AddFailedStringToDict(input, " Say_AddOption_Patch");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
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

                if (Main.translationDict.ContainsKey(__result))
                {
                    //if(Main.enabledDebugLogging)  Debug.Log($"Found matching string!: {Main.translationDict[__result]}");
                    __result = Main.translationDict[__result];
                    //Debug.Log($"Updated String: {__result}");
                }
                else
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {__result}");
                    Main.AddFailedStringToDict(__result, " Tools_Code64ToString_Patch");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
    [HarmonyPatch(typeof(Tools), "Code64")]
    static class Tools_Code64_Patch
    {

        static void Postfix(ref string __result)
        {
            try
            {

                if (Main.translationDict.ContainsKey(__result))
                {
                    //if(Main.enabledDebugLogging)  Debug.Log($"Found matching string!: {Main.translationDict[__result]}");
                    __result = Main.translationDict[__result];
                    //Debug.Log($"Updated String: {__result}");
                }
                else
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {__result}");
                    Main.AddFailedStringToDict(__result, " Tools_Code64ToString_Patch");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    // This is for active skills since they often get their values dynamically generated
    [HarmonyPatch(typeof(Tools), "getDesc")]
    static class Tools_getDesc_Patch
    {
        static void Prefix(ref string desstr)
        {
            try
            {
                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {desstr}");
                if (Main.translationDict.ContainsKey(desstr))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[desstr]}");
                    desstr = Main.translationDict[desstr];
                    if (Main.enabledDebugLogging) Debug.Log($"Updated String: {desstr}");
                }
                else
                {
                    Main.AddFailedStringToDict(desstr, " Tools_getDesc_Patch");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }


    [HarmonyPatch(typeof(Skill), "skillInit")]
    static class Tools_skillInit_Patch
    {

        static void Postfix(Skill __instance)
        {
            try
            {
                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {__instance.skill_Name}");
                if (Main.translationDict.ContainsKey(__instance.skill_Name))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[__instance.skill_Name]}");
                    __instance.skill_Name = Main.translationDict[__instance.skill_Name];
                    if (Main.enabledDebugLogging) Debug.Log($"Updated String: {__instance.skill_Name}");
                }
                else
                {
                    Main.AddFailedStringToDict(__instance.skill_Name, " Tools_skillInit_Patch");
                }

                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {__instance.skill_Desc}");
                if (Main.translationDict.ContainsKey(__instance.skill_Desc))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[__instance.skill_Desc]}");
                    __instance.skill_Desc = Main.translationDict[__instance.skill_Desc];
                    if (Main.enabledDebugLogging) Debug.Log($"Updated String: {__instance.skill_Desc}");
                }
                else
                {
                    Main.AddFailedStringToDict(__instance.skill_Desc, " Tools_skillInit_Patch");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

        }
    }


    [HarmonyPatch(typeof(Skill), "initStaticSkill")]
    static class Tools_initStaticSkill_Patch
    {

        static void Postfix(Skill __instance)
        {
            try
            {
                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {__instance.skill_Name}");
                if (Main.translationDict.ContainsKey(__instance.skill_Name))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[__instance.skill_Name]}");
                    __instance.skill_Name = Main.translationDict[__instance.skill_Name];
                    if (Main.enabledDebugLogging) Debug.Log($"Updated String: {__instance.skill_Name}");
                }
                else
                {
                    Main.AddFailedStringToDict(__instance.skill_Name, " initStaticSkill");
                }

                if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {__instance.skill_Desc}");
                if (Main.translationDict.ContainsKey(__instance.skill_Desc))
                {
                    if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[__instance.skill_Desc]}");
                    __instance.skill_Desc = Main.translationDict[__instance.skill_Desc];
                    if (Main.enabledDebugLogging) Debug.Log($"Updated String: {__instance.skill_Desc}");
                }
                else
                {
                    Main.AddFailedStringToDict(__instance.skill_Desc, " initStaticSkill");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

        }
    }

    //Database Info
    [HarmonyPatch(typeof(TuJianDB), nameof(TuJianDB.InitDB))]
    static class TuJianDB_InitDB_Patch
    {
        static void Postfix()
        {
            try
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
                    var mergeDict = new Dictionary<string, string>();

                    foreach (var kvp in dict)
                    {
                        if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {kvp.Value}");
                        if (Main.translationDict.ContainsKey(kvp.Value))
                        {
                            mergeDict[kvp.Key] = Main.translationDict[kvp.Value];

                        }
                        else
                        {
                            Main.AddFailedStringToDict(kvp.Value, " TuJianDB_InitDB_Patch");
                        }
                    }
                    mergeStringDicts.Add(mergeDict);
                }

                List<Dictionary<int, string>> mergeIntDicts = new List<Dictionary<int, string>>();
                foreach (Dictionary<int, string> dict in dictList)
                {
                    var mergeDict = new Dictionary<int, string>();
                    foreach (var kvp in dict)
                    {
                        if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {kvp.Value}");
                        if (Main.translationDict.ContainsKey(kvp.Value))
                        {
                            mergeDict[kvp.Key] = Main.translationDict[kvp.Value];
                        }
                        else
                        {
                            Main.AddFailedStringToDict(kvp.Value, " TuJianDB_InitDB_Patch");
                        }
                    }
                    mergeIntDicts.Add(mergeDict);
                }

                for (int i = 0; i < mergeStringDicts.Count; i++)
                {
                    var dict = mergeStringDicts[i];
                    foreach (var kvp in dict)
                    {
                        stringDictList[i][kvp.Key] = kvp.Value;
                    }
                }
                for (int i = 0; i < mergeIntDicts.Count; i++)
                {
                    var dict = mergeIntDicts[i];
                    foreach (var kvp in dict)
                    {
                        dictList[i][kvp.Key] = kvp.Value;
                    }
                }

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

        }
    }

    //I think this is related to stores
    [HarmonyPatch(typeof(USelectNum), "Show")]
    static class USelectNum_Show_Patch
    {
        static void Prefix(ref string desc)
        {
            try
            {
                //              if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {desc}");
                if (Main.translationDict.ContainsKey(desc))
                {
                    //                 if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[desc]}");
                    desc = Main.translationDict[desc];
                    //                  if (Main.enabledDebugLogging) Debug.Log($"Updated String: {desc}");
                }
                else
                {
                    Main.AddFailedStringToDict(desc, " USelectNum_Show_Patch");
                }
            }
            catch (Exception e)
            {
                //              Debug.Log(e.ToString());
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
            var AllItemLeiXin = AllItemLeiXinRef(__instance);

            try
            {
                foreach (KeyValuePair<string, JToken> kvp in AllItemLeiXin)
                {
                    if (Main.translationDict.ContainsKey((string)AllItemLeiXin[kvp.Key]["name"]))
                    {

                        AllItemLeiXin[kvp.Key]["name"] = Main.translationDict[(string)AllItemLeiXin[kvp.Key]["name"]];
                    }
                    else
                    {

                    }
                }


            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
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
            yield return AccessTools.Method(typeof(YaoShouCaiLiaoInfoPanel), "RefreshPanelData");
            yield return AccessTools.Method(typeof(Skill_UIST), "Show_Tooltip");
            yield return AccessTools.Method(typeof(ShenTongInfoPanel), "RefreshPanelData");
            yield return AccessTools.Method(typeof(KuangShiInfoPanel), "RefreshPanelData");
            yield return AccessTools.Method(typeof(CaoYaoInfoPanel), "RefreshPanelData");
            yield return AccessTools.Method(typeof(YaoShouInfoPanel), "RefreshPanelData");
            yield return AccessTools.Method(typeof(YaoShouCaiLiaoInfoPanel), "RefreshPanelData");
            yield return AccessTools.Method(typeof(DanYaoInfoPanel), "RefreshPanelData");
            yield return AccessTools.Method(typeof(GongFaInfoPanel), "RefreshPanelData");
            yield return AccessTools.Method(typeof(MiShuInfoPanel), "RefreshPanelData");


        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {


                if (codes[i].opcode == OpCodes.Ldstr && Main.translationDict.ContainsKey(codes[i].operand.ToString()))
                {
                    codes[i].operand = Main.translationDict[codes[i].operand.ToString()];
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
            yield return AccessTools.Method(typeof(item), "StudyTiaoJian");
            yield return AccessTools.Method(typeof(Tab.WuDaoTooltip), "Show", new Type[] { typeof(Sprite), typeof(int), typeof(UnityAction) });
            yield return AccessTools.Method(typeof(WuDaoCellTooltip), "open");
            yield return AccessTools.Method(typeof(SiXuData), "Init");
            yield return AccessTools.Method(typeof(BiGuanYinfo), "getTaskNextTime");
            yield return AccessTools.Method(typeof(BiGuanYinfo), "Update");
            yield return AccessTools.Method(typeof(TaskCell), "getTaskNextTime");
            yield return AccessTools.Method(typeof(MainUIDataCell), "Init");
            yield return AccessTools.Method(typeof(UIBiGuanPanel), "RefreshKeFangTime");
            yield return AccessTools.Method(typeof(Tools), "getStr");
            yield return AccessTools.Method(typeof(UIBiGuanXiuLianPanel), "RefreshSpeedUI");
            yield return AccessTools.Method(typeof(createAvatarChoice), "setValue");
            yield return AccessTools.Method(typeof(createTianfu), "Awake");
            yield return AccessTools.Method(typeof(MainUISelectTianFu), "NextPage");
            yield return AccessTools.Method(typeof(Bag.DanLuBag), "GetQualityData");

        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {


                if (codes[i].opcode == OpCodes.Ldstr && Main.translationDict.ContainsKey(codes[i].operand.ToString()))
                {
                    codes[i].operand = Main.translationDict[codes[i].operand.ToString()];
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
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr)
                {

                }

                if (codes[i].opcode == OpCodes.Ldstr && Main.translationDict.ContainsKey(codes[i].operand.ToString()))
                {
                    codes[i].operand = Main.translationDict[codes[i].operand.ToString()];


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
                // if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {__result}");
                if (Main.translationDict.ContainsKey(__result))
                {

                    //     if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[__result]}");
                    //        __result = Main.translationDict[__result];


                    __instance.str = Main.translationDict[__result];

                    //     if (Main.enabledDebugLogging) Debug.Log($"Updated String: {__result}");

                }
                else
                {
                    Main.AddFailedStringToDict(__result, " USelectNum_Show_Patch");
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
                Debug.Log(e.ToString());
            }
        }
        static void Postfix(ref string __result)
        {
            try
            {
                // if (Main.enabledDebugLogging) Debug.Log($"Trying to translate: {__result}");
                if (Main.translationDict.ContainsKey(__result))
                {
                    //     if (Main.enabledDebugLogging) Debug.Log($"Found matching string!: {Main.translationDict[__result]}");
                    __result = Main.translationDict[__result];
                    //__instance.str = Main.translationDict[__result];

                    //     if (Main.enabledDebugLogging) Debug.Log($"Updated String: {__result}");
                }
                else
                {
                    //    Main.AddFailedStringToDict(__result, " USelectNum_Show_Patch");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

    }





    public static class DictionaryExtensions
    {
        // Works in C#3/VS2008:
        // Returns a new dictionary of this ... others merged leftward.
        // Keeps the type of 'this', which must be default-instantiable.
        // Example: 
        //   result = map.MergeLeft(other1, other2, ...)
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>, new()
        {
            T newMap = new T();
            foreach (IDictionary<K, V> src in
                (new List<IDictionary<K, V>> { me }).Concat(others))
            {
                // ^-- echk. Not quite there type-system.
                foreach (KeyValuePair<K, V> p in src)
                {
                    newMap[p.Key] = p.Value;
                }
            }
            return newMap;
        }

        public static Dictionary<TKey, TValue>
        Merge<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
        {
            var result = new Dictionary<TKey, TValue>(dictionaries.First().Comparer);
            foreach (var dict in dictionaries)
                foreach (var x in dict)
                    result[x.Key] = x.Value;
            return result;
        }

    }

}

