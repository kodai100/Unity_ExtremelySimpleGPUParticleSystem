using System.Runtime.InteropServices;
using UnityEngine;

namespace SimpleParticleSystem
{

    public enum ParticleNumEnum
    {
        NUM_8K = 8 * 1024,
        NUM_16K = 16 * 1024,
        NUM_32K = 32 * 1024,
        NUM_64K = 64 * 1024
    }


    public abstract class SimpleParticleSystemBase<T> : MonoBehaviour where T : struct
    {

        protected const int THREAD_NUM_X = 1024;


        [SerializeField] ParticleNumEnum _particleNumEnum = ParticleNumEnum.NUM_8K;
        [SerializeField] Material _particleRenderMaterial;

        private int _particleNum;
        private ComputeBuffer _particleBuffer;

        public int ParticleNum { get { return _particleNum; } }
        public ComputeBuffer ParticleBuffer { get { return _particleBuffer; } }

        void Start()
        {
            _particleNum = (int)_particleNumEnum;
            _particleBuffer = new ComputeBuffer(_particleNum, Marshal.SizeOf(typeof(T)));
            InitParticles();
        }
        
        void Update()
        {
            UpdateParticles();
        }
        

        protected abstract void InitParticles();
        protected abstract void UpdateParticles();
        

        private void OnRenderObject()
        {
            _particleRenderMaterial.SetPass(0); // !important
            _particleRenderMaterial.SetBuffer("_ParticlesBuffer", _particleBuffer);
            Graphics.DrawProcedural(MeshTopology.Points, _particleNum);
        }

        private void OnDestroy()
        {
            _particleBuffer.Release();
        }
    }
}


