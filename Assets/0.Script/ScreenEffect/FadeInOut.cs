using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Jamcat.Effect.ScreenEffect
{
    public class FadeInOut : MonoBehaviour
    {
        public Image fadeImage;
        public float fadeDuration = 1.0f;

        private void Start()
        {
            if (fadeImage == null)
            {
                Debug.LogError("Fade Image is not assigned.");
                return;
            }
        }

        public void FadeIn()
        {
            StartCoroutine(Fade(1, 0));
        }

        public void FadeOut()
        {
            StartCoroutine(Fade(0, 1));
        }

        private IEnumerator Fade(float startAlpha, float endAlpha)
        {
            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            color.a = endAlpha;
            fadeImage.color = color;
        }
    }
}