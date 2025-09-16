using Fusion;
using UnityEngine;

namespace Jamcat.Ingame.Player
{
    public class NetworkRigCameraFollower : NetworkBehaviour
    {
        private Transform _cameraTransform;
        private bool _isFollowing = false;
        
        public void Init(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
            _isFollowing = true;
            
            Debug.Log($"[NetworkRigCameraFollower] Initialized to follow camera");
        }
        
        public override void FixedUpdateNetwork()
        {
            // InputAuthority가 있는 클라이언트에서만 NetworkRig 위치 업데이트
            if (!Object.HasInputAuthority || !_isFollowing || _cameraTransform == null)
                return;
                
            // NetworkRig의 위치와 회전을 카메라에 맞춤
            transform.position = _cameraTransform.position;
            transform.rotation = _cameraTransform.rotation;
        }
        
        public void StopFollowing()
        {
            _isFollowing = false;
        }
        
        public void StartFollowing()
        {
            _isFollowing = true;
        }
    }
}