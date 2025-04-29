using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LanguageSelector : MonoBehaviour
{
    [System.Serializable]
    public class LanguageFlag
    {
        public string displayCode;
        public string localeCode;
        public Sprite flagIcon;
    }

    public GameObject flagButtonPrefab;
    public Transform contentParent;
    public List<LanguageFlag> availableLanguages = new List<LanguageFlag>();

    private const string PLAYER_PREFS_KEY = "SelectedLanguage";
    private GameObject selectedButton = null;
    private readonly Vector3 selectedScale = new Vector3(1.2f, 1.3f, 1f);
    private readonly Vector3 defaultScale = Vector3.one;

    // Load saved language and generate language buttons
    private void Start()
    {
        string savedLangCode = PlayerPrefs.GetString(PLAYER_PREFS_KEY, "");
        if (!string.IsNullOrEmpty(savedLangCode))
        {
            StartCoroutine(SetLocale(savedLangCode));
        }

        foreach (var lang in availableLanguages)
        {
            GameObject flagButtonObj = Instantiate(flagButtonPrefab, contentParent);

            var icon = flagButtonObj.transform.Find("Icon").GetComponent<Image>();
            if (icon != null)
                icon.sprite = lang.flagIcon;

            var label = flagButtonObj.transform.Find("Label").GetComponent<TextMeshProUGUI>();
            if (label != null)
                label.text = lang.displayCode;

            string localeCode = lang.localeCode;
            flagButtonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                PlayerPrefs.SetString(PLAYER_PREFS_KEY, localeCode);
                PlayerPrefs.Save();
                StartCoroutine(SetLocale(localeCode));

                if (selectedButton != null)
                    selectedButton.transform.localScale = defaultScale;

                flagButtonObj.transform.localScale = selectedScale;
                selectedButton = flagButtonObj;
            });
        }

        if (!string.IsNullOrEmpty(savedLangCode))
            StartCoroutine(SelectSavedButton(savedLangCode));

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    // Apply locale change by code
    private IEnumerator SetLocale(string code)
    {
        yield return LocalizationSettings.InitializationOperation;

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == code)
            {
                LocalizationSettings.SelectedLocale = locale;
                Debug.Log($"✅ Language changed to: {code}");
                yield break;
            }
        }

        Debug.LogWarning($"⚠️ Locale code '{code}' not found.");
    }

    // Scale the correct flag button based on saved locale
    private IEnumerator SelectSavedButton(string savedLocaleCode)
    {
        yield return null;

        foreach (Transform child in contentParent)
        {
            var label = child.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                string code = GetLocaleCodeFromDisplayCode(label.text);
                if (code == savedLocaleCode)
                {
                    child.transform.localScale = selectedScale;
                    selectedButton = child.gameObject;
                    yield break;
                }
            }
        }
    }

    // Get localeCode from displayCode
    private string GetLocaleCodeFromDisplayCode(string displayCode)
    {
        foreach (var lang in availableLanguages)
        {
            if (lang.displayCode == displayCode)
                return lang.localeCode;
        }
        return "";
    }

    // Handle locale changed event
    private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        Debug.Log($"🌍 Locale changed to: {newLocale.Identifier.Code}");
    }

    // Cleanup
    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
}
