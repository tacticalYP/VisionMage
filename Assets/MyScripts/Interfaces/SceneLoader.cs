using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    void Awake()
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

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(sceneName);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeOut()
    {
        UI_Fade.Instance.FadeOut();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator FadeIn()
    {
        UI_Fade.Instance.FadeIn();
        yield return new WaitForSeconds(1f);
    }
}