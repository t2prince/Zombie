using UnityEngine;

namespace Script.Util
{
    public class UIRotator : MonoBehaviour
    {
        public float rotationSpeed = 50f; // 회전 속도

        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            // 회전 변위 계산
            var rotationDelta = rotationSpeed * Time.deltaTime;

            // UI 이미지 회전
            rectTransform.rotation *= Quaternion.Euler(0f, 0f, rotationDelta);
        }
    }
}