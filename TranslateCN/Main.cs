using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using I2.Loc;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using TMPro;
using HoldfastGame;

namespace TranslateCN
{
    static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Hashtable translateBox = new Hashtable();
        public static Hashtable translateReplace = new Hashtable();
        public static Hashtable missBox = new Hashtable();
        public static TMP_FontAsset tmpfa;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            logger = modEntry.Logger;
            logger.Log("翻译插件开始加载 patch id:" + modEntry.Info.Id);
            loadLanguageFile();
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("翻译插件注入完毕");
            translateReplace.Add("（", "( ");
            translateReplace.Add("）", " )");
            translateReplace.Add("，", ", ");
            translateReplace.Add("。", ". ");
            translateReplace.Add("「", " \" ");
            translateReplace.Add("」", " \" ");
            translateReplace.Add("：", " : ");
            return true;
        }
        
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            loadFont();
            enabled = value;
            if (!enabled) {
                string obj = "{";
                foreach (string key in missBox.Keys)
                {
                    obj += string.Format("\"{0}\":\"{1}\",", key, missBox[key]);
                }
                obj += "}";
                string path = string.Format("{0}\\Mods\\translateCn\\{1}",
                    Directory.GetCurrentDirectory(), "miss.json");
                File.WriteAllText(path, obj);
                logger.Log("缺失的翻译已保存到" + path);
            }
            else
            {
                loadLanguageFile();
            }
            return true;
        }

        private static void loadLanguageFile()
        {
            translateBox.Clear();
            string path = string.Format("{0}\\Mods\\translateCn\\{1}", 
                Directory.GetCurrentDirectory(), "lang.json");
            if (!File.Exists(path)) {
                logger.Log("找不到语言文件，配置加载失败，将使用游戏默认设定。");
                return;
            }
            int count = 0;
            using (StreamReader file = File.OpenText(path))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    foreach (KeyValuePair<string, JToken> item in o)
                    {
                        string value = item.Value.ToString();
                        foreach(string key in translateReplace.Keys)
                        {
                            value = value.Replace(key, (string)translateReplace[key]);
                        }
                        translateBox.Add(item.Key, value);
                        count++;
                    }
                }
            }
            logger.Log(string.Format("翻译文件加载完毕，词条数{0}", count));
        }

        [HarmonyPatch(typeof(LocalizationManager))]
        [HarmonyPatch("GetTranslation")]
        public static class Translation_Patch
        {
            static void Postfix(string Term, ref string __result)
            {
                if (Term == "NotoSansCJKkr-Medium SDF" || __result == null) return;
                if (!translateBox.ContainsKey(Term) && !missBox.ContainsKey(Term))
                {
                    logger.Log(string.Format("发现新词条:{0}:{1}", Term, __result));
                    missBox.Add(Term, __result);
                }
                if (translateBox.ContainsKey(Term))
                {
                    __result = (string)translateBox[Term];
                }
            }
        }
        
        [HarmonyPatch(typeof(UIChatEntry), "SetEntry")]
        public static class Font_Patch
        {
            static void Postfix(UIChatEntry __instance, string textEntry)
            {
                foreach (string key in translateReplace.Keys)
                {
                    __instance.messageField.text = 
                        __instance.messageField.text.Replace(key, (string)translateReplace[key]);
                }
            }
        }

        private static void loadFont() {
            string path = string.Format("{0}\\Mods\\translateCn\\{1}",
                Directory.GetCurrentDirectory(), "font.asset");
            tmpfa = Resources.Load<TMP_FontAsset>(path);
            logger.Log("加载字体中：" + path);
            if (tmpfa == null) logger.Log("加载字体失败");
            else {
                logger.Log("加载字体完毕，字典如下");
                logger.Log(TMP_FontAsset.GetCharacters(tmpfa));
            }
        }
    }
}
