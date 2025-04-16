using Jjamcat.Util;
using TMPro;
using UnityEngine;

namespace Jamcat.Locomotion.Test
{
    public class HandTestUI : Singleton<HandTestUI>
    {
        private Locomotion _locomotion;
        
        [SerializeField] private TMP_Text leftHandText;
        [SerializeField] private TMP_Text rightHandText;
        [SerializeField] private TMP_Text leftHandTouchText;
        [SerializeField] private TMP_Text rightHandTouchText;
        
        public void Init(Locomotion locomotion)
        {
            _locomotion = locomotion;
        }

        private void FixedUpdate()
        {
            if (_locomotion == null) return;
            
            leftHandText.text = $"Left:{_locomotion.leftHandTransform.position.ToString()}";
            rightHandText.text = $"Right{_locomotion.rightHandTransform.position.ToString()}";
            
            leftHandTouchText.text = $"Left:{_locomotion.wasLeftHandTouching.ToString()}";
            rightHandTouchText.text = $"Right:{_locomotion.wasRightHandTouching.ToString()}";
        }
    }
}