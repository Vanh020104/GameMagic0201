using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("UI")]
    public GameObject tapToPlayText;
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;

    [Header("Scene")]
    public string sceneToLoad = "Scenes_Home_Game";

    private bool isTapped = false;

    private void Start()
    {
        loadingSlider.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
        loadingSlider.value = 0f;
    }

    public void OnTapToPlayClicked()
    {
        if (isTapped) return;
        isTapped = true;
        tapToPlayText.SetActive(false);
        loadingSlider.gameObject.SetActive(true);
        loadingText.gameObject.SetActive(true);

        StartCoroutine(SimulateLoading());
    }


    IEnumerator SimulateLoading()
    {
        loadingSlider.value = 0f;
        float current = 0f;

        while (current < 1f)
        {
            float nextStep = Random.Range(0.05f, 0.15f);
            current += nextStep;

            current = Mathf.Clamp01(current);

            loadingSlider.value = current;
            loadingText.text = $"Loading... {(int)(current * 100)}%";

            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }

        loadingText.text = "Loading... 100%";
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(sceneToLoad);
    }

}
