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

            if (IsLocalNetworkRig)
            {
                if (Runner.IsServer)
                {
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
                            break;
                        }
                    }

                    if (alreadyInitialized) return;
                }

                StartCoroutine(DelayedInitialization());
            }
        }

        private System.Collections.IEnumerator DelayedInitialization()
        {
            yield return null;

            if (!IsLocalNetworkRig) yield break;

            var playerCamera = FindAnyObjectByType<PlayerFollowerCamera>();
            if (playerCamera != null)
            {
                hardwareRig = playerCamera.GetComponentInChildren<HardwareRig>();
                if (hardwareRig != null)
                {
                    InitializeLocomotion();
                }
            }
        }

        private void InitializeLocomotion()
        {
            var locomotion = GetComponentInChildren<Jamcat.Locomotion.Locomotion>();
            if (locomotion != null)
            {
                locomotion.Init(this, hardwareRig);
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (!_playerIdInitialized && PlayerId == -1)
            {
                if (Object.HasStateAuthority || Runner.IsServer)
                {
                    PlayerId = Object.InputAuthority.PlayerId;
                    _playerIdInitialized = true;
                }
            }

            if (PlayerId != -1 && !name.Contains($"NetworkRig_{PlayerId}"))
            {
                name = $"NetworkRig_{PlayerId}";
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

        public void SetHardwareRig(HardwareRig rig)
        {
            if (!IsLocalNetworkRig) return;

            int myPlayerId = Runner.LocalPlayer.PlayerId;
            if (PlayerId != -1 && PlayerId != myPlayerId) return;

            hardwareRig = rig;
        }

    }
}