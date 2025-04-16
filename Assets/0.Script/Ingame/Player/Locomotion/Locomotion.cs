
using Jamcat.Locomotion.Test;
using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Jamcat.Locomotion
{
    public class Locomotion : MonoBehaviour
    {
        //좌,우 손의 Transform
        //컨트롤러의 움직임을 따라 움직이게 하기 위해
        //로코모션을 정의할 때 가장 중요한 부분
        public Transform rightHandTransform;
        public Transform leftHandTransform;

        //좌우, 물리 환경 안에서 움직여야 만 하는 오브젝트
        //컨트롤러의 움직임에 의해 땅속에 박히거나, 박혀있거나 하는 물리 법칙 외의 상태를 방지하기 위해
        public Transform leftHandFollower;
        public Transform rightHandFollower;
        
        //좌우 손의 물리적 충돌을 감지하기 위한 변수
        //고릴라택의 로코모션이 기준이 되다보니 필요한 변수지만,
        //이후에 다른 로코모션을 만들 때는 필요 없을 수도 있음
        public bool wasLeftHandTouching;
        public bool wasRightHandTouching;
        
        public void Init(NetworkRig rig)
        {
            leftHandFollower = rig.leftHand.transform;
            rightHandFollower = rig.rightHand.transform;

            var hardwareRig = FindAnyObjectByType<HardwareRig>();
            leftHandTransform = hardwareRig.leftHand.transform;
            rightHandTransform = hardwareRig.rightHand.transform;
            
            InitializeValues();
        }
        
        protected virtual void InitializeValues()
        {
            leftHandTransform.localPosition = Vector3.zero;
            rightHandTransform.localPosition = Vector3.zero;
            
            leftHandTransform.localRotation = Quaternion.identity;
            rightHandTransform.localRotation = Quaternion.identity;
        }
    }
}