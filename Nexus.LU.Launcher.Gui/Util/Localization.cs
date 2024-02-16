using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Threading;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.Gui.Util;

public class Localization
{
    /// <summary>
    /// Current language to localize for.
    /// </summary>
    public string CurrentLanguage { get; private set; }

    /// <summary>
    /// Languages that can be used.
    /// </summary>
    public readonly List<string> Languages = new List<string>();

    /// <summary>
    /// Event for the selected language changing.
    /// </summary>
    public event Action<string>? LanguageChanged;
    
    /// <summary>
    /// Localized strings for the launcher.
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, string>> localizedStrings = new Dictionary<string, Dictionary<string, string>>();
    
    /// <summary>
    /// Localized sizes for the launcher.
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, int>> localizedSizes = new Dictionary<string, Dictionary<string, int>>();
    
    /// <summary>
    /// Static instance of Localization.
    /// </summary>
    private static Localization? _localization;
    
    /// <summary>
    /// Creates a localization state.
    /// </summary>
    private Localization()
    {
        // Read the XML locale file.
        var localeXmlStream = AssetLoader.Open(new Uri($"avares://{Assembly.GetEntryAssembly()?.GetName().Name}/Assets/Locale.xml"));
        var localeXml = new StreamReader(localeXmlStream).ReadToEnd();
        
        // Add the language options.
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(localeXml);
        foreach (XmlNode localeEntryXml in xmlDocument.SelectNodes("//locales/locale")!)
        {
            this.Languages.Add(localeEntryXml.InnerText);
        }
        
        // Load the XML sizes.
        foreach (XmlNode sizeXml in xmlDocument.SelectNodes("//sizes/size")!)
        {
            // Load the sizes.
            var localizationKey = sizeXml.Attributes!["id"]!.Value;
            var entry = new Dictionary<string, int>();
            foreach (XmlNode translationXml in sizeXml.ChildNodes)
            {
                if (translationXml?.Attributes == null) continue;
                entry[translationXml.Attributes["locale"]!.Value] = int.Parse(translationXml.InnerText);
            }
            
            // Add the defaults.
            var defaultSize = int.Parse(sizeXml.Attributes!["default"]!.Value);
            foreach (var language in this.Languages.Where(language => !entry.ContainsKey(language)))
            {
                entry[language] = defaultSize;
            }

            // Store the sizes.
            this.localizedSizes[localizationKey] = entry;
        }
        
        // Load the XML translations.
        foreach (XmlNode phraseXml in xmlDocument.SelectNodes("//phrases/phrase")!)
        {
            // Load the translations.
            var localizationKey = phraseXml.Attributes!["id"]!.Value;
            var entry = new Dictionary<string, string>();
            foreach (XmlNode translationXml in phraseXml.ChildNodes)
            {
                if (translationXml?.Attributes == null) continue;
                entry[translationXml.Attributes["locale"]!.Value] = translationXml.InnerText;
            }
            
            // Try to match missing translations.
            foreach (var language in this.Languages)
            {
                if (entry.ContainsKey(language)) continue;
                var baseLanguage = language.Substring(0, language.IndexOf('_') + 1);
                foreach (var otherLanguage in this.Languages)
                {
                    if (!otherLanguage.StartsWith(baseLanguage)) continue;
                    if (!entry.ContainsKey(otherLanguage)) continue;
                    entry[language] = entry[otherLanguage];
                    break;
                }
            }
            
            // Store the translations.
            this.localizedStrings[localizationKey] = entry;
        }
        
        // Set the current language.
        this.CurrentLanguage = SystemInfo.GetDefault().Settings.Locale;
        if (!this.Languages.Contains(this.CurrentLanguage))
        {
            this.CurrentLanguage = this.Languages[0];
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
    /// Returns the text for an object.
    /// This is done manually without reflection in order to allow building with .NET AOT.
    /// </summary>
    /// <param name="textObject">Text object to get the text of.</param>
    /// <returns>Text of the object to return.</returns>
    private static string? GetText(object textObject)
    {
        return textObject switch
        {
            TextBlock textBlock => textBlock.Text,
            ImageTextButton imageTextButton => imageTextButton.Text,
            _ => throw new InvalidOperationException($"GetText does not support {textObject.GetType().Name}")
        };
    }

    /// <summary>
    /// Set the text for an object.
    /// This is done manually without reflection in order to allow building with .NET AOT.
    /// </summary>
    /// <param name="textObject">Text object to set the text of.</param>
    /// <param name="text">Text to set for the object.</param>
    private static void SetText(object textObject, string text)
    {
        switch (textObject)
        {
            case TextBlock textBlock:
                textBlock.Text = text;
                return;
            case ImageTextButton imageTextButton:
                imageTextButton.Text = text;
                return;
        }
        throw new InvalidOperationException($"SetText does not support {textObject.GetType().Name}");
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
    public void LocalizeText(object textObject)
    {
        // Get the localization id.
        var localizationId = GetText(textObject)!;
        
        // Set the text and connect the language changing.
        LanguageChanged += (language) =>
        {
            Dispatcher.UIThread.InvokeAsync(() => SetText(textObject, this.GetLocalizedString(localizationId)));
        };
        SetText(textObject, this.GetLocalizedString(localizationId));
    }

    /// <summary>
    /// Returns the localization size for a key.
    /// </summary>
    /// <param name="key">Key to get the localized string of.</param>
    /// <returns>The localized size.</returns>
    public int GetLocalizedSize(string key)
    {
        return this.localizedSizes[key][this.CurrentLanguage];
    }

    /// <summary>
    /// Localizes a width for an object.
    /// </summary>
    /// <param name="widthObject">Object to localize.</param>
    /// <param name="localizationId">Id of the size to use.</param>
    public void LocalizeWidth(Layoutable widthObject, string localizationId)
    {
        LanguageChanged += (language) =>
        {
            Dispatcher.UIThread.InvokeAsync(() => widthObject.Width = this.GetLocalizedSize(localizationId));
        };
        widthObject.Width = this.GetLocalizedSize(localizationId);
    }

    /// <summary>
    /// Sets the new current language.
    /// </summary>
    /// <param name="language">Language to change to.</param>
    public void SetCurrentLanguage(string language)
    {
        // Set the language.
        this.CurrentLanguage = language;
        this.LanguageChanged?.Invoke(language);
        
        // Store the language.
        var systemInfo = SystemInfo.GetDefault();
        systemInfo.Settings.Locale = language;
        systemInfo.SaveSettings();
    }
}