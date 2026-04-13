using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_Fade : MonoBehaviour
{
    public static UI_Fade Instance;

    public Image fadeImage;
    public float fadeSpeed = 2f;

    void Awake()
    {
        // Instance = this;
        // If an instance doesn't exist, make this the instance and don't kill it.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            // If another one exists,
            // destroy this one so they don't fight.
            Destroy(gameObject);
        }
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1));
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0));
    }

    IEnumerator Fade(float targetAlpha)
    {
        Color color = fadeImage.color;

        while (!Mathf.Approximately(color.a, targetAlpha))
        {
            color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            fadeImage.color = color;
            yield return null;
        }
    }
}