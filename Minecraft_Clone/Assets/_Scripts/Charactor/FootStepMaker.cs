using FMOD.Studio;
using FMODUnity;
using Minecraft.Audio;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft
{

    public interface IMovementData
    {
        public Vector3 Velocity { get; }
        public bool IsGrounded { get; }
    }

    public class FootStepMaker : MonoBehaviour
    {
        [SerializeField]
        private Transform groundPosition;

        [SerializeField]
        private EventReference footStepEvent;

        [SerializeField]
        private BlockMaterial groundSuface;

        [SerializeField, Range(0, 1f)]
        private float volume = 1f;

        [MinMaxSlider(0f, 10f)]
        public Vector2 speedRange = new Vector2(0f, 5f);

        private EventInstance _footStepInstance;
        private PARAMETER_ID _blockMaterialParameterId;
        private IMovementData _movementData;
        private float _lastPlayTime;

        private void Awake()
        {
            _footStepInstance = RuntimeManager.CreateInstance(footStepEvent);
            AudioManager.GetParameterID(_footStepInstance, "BlockMaterial", out _blockMaterialParameterId);
            _footStepInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            if(!TryGetComponent(out _movementData))
            {
                _movementData = GetComponentInParent<IMovementData>();
            }
            if(_movementData == null)
            {
                Debug.LogError("Can't find IMovementData");
            }
        }

        private void OnDestroy()
        {
            _footStepInstance.release();
        }

        private void Update()
        {
            if(_movementData.IsGrounded)
            {
                Vector2 flatVelocity = _movementData.Velocity.XZ();
                float speed = flatVelocity.magnitude;
                if(speed < speedRange.x + 0.1f)
                {
                    return;
                }

                float time = Time.time;
                if(time - _lastPlayTime > 1f / speed * 2f)
                {
                    _lastPlayTime = time;
                    BlockMaterial steppingBlock = Chunk.GetBlock(groundPosition.position).Data().BlockMaterial;
                    PlayFootStep(steppingBlock, speed);
                }
            }
        }

        public void PlayFootStep(BlockType blockType, float speed)
        {
            float volume = math.remap(speedRange.x, speedRange.y, 0f, 1f, speed);
            BlockMaterial groundSuface = blockType.Data().BlockMaterial;
            PlayFootStep(groundSuface, volume);
        }

        public void PlayFootStep(BlockMaterial groundSuface, float volume = 1f)
        {
            this.groundSuface = groundSuface;
            this.volume = math.saturate(volume);
            PlayFootStep();
        }

        [Button("Play", EButtonEnableMode.Playmode)]
        private void PlayFootStep()
        {
            _footStepInstance.setParameterByID(_blockMaterialParameterId, (float)groundSuface);
            _footStepInstance.setVolume(volume);
            _footStepInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            _footStepInstance.start();
        }

    }
}
