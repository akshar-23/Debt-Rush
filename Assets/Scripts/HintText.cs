using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintText : MonoBehaviour
{
    public TextMeshProUGUI hintText;
    public Image hintBackground;
    public float fadeDuration = 0.4f;

    Coroutine fadeRoutine;

    private void Awake()
    {
        // Start fully invisible
        if (hintText != null)
        {
            Color c = hintText.color;
            c.a = 0f;
            hintText.color = c;
        }

        if (hintBackground != null)
        {
            Color c = hintBackground.color;
            c.a = 0f;
            hintBackground.color = c;
        }
    }

    public void ShowHint(string message)
    {
        hintText.text = message;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Fade(1f));
    }

    public void HideHint()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Fade(0f));
    }

    IEnumerator Fade(float target)
    {
        float startAlpha = hintText.color.a;
        float startAlphaImage = hintBackground.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            Color c = hintText.color;
            c.a = Mathf.Lerp(startAlpha, target, t);
            hintText.color = c;

            Color imageC = hintBackground.color;
            imageC.a = Mathf.Lerp(startAlphaImage, target / 2f, t);
            hintBackground.color = imageC;

            yield return null;
        }
    }
}
