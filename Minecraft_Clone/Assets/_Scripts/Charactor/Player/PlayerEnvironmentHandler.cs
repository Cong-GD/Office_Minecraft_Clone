using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Minecraft
{
    public class PlayerEnvironmentHandler : MonoBehaviour
    {
        [SerializeField]
        private PlayerData_SO playerData;

        [SerializeField]
        private Transform foot;

        [SerializeField]
        private Transform body;

        [SerializeField]
        private Transform head;

        [SerializeField]
        private Camera mainCam;

        [SerializeField]
        private Volume volume;

        [SerializeField]
        private VolumeProfile normalProfile;

        [SerializeField]
        private VolumeProfile underWaterProfile;

        private bool _isCammeraUnderWater;

        private void Update()
        {
            GroundCheck();
            WaterCheck();
            CameraCheck();
        }

        private void OnDestroy()
        {
            volume.profile = normalProfile;
            RenderSettings.fog = false;
        }

        public void GroundCheck()
        {
            playerData.isGrounded = Physics.CheckSphere(foot.position, playerData.GroundRadius, playerData.GroundLayer, QueryTriggerInteraction.Ignore);
        }

        public void WaterCheck()
        {
            playerData.isStepInWater = Chunk.CheckWater(foot.position);
            playerData.isBobyInWater = Chunk.CheckWater(body.position);
            playerData.isHeadInWater = Chunk.CheckWater(head.position);
        }

        private void CameraCheck()
        {
            var isUndeWater = Chunk.CheckWater(mainCam.transform.position);
            if (isUndeWater == _isCammeraUnderWater)
                return;

            _isCammeraUnderWater = isUndeWater;

            volume.profile = isUndeWater ? underWaterProfile : normalProfile;
            RenderSettings.fog = isUndeWater;
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = playerData.isGrounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(
                foot.position,
                playerData.GroundRadius);
        }
#endif
    }
}