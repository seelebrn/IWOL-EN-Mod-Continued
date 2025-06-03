using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModularTranslator;
using UnityModularTranslator.Translation;

namespace EngTranslatorMod.Main
{
    public class TranslationManager : MonoBehaviour
    {

        List<Text> knowTexts = new List<Text>();

        private void Init()
        {
            Translator.Initialize(Path.Combine(MainScript.sourceDir, "Translations"));
            DontDestroyOnLoad(gameObject);
        }


        public void Start()
        {
            Init();

            UMTLogger.Log("Translator Kun is alive");
            UMTLogger.Log($"Source Dir Check = {MainScript.sourceDir}");
            UMTLogger.Log($"Parent Source Dir Check = {Directory.GetParent(MainScript.sourceDir)}");
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.F9))
            {
                UMTLogger.Log("--- Logging failed strings ---");
                foreach (KeyValuePair<string, string> kvp in MainScript.FailedStringsDict)
                {
                    UMTLogger.Log($"'{kvp.Key}' in: {kvp.Value}");
                    UMTLogger.Log($"'{Regex.Replace(kvp.Key, @"\s*(\n)", string.Empty)}'");
                }
                UMTLogger.Log("--- Finished logging failed strings ---");
            }
        }


        private void LateUpdate()
        {
            knowTexts = knowTexts.Where(x => x != null).ToList();
            //TextTranslator();
        }

        private void TextTranslator()
        {
            List<GameObject> rootObjectsInScene = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjectsInScene);

            foreach (Text text in FindObjectsOfType<Text>())
            {
                if (!knowTexts.Contains(text))
                {
                    knowTexts.Add(text);
                    text.fontSize = text.fontSize * 3 / 4;
                }

                if (Helpers.IsChinese(text.text))
                {
                    if (Translator.TryGetTranslation(text.text, out string translatedText))
                    {
                        text.text = translatedText;
                    }
                    else
                    {
                        MainScript.AddFailedStringToDict(text.text, " TranslatorKun - TextAsset");
                    }
                }
            }

            foreach (UILabel text in FindObjectsOfType<UILabel>())
            {
                if (Helpers.IsChinese(text.text))
                {
                    if (Translator.TryGetTranslation(text.text, out string translatedText))
                    {
                        text.text = translatedText;
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
