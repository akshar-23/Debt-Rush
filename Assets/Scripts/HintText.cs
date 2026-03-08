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

    public void ShowHint(string message)
    {
        hintText.text = message;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        //hintBackground.gameObject.SetActive(true);
        fadeRoutine = StartCoroutine(Fade(1f));
    }

    public void HideHint()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        //hintBackground.gameObject.SetActive(false);
        fadeRoutine = StartCoroutine(Fade(0f));
    }

    IEnumerator Fade(float target)
    {
        Color startColor = hintText.color;
        float startAlpha = startColor.a;

        Color startColorImage = hintBackground.color;
        float startAlphaImage = startColorImage.a;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float newAlpha = Mathf.Lerp(startAlpha, target, time / fadeDuration);
            float newAlphaImage = Mathf.Lerp(startAlphaImage, target/2, time / fadeDuration);

            Color c = hintText.color;
            c.a = newAlpha;
            hintText.color = c;

            Color imageC = hintBackground.color;
            imageC.a = newAlphaImage;
            hintBackground.color = imageC;

            yield return null;
        }
    }
}
