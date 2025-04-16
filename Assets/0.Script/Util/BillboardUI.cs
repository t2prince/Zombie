using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform m_camTrans = null;
    public Canvas m_canvas = null;
    private static Camera mainCamera;
    private void OnEnable()
    {
        if(mainCamera == null)
            mainCamera = Camera.main;

        if (!mainCamera.gameObject.activeSelf) return;
        
        m_camTrans = mainCamera.transform;
        m_canvas.worldCamera = mainCamera;
    }
    private void LateUpdate()
    {
        transform.LookAt(transform.position + m_camTrans.rotation * Vector3.forward, m_camTrans.rotation * Vector3.up);
    }
}
