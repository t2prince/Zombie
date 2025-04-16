using UnityEngine;

namespace Util
{
    internal static class Camera
    {
        public static Vector2 WorldToCanvasPosition (RectTransform canvas, UnityEngine.Camera camera, Vector3 position)
        {
            //Vector position (percentage from 0 to 1) considering camera size.
            //For example (0,0) is lower left, middle is (0.5,0.5)
            Vector2 temp = camera.WorldToViewportPoint (position);

            //Calculate position considering our percentage, using our canvas size
            //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
            temp.x *= canvas.sizeDelta.x;
            temp.y *= canvas.sizeDelta.y;

            //The result is ready, but, this result is correct if canvas recttransform pivot is 0,0 - left lower corner.
            //But in reality its middle (0.5,0.5) by default, so we remove the amount considering cavnas rectransform pivot.
            //We could mutiply with constant 0.5, but we will actually read the value, so if custom rect transform is passed(with custom pivot) , 
            //returned value will still be correct.

            temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
            temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

            return temp;
        }
    }
}