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
using HoldfastGame;

namespace Translation
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Hashtable translateBox = new Hashtable();
        public static Hashtable translateReplace = new Hashtable();
        public static Hashtable missBox = new Hashtable();
        public static string basePath;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            basePath = modEntry.Path;
            modEntry.OnToggle = OnToggle;
            logger = modEntry.Logger;
            logger.Log("翻译插件开始加载 patch id:" + modEntry.Info.Id);
            loadLanguageFile();
            loadReplaceBox();
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("翻译插件注入完毕");
            return true;
        }

        public static void loadReplaceBox()
        {
            translateReplace.Add("（", "( ");
            translateReplace.Add("）", " )");
            translateReplace.Add("，", ", ");
            translateReplace.Add("。", ". ");
            translateReplace.Add("「", " \" ");
            translateReplace.Add("」", " \" ");
            translateReplace.Add("：", " : ");
            translateReplace.Add("！", " ! ");
            translateReplace.Add("《", "< ");
            translateReplace.Add("》", " >");
            translateReplace.Add("；", "; ");
            translateReplace.Add("“", " \" ");
            translateReplace.Add("”", " \" ");
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            if (!enabled)
            {
                string obj = "{";
                foreach (string key in missBox.Keys)
                {
                    obj += string.Format("\"{0}\":\"{1}\",", key, missBox[key]);
                }
                obj += "}";
                string path = string.Format("{0}{1}", basePath, "miss.json");
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
            string path = string.Format("{0}{1}", basePath, "lang.json");
            if (!File.Exists(path))
            {
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
                        foreach (string key in translateReplace.Keys)
                        {
                            value = value.Replace(key, (string)translateReplace[key]);
                        }
                        translateBox.Add(item.Key, value);
                        if (missBox.Contains(item.Key)) missBox.Remove(item.Key);
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
            static void Postfix(string Term, string overrideLanguage, ref string __result)
            {
                if (!enabled) return;
                if (Term == "NotoSansCJKkr-Medium SDF" || __result == null) return;
                if (!translateBox.ContainsKey(Term) && !missBox.ContainsKey(Term))
                {
                    logger.Log(string.Format("发现新词条:{0}:{1}", Term, __result));
                    missBox.Add(Term, __result);
                }
                if (translateBox.ContainsKey(Term) &&
                    LocalizationManager.CurrentLanguage == "Chinese (Simplified)")
                {
                    __result = (string)translateBox[Term];
                }
            }
        }

        [HarmonyPatch(typeof(CharacterClassDetailsRepository))]
        [HarmonyPatch("ResolveCharacterClassInfo")]
        public static class Class_Patch
        {
            static void Postfix(FactionCountry faction, PlayerClass playerClass, ref CharacterClassInfo __result)
            {
                if (!enabled) return;
                string Term = "Force Class/" + playerClass.ToString();
                if (!translateBox.ContainsKey(Term) && !missBox.ContainsKey(Term))
                {
                    logger.Log(string.Format("发现新词条:{0}:{1}", Term, __result));
                    missBox.Add(Term, __result);
                }
                if (translateBox.ContainsKey(Term) &&
                    LocalizationManager.CurrentLanguage == "Chinese (Simplified)")
                {
                    __result.playerClassName = (string)translateBox[Term];
                }
            }
        }

        [HarmonyPatch(typeof(ClientAdminBroadcastMessageManager))]
        [HarmonyPatch("PrivateMessage")]
        public static class AdminBroadcastA_Patch
        {
            static void Prefix(ref string message)
            {
                if (!enabled) return;
                foreach (string key in translateReplace.Keys)
                {
                    message = message.Replace(key, (string)translateReplace[key]);
                }
            }
        }

        [HarmonyPatch(typeof(ClientAdminBroadcastMessageManager))]
        [HarmonyPatch("AdminMessage")]
        public static class AdminBroadcastB_Patch
        {
            static void Prefix(ref string message)
            {
                if (!enabled) return;
                foreach (string key in translateReplace.Keys)
                {
                    message = message.Replace(key, (string)translateReplace[key]);
                }
            }
        }
    }
}
