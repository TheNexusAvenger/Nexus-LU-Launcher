using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Nexus.LU.Launcher.Gui.Util;

public class Localization
{
    /// <summary>
    /// Current language to localize for.
    /// </summary>
    public string CurrentLanguage { get; private set; } = "en_US";

    /// <summary>
    /// Event for the selected language changing.
    /// </summary>
    public event Action<string>? LanguageChanged;
    
    /// <summary>
    /// Localized strings for the launcher.
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, string>> localizedStrings = new Dictionary<string, Dictionary<string, string>>();
    
    /// <summary>
    /// Static instance of Localization.
    /// </summary>
    private static Localization? _localization;
    
    /// <summary>
    /// Creates a localization state.
    /// </summary>
    private Localization()
    {
        // Read the XML locale file..
        var localeXmlStream = AssetLoader.Open(new Uri($"avares://{Assembly.GetEntryAssembly()?.GetName().Name}/Assets/Locale.xml"));
        var localeXml = new StreamReader(localeXmlStream).ReadToEnd();
        
        // Load the XML.
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(localeXml);
        foreach (XmlNode phraseXml in xmlDocument.SelectNodes("//phrases/phrase")!)
        {
            var localizationKey = phraseXml.Attributes!["id"]!.Value;
            var entry = new Dictionary<string, string>();
            foreach (XmlNode translationXml in phraseXml.ChildNodes)
            {
                if (translationXml?.Attributes == null) continue;
                entry[translationXml.Attributes["locale"]!.Value] = translationXml.InnerText;
            }
            this.localizedStrings[localizationKey] = entry;
        }
    }

    /// <summary>
    /// Returns a static instance of Localization.
    /// </summary>
    /// <returns>Static instance of Localization.</returns>
    public static Localization Get()
    {
        return _localization ??= new Localization();
    }

    /// <summary>
    /// Returns the localization string for a key.
    /// </summary>
    /// <param name="key">Key to get the localized string of.</param>
    /// <returns>The localized string, or the original if it is unsupported.</returns>
    public string GetLocalizedString(string key)
    {
        if (!localizedStrings.TryGetValue(key, out var keyLocalization))
        {
            return key;
        }
        return !keyLocalization.ContainsKey(this.CurrentLanguage) ? key : keyLocalization[this.CurrentLanguage];
    }

    /// <summary>
    /// Localizes a text object.
    /// </summary>
    /// <param name="textObject">Text object to localize.</param>
    public void LocalizeText(Control textObject)
    {
        // Get the localization id.
        var textProperty = textObject.GetType()?.GetProperty("Text");
        if (textProperty == null) return;
        var localizationId = (string) textProperty.GetValue(textObject)!;
        
        // Set the text and connect the language changing.
        LanguageChanged += (language) => textProperty.SetValue(textObject, this.GetLocalizedString(localizationId));
        textProperty.SetValue(textObject, this.GetLocalizedString(localizationId));
    }
}