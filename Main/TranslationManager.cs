using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModularTranslator.Translation;

namespace EngTranslatorMod.Main
{
    public class TranslationManager : MonoBehaviour
    {

        List<Text> knowTexts = new List<Text>();

        private void Init()
        {
            Translator.Initialize();
            DontDestroyOnLoad(gameObject);
        }


        public void Awake()
        {
            Init();

            Debug.Log("Translator Kun is alive");
            Debug.Log($"Source Dir Check = {MainScript.sourceDir}");
            Debug.Log($"Parent Source Dir Check = {Directory.GetParent(MainScript.sourceDir)}");
            Debug.Log($"Source Config Dir Check{MainScript.configDir}");
        }

        public void LogCurrentSceneName()
        {
            Scene scene = SceneManager.GetActiveScene();
            Debug.Log(scene.name);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.F10))
            {
                LogCurrentSceneName();
            }
            if (Input.GetKeyUp(KeyCode.F9))
            {
                Debug.Log("--- Loggin failed strings ---");
                foreach (KeyValuePair<string, string> kvp in MainScript.FailedStringsDict)
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

                    if (MainScript.TextAssetDict.ContainsKey(text.text))
                    {
                        Debug.Log("Found");
                        text.text = MainScript.TextAssetDict[text.text];
                    }
                    else
                    {
                        MainScript.AddFailedStringToDict(text.text, " TranslatorKun - TextAsset");
                    }
                }
            }

            foreach (UILabel text in allUILabelComponents)
            {
                if (Helpers.IsChinese(text.text))
                {
                    if (MainScript.UILabelsDict.ContainsKey(text.text))
                    {
                        text.text = MainScript.UILabelsDict[text.text];
                    }
                    else
                    {
                        MainScript.AddFailedStringToDict(text.text, " TranslatorKun - UILabel");
                    }
                }
            }
        }

    }
}
