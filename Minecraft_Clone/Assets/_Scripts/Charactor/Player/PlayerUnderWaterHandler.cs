using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Minecraft
{
    public class PlayerUnderWaterHandler : MonoBehaviour
    {
        [ShowNonSerializedField]
        private bool isCammeraUnderWater;

        [SerializeField]
        private Camera mainCam;

        [SerializeField]
        private Volume volume;

        [SerializeField]
        private VolumeProfile normalProfile;

        [SerializeField]
        private VolumeProfile underWaterProfile;

        private void Update()
        {
            var isUndeWater = Chunk.CheckWater(mainCam.transform.position);
            if (isUndeWater == isCammeraUnderWater)
                return;

            isCammeraUnderWater = isUndeWater;

            volume.profile = isUndeWater ? underWaterProfile : normalProfile;
            RenderSettings.fog = isUndeWater;
        }
    }
}
