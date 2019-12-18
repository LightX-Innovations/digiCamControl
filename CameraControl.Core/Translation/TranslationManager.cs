#region Licence

// Distributed under MIT License
// ===========================================================
// 
// digiCamControl - DSLR camera remote control open source software
// Copyright (C) 2014 Duka Istvan
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY,FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using CameraControl.Devices;
using CameraControl.Devices.Classes;

#endregion

namespace CameraControl.Core.Translation
{
    public static class TranslationManager
    {
        private static Dictionary<string, string> _translations;
        private static readonly string _path = string.Empty;
        private static Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();

        private static AsyncObservableCollection<TranslationLangDesc> _availableLangs;

        public static AsyncObservableCollection<TranslationLangDesc> AvailableLangs
        {
            get
            {
                if (_availableLangs == null)
                    _availableLangs = new AsyncObservableCollection<TranslationLangDesc>();
                return _availableLangs;
            }
            set { _availableLangs = value; }
        }

        static TranslationManager()
        {
            AvailableLangs = new AsyncObservableCollection<TranslationLangDesc>();
            try
            {
                _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string[] files = Directory.GetFiles(Path.Combine(_path, "Languages"));
                foreach (string file in files)
                {
                    try
                    {
                        CultureInfo cult =
                            CultureInfo.GetCultureInfo(Path.GetFileNameWithoutExtension(file).Replace('_', '-'));
                        AvailableLangs.Add(new TranslationLangDesc()
                                               {
                                                   Value = Path.GetFileNameWithoutExtension(file),
                                                   Name = cult.EnglishName + " - " + cult.NativeName
                                               });
                    }
                    catch (Exception exception)
                    {
                        Log.Error("Error loading language", exception);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets the translated strings collection in the active language
        /// </summary>
        public static Dictionary<string, string> Strings
        {
            get
            {
                if (_translations == null)
                {
                    _translations = new Dictionary<string, string>();
                    Type transType = typeof (TranslationStrings);
                    FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    foreach (FieldInfo field in fields)
                    {
                        _translations.Add(field.Name, field.GetValue(transType).ToString());
                    }
                }
                return _translations;
            }
        }


        public static int LoadLanguage(string lang_code)
        {
            XmlDocument doc = new XmlDocument();
            TranslatedStrings = new Dictionary<string, string>();
            string langPath = "";
            try
            {
                langPath = Path.Combine(_path, "Languages", lang_code.Replace('-', '_') + ".xml");
                doc.Load(langPath);
            }
            catch (Exception)
            {
                if (lang_code == "en-US")
                    return 0; // otherwise we are in an endless loop!
                return LoadLanguage("en-US");
            }
            foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
            {
                if (stringEntry.NodeType == XmlNodeType.Element)
                    try
                    {
                        TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("name").Value, stringEntry.InnerText);
                    }
                    catch (Exception)
                    {
                        //Log.Error("Error in Translation Engine");
                        //Log.Error(ex);
                    }
            }

            Type TransType = typeof (TranslationStrings);
            FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fieldInfos)
            {
                if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
                    TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType,
                                           new object[] {TranslatedStrings[fi.Name]});
                //else
                //  Log.Info("Translation not found for field: {0}.  Using hard-coded English default.", fi.Name);
            }
            return TranslatedStrings.Count;
        }

        public static string GetTranslation(string key)
        {
            if (Strings.ContainsKey(key))
                return Strings[key];
            if (TranslatedStrings.ContainsKey(key))
                return TranslatedStrings[key];
            return key;
        }

        public static bool HaveTranslation(string key)
        {
            return Strings.ContainsKey(key) || TranslatedStrings.ContainsKey(key);
        }
    }
}