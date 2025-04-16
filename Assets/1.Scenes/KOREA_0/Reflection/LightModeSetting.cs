using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
#if UNITY_EDITOR
    using UnityEditor;
#endif
*/

[ExecuteInEditMode]
public class LightModeSetting : MonoBehaviour
{
    [Header("Lighting ----------------------------------")]
    public LightMode lightModeSelect = LightMode.Directional;

    public enum LightMode { Directional = 0, Point = 1 }
    public float pointLightIntensityMult = 1f;
    public bool isUsingLightProbe = false;

    private int interval = 10;


 //#if UNITY_EDITOR
    void Update()
    {
        if (Time.frameCount % interval == 0)
        {
            if (lightModeSelect == LightMode.Directional)
            {
                Shader.EnableKeyword("_LIGHTMODE_DIRECTIONAL");
                Shader.DisableKeyword("_LIGHTMODE_POINT");
                //Debug.Log("Directional On?: " + Shader.IsKeywordEnabled("_LIGHTMODE_DIRECTIONAL"));
            }
            else if (lightModeSelect == LightMode.Point)
            {
                Shader.EnableKeyword("_LIGHTMODE_POINT");
                Shader.DisableKeyword("_LIGHTMODE_DIRECTIONAL");
                //Debug.Log("Directional On?: " + Shader.IsKeywordEnabled("_LIGHTMODE_DIRECTIONAL"));
            }

            if (isUsingLightProbe)
            {
                Shader.EnableKeyword("_LIGHTPROBE_ON");
            }
            else
            {
                Shader.DisableKeyword("_LIGHTPROBE_ON");
            }

            Shader.SetGlobalFloat("_PointLightIntensityMult", pointLightIntensityMult);
        }
    }
//#endif

}
