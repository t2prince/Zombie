using System.Collections;
using Jamcat.Ingame.Interface;
using UnityEngine;

namespace _0.Script.Ingame.Interface
{
    public class KeyboardKey : MonoBehaviour
    {
        private string text = "";
        private Monitor _monitor;
        private const float originalHeight = 0.03885181f;
        private const float pressHeight = 0.01f;
        private const float animationDuration = 0.1f;

        private Vector3 originalPosition;
        private Coroutine currentCoroutine;

        private void Awake()
        {
            var textMesh = GetComponentInChildren<TextMesh>();
            text = textMesh.text;
            _monitor = GetComponentInParent<Monitor>();
            originalPosition = transform.localPosition;
        }

        private void OnKeyPress()
        {
            _monitor.AddText(text);
            AnimateKeyPress(originalPosition.y - (originalHeight - pressHeight));
        }

        private void KeyRelease()
        {
            AnimateKeyPress(originalPosition.y);
        }

        private void AnimateKeyPress(float targetY)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(AnimateYPosition(targetY));
        }

        private IEnumerator AnimateYPosition(float targetY)
        {
            var startPos = transform.localPosition;
            var targetPos = new Vector3(startPos.x, targetY, startPos.z);
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / animationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = targetPos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnKeyPress();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                KeyRelease();
            }
        }
    }
}