using UnityEngine;
using System.Runtime.InteropServices;

namespace SimpleParticleSystem
{

    [StructLayout(LayoutKind.Explicit)]
    public struct ParticleData2D
    {
        [FieldOffset(0)]
        public bool isActive;
        [FieldOffset(4)]
        public int id;
        [FieldOffset(8)]
        public Vector2 position;
    }

    public class SimpleParticleSystem2D : SimpleParticleSystemBase<ParticleData2D>
    {

        [SerializeField] ComputeShader _particleCS;

        protected override void InitParticles()
        {
            int kernel = _particleCS.FindKernel("Init");
            _particleCS.SetBuffer(kernel, "_ParticlesBuffer", ParticleBuffer);
            _particleCS.Dispatch(kernel, ParticleNum / THREAD_NUM_X, 1, 1);
        }

        protected override void UpdateParticles()
        {
            _particleCS.SetFloat("_TimeSinceStartup", Time.realtimeSinceStartup);

            int kernel = _particleCS.FindKernel("Update");
            _particleCS.SetBuffer(kernel, "_ParticlesBuffer", ParticleBuffer);
            _particleCS.Dispatch(kernel, ParticleNum / THREAD_NUM_X, 1, 1);

        }

    }
}