using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageBootstrap : MonoBehaviour
{
    private const string PLAYER_PREFS_KEY = "SelectedLanguage";

    private void Awake()
    {
        string savedLang = PlayerPrefs.GetString(PLAYER_PREFS_KEY, "");
        if (!string.IsNullOrEmpty(savedLang))
        {
            StartCoroutine(ApplySavedLocale(savedLang));
        }
    }

    private IEnumerator ApplySavedLocale(string code)
    {
        yield return LocalizationSettings.InitializationOperation;

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == code)
            {
                LocalizationSettings.SelectedLocale = locale;
                Debug.Log($"🌍 Loaded saved language: {code}");
                yield break;
            }
        }

        Debug.LogWarning($"⚠️ Saved locale '{code}' not found.");
    }
}
