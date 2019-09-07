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
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace TranslateCN
{
    static public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger logger;
        public static Hashtable translateBox = new Hashtable();
        public static Hashtable translateReplace = new Hashtable();
        public static string basePath;
        public static bool english = false;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            basePath = modEntry.Path;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            logger = modEntry.Logger;
            logger.Log("翻译插件开始加载 patch id:" + modEntry.Info.Id);
            loadLanguageFile();
            loadReplaceBox();
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("翻译插件注入完毕");
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                if (GUILayout.Button("导出所有文本"))
                {
                    exportLangugeFile();
                }

                if (GUILayout.Button("重载翻译文本"))
                {
                    loadLanguageFile();
                }

                if (GUILayout.Button("云端更新翻译"))
                {
                    updateLanguageFile();
                }
            } catch (Exception e)
            {
                logger.Log(e.Message);
                logger.Log(e.StackTrace);
                throw e;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受   
        }

        private static void updateLanguageFile()
        {
            List<string> categories = LocalizationManager.GetCategories();
            string obj = "{";
            translateBox.Clear();
            bool needContinue = true;
            int index = 1;
            while (needContinue)
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                string url = "https://weblate.chaotic-quantum.site/api/translations/holdfast/plugin/zh_Hans/units/?page=" + index.ToString();
                logger.Log(url);
                WebRequest w1 = WebRequest.Create(url);
                w1.Headers.Add("Authorization", "Token 2wkfYAiXTL36SLELtYJdVWRnhsL8cKlwab0i92dA");
                WebResponse w2 = w1.GetResponse();
                Stream s1 = w2.GetResponseStream();
                StreamReader sr = new StreamReader(s1);
                string s2 = sr.ReadToEnd();
                JToken o = JToken.Parse(s2);
                JArray result = (JArray)o["results"];
                foreach (JToken key in result)
                {
                    if (translateBox.ContainsKey(key["context"])) continue;
                    translateBox.Add(key["context"].ToString().Substring(1), key["target"]);
                    obj += string.Format("\"{0}\":\"{1}\",", key["context"].ToString().Substring(1), key["target"]);
                }
                if (o["next"].ToString().Trim().Length == 0) needContinue = false;
                else index++;
            }
            obj = obj.Substring(0, obj.Length - 1);
            obj += "}";
            string path = string.Format("{0}{1}", basePath, "lang.json");
            File.WriteAllText(path, obj.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n"));
        }

        private static void exportLangugeFile()
        {
            List<string> categories = LocalizationManager.GetCategories();
            string obj = "{";
            categories.ForEach((string item) =>
            {
                logger.Log(item + ":");
                LocalizationManager.GetTermsList(item).ForEach((string term) =>
                {
                    obj += string.Format("\"{0}\":\"{1}\",", term,
                        (translateBox.ContainsKey(term) ?
                        translateBox[term] :
                        LocalizationManager.GetTranslation(term, true, 0, true, false, null, null)));
                });
            });
            obj += "}";
            string path = string.Format("{0}{1}", basePath, "dict.json");
            File.WriteAllText(path, obj.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n"));
        }

        private static void loadReplaceBox()
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
                if (translateBox.ContainsKey(Term) &&
                    LocalizationManager.CurrentLanguage == "Chinese (Simplified)")
                {
                    __result = (string)translateBox[Term];
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
