using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Jamcat.Ingame.Player
{
    public class CustomNetworkRig : NetworkRig
    {
        // InputAuthority 기반으로 로컬 NetworkRig 판단
        public override bool IsLocalNetworkRig => Object && Object.InputAuthority == Runner.LocalPlayer;

        public override void Spawned()
        {
            base.Spawned();
            Debug.Log($"[CustomNetworkRig] Spawned - Name: {name}, IsLocalNetworkRig: {IsLocalNetworkRig}, InputAuthority: {Object.InputAuthority.PlayerId}, LocalPlayer: {Runner.LocalPlayer.PlayerId}");

            // InputAuthority 기반으로 HardwareRig 찾기
            if (IsLocalNetworkRig)
            {
                Debug.Log($"[CustomNetworkRig] {name} - Searching for HardwareRig as local player");
                hardwareRig = FindObjectOfType<HardwareRig>();
                if (hardwareRig == null)
                {
                    Debug.LogError($"[CustomNetworkRig] {name} - Missing HardwareRig in the scene");
                }
                else
                {
                    Debug.Log($"[CustomNetworkRig] {name} - Found HardwareRig: {hardwareRig.name}");
                }
            }
            else
            {
                Debug.Log($"[CustomNetworkRig] {name} - Remote NetworkRig, skipping HardwareRig setup");
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            // InputAuthority 기반으로 로컬 플레이어 업데이트
            if (IsLocalNetworkRig && hardwareRig)
            {
                RigState rigState = hardwareRig.RigState;
                ApplyLocalStateToRigParts(rigState);
                ApplyLocalStateToHandPoses(rigState);
            }
        }

        public override void Render()
        {
            base.Render();
            if (IsLocalNetworkRig && hardwareRig)
            {
                // 로컬 사용자의 경우 최신 하드웨어 위치로 즉시 업데이트
                RigState rigState = hardwareRig.RigState;

                transform.position = rigState.playAreaPosition;
                transform.rotation = rigState.playAreaRotation;
                leftHand.transform.position = rigState.leftHandPosition;
                leftHand.transform.rotation = rigState.leftHandRotation;
                rightHand.transform.position = rigState.rightHandPosition;
                rightHand.transform.rotation = rigState.rightHandRotation;
                headset.transform.position = rigState.headsetPosition;
                headset.transform.rotation = rigState.headsetRotation;
            }
        }

        // PlayerBody에서 HardwareRig를 수동으로 설정할 수 있는 메서드
        public void SetHardwareRig(HardwareRig rig)
        {
            if (IsLocalNetworkRig)
            {
                hardwareRig = rig;
                Debug.Log($"[CustomNetworkRig] {name} - HardwareRig manually set: {rig?.name}");
            }
            else
            {
                Debug.LogWarning($"[CustomNetworkRig] {name} - Cannot set HardwareRig on remote NetworkRig");
            }
        }
    }
}