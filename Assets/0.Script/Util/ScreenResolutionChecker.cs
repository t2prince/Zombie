using UnityEngine;

namespace _Scripts.Util
{
    public static class ScreenScale
    {
        private const float MAX_RESOLUTION_RATIO = 2436f / 1125f;
        private const float DEFAULT_RESOLUTION_RATIO = 1136f / 640f;
		
        public static float GetCameraSize(float minSize, float maxSize)
        {	
            const float dx = MAX_RESOLUTION_RATIO - DEFAULT_RESOLUTION_RATIO;
            var dy = maxSize - minSize;
            var gradient = dy / dx;
			
            var currentRatio = Screen.height / (float)Screen.width;
            return currentRatio >= DEFAULT_RESOLUTION_RATIO ? minSize + gradient * (currentRatio - DEFAULT_RESOLUTION_RATIO) : minSize;
        }
    }
}