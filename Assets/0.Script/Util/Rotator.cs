using System;
using UnityEngine;

namespace Util
{
    public class Rotator : MonoBehaviour
    {
        public enum RotateAxis
        {
            X,
            Y,
            Z
        }
        
        public float rotationSpeed = 50f; // 회전 속도
        public RotateAxis rotateAxis;

        private void Update()
        {
            // 회전 변위 계산
            var rotationDelta = rotationSpeed * Time.deltaTime;

            transform.rotation *= rotateAxis switch
            {
                RotateAxis.X => Quaternion.Euler(rotationDelta, 0f, 0),
                RotateAxis.Y => Quaternion.Euler(0f, rotationDelta, 0),
                RotateAxis.Z => Quaternion.Euler(0f, 0f, rotationDelta),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}