namespace Minecraft
{
    public abstract class NoiseInstance
    {
        public abstract float GetNoise(float x, float y);
        public abstract float GetNoise(float x, float y, float z);
    }


    public class SingleNoise : NoiseInstance
    {
        private readonly FastNoise _fastNoise;

        public SingleNoise(FastNoise fastNoise)
        {
            _fastNoise = fastNoise;
        }

        public override float GetNoise(float x, float y)
        {
            return _fastNoise.GetNoise(x, y);
        }

        public override float GetNoise(float x, float y, float z)
        {
            return _fastNoise.GetNoise(x, y, z);
        }
    }


    public class DomainWrappedNoise : NoiseInstance
    {
        private readonly FastNoise _domainWrapper;
        private readonly NoiseInstance _noiseInstance;

        public DomainWrappedNoise(NoiseInstance noiseInstance, FastNoise domainWrapper)
        {
            _domainWrapper = domainWrapper;
            _noiseInstance = noiseInstance;
        }

        public override float GetNoise(float x, float y)
        {
            _domainWrapper.DomainWrap(ref x, ref y);
            return _noiseInstance.GetNoise(x, y);
        }

        public override float GetNoise(float x, float y, float z)
        {
            _domainWrapper.DomainWrap(ref x, ref y, ref z);
            return _noiseInstance.GetNoise(x, y, z);
        }
    }


    public class PostProcessedNoise : NoiseInstance
    {
        private readonly NoiseInstance _noiseInstance;
        private readonly INoisePostProcessor[] _postProcessors;

        public PostProcessedNoise(NoiseInstance noiseInstance, INoisePostProcessor[] postProcessors)
        {
            _noiseInstance = noiseInstance;
            _postProcessors = postProcessors;
        }

        public override float GetNoise(float x, float y)
        {
            float noiseValue = _noiseInstance.GetNoise(x, y);
            for (int i = 0; i < _postProcessors.Length; i++)
            {
                noiseValue = _postProcessors[i].Process(noiseValue);
            }
            return noiseValue;
        }

        public override float GetNoise(float x, float y, float z)
        {
            float noiseValue = _noiseInstance.GetNoise(x, y, z);
            for (int i = 0; i < _postProcessors.Length; i++)
            {
                noiseValue = _postProcessors[i].Process(noiseValue);
            }
            return noiseValue;
        }
    }
}