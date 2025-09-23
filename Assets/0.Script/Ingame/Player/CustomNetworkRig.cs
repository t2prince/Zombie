using Fusion;
using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Jamcat.Ingame.Player
{
    public class CustomNetworkRig : NetworkRig
    {
        private bool _playerIdInitialized = false;

        // InputAuthority 기반으로 로컬 NetworkRig 판단
        public override bool IsLocalNetworkRig =>
            Object &&
            Object.InputAuthority == Runner.LocalPlayer;

        public override void Spawned()
        {
            base.Spawned();

            Debug.Log($"[CustomNetworkRig] Spawned - Name: {name}, PlayerId: {PlayerId}, IsLocalNetworkRig: {IsLocalNetworkRig}, InputAuthority: {Object.InputAuthority.PlayerId}, LocalPlayer: {Runner.LocalPlayer.PlayerId}, IsServer: {Runner.IsServer}, IsClient: {Runner.IsClient}, HasStateAuthority: {Object.HasStateAuthority}");

            // 로컬 플레이어인 경우에만 HardwareRig 초기화
            if (IsLocalNetworkRig)
            {
                // 서버(호스트)인 경우 자신의 첫번째 NetworkRig에만 초기화
                if (Runner.IsServer)
                {
                    // 호스트의 경우 이미 다른 NetworkRig에서 초기화가 되었는지 확인
                    var existingRigs = FindObjectsByType<CustomNetworkRig>(FindObjectsSortMode.None);
                    bool alreadyInitialized = false;

                    foreach (var existingRig in existingRigs)
                    {
                        if (existingRig != this &&
                            existingRig.Object != null &&
                            existingRig.Object.InputAuthority == Object.InputAuthority &&
                            existingRig.hardwareRig != null)
                        {
                            alreadyInitialized = true;
                            Debug.Log($"[CustomNetworkRig] {name} - Server's HardwareRig already initialized in another NetworkRig: {existingRig.name}, skipping");
                            break;
                        }
                    }

                    if (alreadyInitialized)
                    {
                        return;
                    }
                }

                Debug.Log($"[CustomNetworkRig] {name} - This is MY NetworkRig (Player ID: {Object.InputAuthority.PlayerId}), initializing HardwareRig");

                // 지연 초기화: 다음 프레임에서 초기화 (PlayerCamera가 준비될 때까지 대기)
                StartCoroutine(DelayedInitialization());
            }
            else
            {
                Debug.Log($"[CustomNetworkRig] {name} - Remote NetworkRig (InputAuthority: {Object.InputAuthority.PlayerId}), skipping HardwareRig setup");
            }
        }

        private System.Collections.IEnumerator DelayedInitialization()
        {
            // 1프레임 대기
            yield return null;

            Debug.Log($"[CustomNetworkRig] {name} - Starting delayed initialization for local player");

            // 다시 한번 로컬 플레이어인지 확인
            if (!IsLocalNetworkRig)
            {
                Debug.LogWarning($"[CustomNetworkRig] {name} - No longer local NetworkRig, aborting initialization");
                yield break;
            }

            // PlayerFollowerCamera를 통해 HardwareRig 찾기
            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
            if (playerCamera != null)
            {
                hardwareRig = playerCamera.GetComponentInChildren<HardwareRig>();
                if (hardwareRig != null)
                {
                    Debug.Log($"[CustomNetworkRig] {name} - Found HardwareRig through PlayerCamera: {hardwareRig.name}");
                    InitializeLocomotion();
                }
                else
                {
                    Debug.LogError($"[CustomNetworkRig] {name} - HardwareRig not found in PlayerCamera");
                }
            }
            else
            {
                Debug.LogError($"[CustomNetworkRig] {name} - PlayerCamera not found for local player");
            }
        }

        private void InitializeLocomotion()
        {
            // NetworkRig 하위의 Locomotion 컴포넌트 찾기
            var locomotion = GetComponentInChildren<Jamcat.Locomotion.Locomotion>();
            if (locomotion != null)
            {
                Debug.Log($"[CustomNetworkRig] {name} - Initializing Locomotion with HardwareRig: {hardwareRig.name}");
                locomotion.Init(this, hardwareRig);
            }
            else
            {
                Debug.LogWarning($"[CustomNetworkRig] {name} - Locomotion component not found in children");
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            // PlayerId가 아직 설정되지 않았다면 설정
            if (!_playerIdInitialized && PlayerId == -1)
            {
                // StateAuthority 또는 서버에서 설정
                if (Object.HasStateAuthority || Runner.IsServer)
                {
                    PlayerId = Object.InputAuthority.PlayerId;
                    _playerIdInitialized = true;
                    Debug.Log($"[CustomNetworkRig] FixedUpdateNetwork - PlayerId set: {PlayerId} for {name} (HasStateAuthority: {Object.HasStateAuthority}, IsServer: {Runner.IsServer})");
                }
            }

            // PlayerId가 동기화되면 모든 클라이언트에서 오브젝트 이름 설정
            if (PlayerId != -1 && !name.Contains($"NetworkRig_{PlayerId}"))
            {
                name = $"NetworkRig_{PlayerId}";
                Debug.Log($"[CustomNetworkRig] Object name synchronized: {name}");
            }

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