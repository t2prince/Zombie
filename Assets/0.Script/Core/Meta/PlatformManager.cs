namespace Jamcat.Core.Meta
{
    using Oculus.Platform;
    using Oculus.Platform.Models;
    using UnityEngine;

/// <summary>
/// Platform SDK 초기화 및 에러 처리, 권한 확인(Entitlement Check)을 담당하는 유틸리티 클래스
/// </summary>
    public static class PlatformManager
    {
        private static bool isInitialized = false;

        /// <summary>
        /// Meta Platform SDK 초기화 및 권한 체크 시작
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
                return;

            try
            {
                Core.Initialize(); // 앱 ID는 AndroidManifest에 설정되어 있어야 함
                Entitlements.IsUserEntitledToApplication().OnComplete(OnEntitlementCheckComplete);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Platform initialization failed: " + e.Message);
    #if !UNITY_EDITOR
                Application.Quit();
    #endif
            }

            isInitialized = true;
        }

        /// <summary>
        /// 권한 체크 콜백 처리
        /// </summary>
        private static void OnEntitlementCheckComplete(Message msg)
        {
            if (msg.IsError)
            {
                Debug.LogError("User is NOT entitled to use this app.");
                var error = msg.GetError();
                Debug.LogErrorFormat("Error {0}: {1}", error.Code, error.Message);
    #if !UNITY_EDITOR
                Application.Quit();
    #endif
            }
            else
            {
                Debug.Log("User is entitled to use this app.");
            }
        }

        /// <summary>
        /// Generic 에러 처리 후 종료
        /// </summary>
        public static void TerminateWithError(Message msg)
        {
            if (msg.IsError)
            {
                Debug.LogError("Platform SDK Error: " + msg.GetError().Message);
    #if !UNITY_EDITOR
                Application.Quit();
    #endif
            }
        }

        /// <summary>
        /// Generic 에러 처리 후 종료 (제네릭 버전)
        /// </summary>
        public static void TerminateWithError<T>(Message<T> msg)
        {
            if (msg.IsError)
            {
                Debug.LogError("Platform SDK Error: " + msg.GetError().Message);
    #if !UNITY_EDITOR
                Application.Quit();
    #endif
            }
        }
    }
}