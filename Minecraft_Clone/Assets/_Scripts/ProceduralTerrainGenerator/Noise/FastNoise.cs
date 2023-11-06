using System;
using System.Runtime.InteropServices;

namespace Minecraft
{
    public class FastNoise
    {
        public enum NoiseType
        {
            OpenSimplex2 = 0,
            OpenSimplex2S = 1,
            Cellular = 2,
            Perlin = 3,
            ValueCubic = 4,
            Value = 5
        };

        public enum RotationType3D
        {
            None = 0,
            ImproveXYPlanes = 1,
            ImproveXZPlanes = 2,
        };

        public enum FractalType
        {
            None = 0,
            FBm = 1,
            Ridged = 2,
            PingPong = 3,
            DomainWarpProgressive = 4,
            DomainWarpIndependent = 5
        };

        public enum CellularDistanceFunction
        {
            Euclidean = 0,
            EuclideanSq = 1,
            Manhattan = 2,
            Hybrid = 3
        };

        public enum CellularReturnType
        {
            CellValue = 0,
            Distance = 1,
            Distance2 = 2,
            Distance2Add = 3,
            Distance2Sub = 4,
            Distance2Mul = 5,
            Distance2Div = 6
        };

        public enum DomainWarpType
        {
            OpenSimplex2 = 0,
            OpenSimplex2Reduced = 1,
            BasicGrid = 2
        };

        private const string DLL = "FastNoiseLib_2";

        private IntPtr _fastNoisePtr = IntPtr.Zero;

        public FastNoise(int seed = 1337)
        {
            _fastNoisePtr = GenerateNoiseInstance(seed);
        }

        ~FastNoise()
        {
            DestroyNoiseInstance(_fastNoisePtr);
            _fastNoisePtr = IntPtr.Zero;
        }

        public void SetSeed(int seed)
        {
            SetSeed(_fastNoisePtr, seed);
        }

        public void SetFrequency(float frequency)
        {
            SetFrequency(_fastNoisePtr, frequency);
        }

        public void SetNoiseType(NoiseType noiseType)
        {
            SetNoiseType(_fastNoisePtr, (int)noiseType);
        }

        public void SetRotationType3D(RotationType3D rotationType)
        {
            SetRotationType3D(_fastNoisePtr, (int)rotationType);
        }

        public void SetFractalType(FractalType fractalType)
        {
            SetFractalType(_fastNoisePtr, (int)fractalType);
        }

        public void SetFractalOctaves(int fractalOctaves)
        {
            SetFractalOctaves(_fastNoisePtr, fractalOctaves);
        }

        public void SetFractalLacunarity(float lacunarity)
        {
            SetFractalLacunarity(_fastNoisePtr, lacunarity);
        }

        public void SetFractalGain(float gain)
        {
            SetFractalGain(_fastNoisePtr, gain);
        }

        public void SetFractalWeightedStrength(float weightedStrength)
        {
            SetFractalWeightedStrength(_fastNoisePtr, weightedStrength);
        }

        public void SetFractalPingPongStrength(float pingPongStrength)
        {
            SetFractalPingPongStrength(_fastNoisePtr, pingPongStrength);
        }
        public void SetCellularDistanceFunction(CellularDistanceFunction cellularDistanceFunction)
        {
            SetCellularDistanceFunction(_fastNoisePtr, (int)cellularDistanceFunction);
        }

        public void SetCellularReturnType(CellularReturnType cellularReturnType)
        {
            SetCellularReturnType(_fastNoisePtr, (int)cellularReturnType);
        }

        public void SetCellularJitter(float cellularJitter)
        {
            SetCellularJitter(_fastNoisePtr, cellularJitter);
        }

        public void SetDomainWarpType(DomainWarpType domainWrapType)
        {
            SetDomainWarpType(_fastNoisePtr, (int)domainWrapType);
        }

        public void SetDomainWarpAmp(float domainWrapAmp)
        {
            SetDomainWarpAmp(_fastNoisePtr, domainWrapAmp);
        }

        public float GetNoise(float x, float y)
        {
            return GetNoise(_fastNoisePtr, x, y);
        }

        public float GetNoise(float x, float y, float z)
        {
            return GetNoise3D(_fastNoisePtr, x, y, z);
        }

        public void DomainWrap(ref float x, ref float y)
        {
            DomainWrap(_fastNoisePtr, ref x, ref y);
        }

        public void DomainWrap(ref float x, ref float y, ref float z)
        {
            DomainWrap3D(_fastNoisePtr, ref x, ref y, ref z);
        }


        // DLL function declarations
        [DllImport(DLL)]
        private static extern IntPtr GenerateNoiseInstance(int seed);

        [DllImport(DLL)]
        private static extern void DestroyNoiseInstance(IntPtr intPtr);

        [DllImport(DLL)]
        private static extern void SetSeed(IntPtr intPtr, int seed);

        [DllImport(DLL)]
        private static extern void SetFrequency(IntPtr intPtr, float frequency);

        [DllImport(DLL)]
        private static extern void SetNoiseType(IntPtr intPtr, int noiseType);

        [DllImport(DLL)]
        private static extern void SetRotationType3D(IntPtr intPtr, int rotationType);

        [DllImport(DLL)]
        private static extern void SetFractalType(IntPtr intPtr, int fractalType);

        [DllImport(DLL)]
        private static extern void SetFractalOctaves(IntPtr intPtr, int fractalOctaves);

        [DllImport(DLL)]
        private static extern void SetFractalLacunarity(IntPtr intPtr, float lacunarity);

        [DllImport(DLL)]
        private static extern void SetFractalGain(IntPtr intPtr, float gain);

        [DllImport(DLL)]
        private static extern void SetFractalWeightedStrength(IntPtr intPtr, float weightedStrength);

        [DllImport(DLL)]
        private static extern void SetFractalPingPongStrength(IntPtr intPtr, float pingPongStrength);

        [DllImport(DLL)]
        private static extern void SetCellularDistanceFunction(IntPtr intPtr, int cellularDistanceFunction);

        [DllImport(DLL)]
        private static extern void SetCellularReturnType(IntPtr intPtr, int cellularReturnType);

        [DllImport(DLL)]
        private static extern void SetCellularJitter(IntPtr intPtr, float cellularJitter);

        [DllImport(DLL)]
        private static extern void SetDomainWarpType(IntPtr intPtr, int domainWrapType);

        [DllImport(DLL)]
        private static extern void SetDomainWarpAmp(IntPtr intPtr, float domainWrapAmp);

        [DllImport(DLL)]
        private static extern float GetNoise(IntPtr intPtr, float x, float y);

        [DllImport(DLL)]
        private static extern float GetNoise3D(IntPtr intPtr, float x, float y, float z);

        [DllImport(DLL)]
        private static extern void DomainWrap(IntPtr intPtr, ref float x, ref float y);

        [DllImport(DLL)]
        private static extern void DomainWrap3D(IntPtr intPtr, ref float x, ref float y, ref float z);
    }
}
