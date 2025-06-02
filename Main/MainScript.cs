using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EngTranslatorMod.Main
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



    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MainScript : BaseUnityPlugin
    {


        public const string pluginGuid = "Cadenza.IWOL.EnMod";
        public const string pluginName = "ENMod Continued";
        public const string pluginVersion = "0.6";
        public static StripedWhiteSpaceCompare comparer = new StripedWhiteSpaceCompare();
        public static string sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString();
        public static string parentDir = Directory.GetParent(sourceDir).ToString();
        public static string configDir = Path.Combine(parentDir, "config");
        public static Dictionary<string, string> UILabelsDict;
        public static Dictionary<string, string> TextAssetDict;
        public static Dictionary<string, string> TextAssetDict1;
        public static Dictionary<string, string> TextAssetDict2;
        public static Dictionary<string, string> FungusSayDict;
        public static Dictionary<string, string> FungusMenuDict;
        public static Dictionary<string, string> etcDict;
        public static Dictionary<string, string> FailedStringsDict = new Dictionary<string, string>(); //String Name, Location; no comparer passed to avoid fuzzy matching invalid strings

        public static void TranslateDictionary<T1>(Dictionary<T1, JSONObject> dict, List<string> fields)
        {
            foreach (KeyValuePair<T1, JSONObject> kvp in dict)
            {
                JSONObject jsonObject = kvp.Value;
                foreach (string field in fields)
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

                string[] arr = line.Split('¤');
                if (arr[0] != arr[1])
                {
                    KeyValuePair<string, string> pair = new KeyValuePair<string, string>(Regex.Replace(arr[0], @"\t|\n|\r", ""), arr[1]);
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
                Dictionary<string, string> _UITextDict = FileToDictionary("UIText.txt");

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

                //TODO: see if these are needed after moving to UMT
                //translationDict = new Dictionary<string, string>().MergeLeft(TextAssetDict, UILabelsDict);
                //translationDict = new Dictionary<string, string>().MergeLeft(translationDict, FungusMenuDict);


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

                //TODO: see if these are needed after moving to UMT
                //translationDict = new Dictionary<string, string>(translationDict, comparer);


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
                gameObject.AddComponent<TranslationManager>();

                Harmony harmony = new Harmony("Cadenza.IWOL.EnMod");
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            }
            catch (Exception e)
            {
                Debug.Log("Error in applying harmony patches");
                Debug.LogException(e);
            }




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
}