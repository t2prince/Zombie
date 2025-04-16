using System;
using System.Collections.Generic;
using System.Linq;
using Fusion.XR.Shared.Grabbing;
using Fusion.XR.Shared.Rig;
using TMPro;
using UnityEngine;

namespace Jamcat.Locomotion.Test
{
    public class GrabTestUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text isGrab;
        [SerializeField] private TMP_Text grabbed;

        private List<Grabber> grabbers;
        private List<HardwareHand> hands;

        private void Start()
        {
            grabbers = FindObjectsOfType<Grabber>().ToList();
            hands = FindObjectsOfType<HardwareHand>().ToList();
        }

        private void Update()
        {
            var grabbedName = grabbers
                .Where(g => g.grabbedObject != null)
                .Select(g => g.grabbedObject.name)
                .FirstOrDefault();

            grabbed.text = grabbedName == null 
                ? "null" 
                : $"Grabbed:{grabbedName}";
            
            isGrab.text = hands.Any(h => h.isGrabbing) ? "isGrab:true" :  "isGrab:false";  
        }
    }
}