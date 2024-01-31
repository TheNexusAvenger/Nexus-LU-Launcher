﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Platform;
using Nexus.LU.Launcher.Gui.Component.Base;

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
        LanguageChanged += (language) => SetText(textObject, this.GetLocalizedString(localizationId));
        SetText(textObject, this.GetLocalizedString(localizationId));
    }
}