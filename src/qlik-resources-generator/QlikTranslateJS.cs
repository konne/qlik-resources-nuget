namespace Qlik.Resources
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Linq;
    using System.IO;
    using System.Resources;
    using Newtonsoft.Json;
    #endregion

    public class QlikTranslateJS
    {
        public static List<CultureInfo> GenerateQlikTranslateJS(string path, string outpath)
        {
            var cultList = new List<CultureInfo>();

            var cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .Select(c => c.IetfLanguageTag)
                    .ToList();

            var TranslationsFiles = new Dictionary<string, Dictionary<CultureInfo, SortedDictionary<string, string>>>();

            #region Parse Json Files
            foreach (var item in Directory.EnumerateDirectories(path))
            {
                string lastFolderName = Path.GetFileName(item);

                CultureInfo ci = CultureInfo.InvariantCulture;
                try
                {
                    ci = CultureInfo.GetCultureInfo(lastFolderName);

                    if (ci.Parent != null && lastFolderName.Substring(0, 2) != "zh")
                        ci = ci.Parent;
                }
                catch (Exception ex)
                {
                    continue;
                }
                if (!cultureInfos.Contains(ci.IetfLanguageTag))
                    continue;

                var langueExt = "";
                if (!String.IsNullOrWhiteSpace(ci.IetfLanguageTag))
                    langueExt = "." + ci.IetfLanguageTag;

                if (!cultList.Contains(ci))
                    cultList.Add(ci);

                foreach (var jsonFile in Directory.EnumerateFiles(item + @"\"))
                {
                    var jsonFileName = Path.GetFileNameWithoutExtension(jsonFile).ToLowerInvariant();
                    var json = File.ReadAllText(jsonFile);
                    // remove the define( from the .js file
                    json = json.Substring(7);
                    var jtr = new JsonTextReader(new StringReader(json));
                    json = json.TrimEnd(new char[] { ')', ';' });

                    if (!TranslationsFiles.ContainsKey(jsonFileName))
                        TranslationsFiles.Add(jsonFileName, new Dictionary<CultureInfo, SortedDictionary<string, string>>());

                    var Translations = TranslationsFiles[jsonFileName];

                    if (!Translations.ContainsKey(ci) && !string.IsNullOrWhiteSpace(ci.Name))
                        Translations.Add(ci, new SortedDictionary<string, string>());

                    var KeyValues = Translations[ci];

                    try
                    {
                        string propName = "";
                        while (jtr.Read() && jtr.TokenType != JsonToken.EndObject)
                        {
                            if (jtr.Value != null && jtr.TokenType == JsonToken.PropertyName)
                            {
                                propName = jtr.Value.ToString();
                                // replace the . with _ because this makes the handling in .NET much easier
                                propName = propName.Replace(".", "_");
                                propName = propName.Replace("-", "_");
                                propName = propName.Replace(" ", "_");
                            }

                            if (jtr.Value != null && jtr.TokenType == JsonToken.String)
                            {
                                KeyValues.Add(propName, jtr.Value.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            #endregion

            #region Add Missing Invariant Culture if necessary
            foreach (var kvFile in TranslationsFiles)
            {
                if (!kvFile.Value.ContainsKey(CultureInfo.InvariantCulture))
                {
                    var listKeys = new SortedDictionary<string, string>();
                    foreach (var kvCults in kvFile.Value)
                    {
                        foreach (var key in kvCults.Value.Keys)
                            if (!listKeys.ContainsKey(key))
                                listKeys.Add(key, key);
                    }

                    kvFile.Value.Add(CultureInfo.InvariantCulture, listKeys);
                }
            }
            #endregion

            foreach (var kvFile in TranslationsFiles)
            {
                var jsonFileName = "Translate_" + kvFile.Key;
                foreach (var kvCult in kvFile.Value)
                {
                    var langueExt = "";
                    if (!String.IsNullOrWhiteSpace(kvCult.Key.IetfLanguageTag))
                        langueExt = "." + kvCult.Key.IetfLanguageTag;
                    else
                    {
                        var csFileName = outpath + "\\" + jsonFileName + ".Designer.cs";
                        using (var rw = new CompilerGenerated(csFileName, jsonFileName))
                        {
                            foreach (var kvTranslate in kvCult.Value)
                            {
                                rw.AddResource(kvTranslate.Key);
                            }
                        }

                    }

                    var resxFileName = outpath + "\\" + jsonFileName + langueExt + ".resx";

                    using (var rw = new ResXResourceWriter(resxFileName))
                    {
                        foreach (var kvTranslate in kvCult.Value)
                        {
                            rw.AddResource(kvTranslate.Key, kvTranslate.Value);
                        }
                    }
                }
            }
            return cultList;
        }
    }
}
