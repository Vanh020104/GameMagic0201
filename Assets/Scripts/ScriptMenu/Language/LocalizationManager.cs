using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLanguage(string languageCode)
    {
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            if (LocalizationSettings.AvailableLocales.Locales[i].Identifier.Code == languageCode)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[i];
                Debug.Log("Language set to: " + languageCode);
                return;
            }
        }
    }
}
