using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit)]
public struct ParticleData
{
    [FieldOffset(0)]
    bool isActive;
    [FieldOffset(4)]
    int id;
    [FieldOffset(8)]
    float life;
    [FieldOffset(12)]
    Vector2 position;
    [FieldOffset(20)]
    Vector2 velocity;
}



public class Waterfall : MonoBehaviour {

    private const int THREAD_NUM_X = 1024;

    [SerializeField]
    int _maxParticleNum = 64 * 1024;

    [SerializeField, Range(0, 1)]
    float _throttle = 1.0f;

    [SerializeField]
    Vector2 _emitterCenter = new Vector2(0, 0);

    [SerializeField]
    Vector2 _emitterSize = new Vector2(1, 1);

    [SerializeField]
    float _life = 6;

    [SerializeField, Range(0, 1)]
    float _lifeRandomness = 0.6f;

    [SerializeField]
    Vector2 _initialVelocity = new Vector2(0, 4.0f);

    [SerializeField]
    Vector2 _direction;

    [SerializeField, Range(0, 1)]
    float _directionSpread = 0.2f;

    [SerializeField]
    float _spread;

    [SerializeField, Range(0, 1)]
    float _speedRandomness = 0.5f;

    [SerializeField]
    int _randomSeed = 0;

    [SerializeField]
    Vector2 _acceleration;
    
    [SerializeField]
    ComputeShader _waterfallCS;

    [SerializeField]
    Material _particleRenderMaterial;

    ComputeBuffer _bufferRead;
    ComputeBuffer _bufferWrite;


    [SerializeField] private Rect _domain;


    static float deltaTime {
        get {
            var isEditor = !Application.isPlaying || Time.frameCount < 2;
            return isEditor ? 1.0f / 10 : Time.deltaTime;
        }
    }


    void InitializeBuffers()
    {
        _bufferRead = new ComputeBuffer(_maxParticleNum, Marshal.SizeOf(typeof(ParticleData)));
        _bufferWrite = new ComputeBuffer(_maxParticleNum, Marshal.SizeOf(typeof(ParticleData)));



        _waterfallCS.SetFloat("_NumParticles", _maxParticleNum);
        _waterfallCS.SetVector("_EmitterPos", _emitterCenter);
        _waterfallCS.SetVector("_EmitterSize", _emitterSize);
        _waterfallCS.SetVector("_Config", new Vector4(_throttle, _randomSeed, deltaTime, Time.time));

        int kernel = _waterfallCS.FindKernel("Init");
        _waterfallCS.SetBuffer(kernel, "_ParticlesBufferWrite", _bufferRead);
        _waterfallCS.Dispatch(kernel, _maxParticleNum / THREAD_NUM_X, 1, 1);

    }

	
	void Start () {

        InitializeBuffers();

	}


    void Update()
    {

        _waterfallCS.SetInt("_NumParticles", _maxParticleNum);
        _waterfallCS.SetVector("_EmitterPos", _emitterCenter);
        _waterfallCS.SetVector("_EmitterSize", _emitterSize);
        _waterfallCS.SetVector("_Config", new Vector4(_throttle, _randomSeed, deltaTime, Time.time));

        var invLifeMax = 1.0f / Mathf.Max(_life, 0.01f);
        var invLifeMin = invLifeMax / Mathf.Max(1 - _lifeRandomness, 0.01f);
        _waterfallCS.SetVector("_LifeParams", new Vector2(invLifeMin, invLifeMax));

        _waterfallCS.SetVector("_Direction", new Vector3(_direction.x, _direction.y, _spread));
        _waterfallCS.SetVector("_Acceleration", _acceleration);

        var speed = _initialVelocity.magnitude;
        var dir = _initialVelocity / speed;
        _waterfallCS.SetVector("_Direction", new Vector3(dir.x, dir.y, _directionSpread));
        _waterfallCS.SetVector("_SpeedParams", new Vector2(speed, _speedRandomness));

        int kernel = _waterfallCS.FindKernel("Update");
        _waterfallCS.SetBuffer(kernel, "_ParticlesBufferRead", _bufferRead);
        _waterfallCS.SetBuffer(kernel, "_ParticlesBufferWrite", _bufferWrite);
        _waterfallCS.Dispatch(kernel, _maxParticleNum / THREAD_NUM_X, 1, 1);

        SwapBuffers(ref _bufferRead, ref _bufferWrite);
    }

    void SwapBuffers(ref ComputeBuffer src, ref ComputeBuffer dst)
    {
        ComputeBuffer tmp = src;
        src = dst;
        dst = tmp;
    }

    private void OnRenderObject()
    {
        _particleRenderMaterial.SetPass(0); // !important
        _particleRenderMaterial.SetBuffer("_ParticlesBuffer", _bufferRead);
        Graphics.DrawProcedural(MeshTopology.Points, _maxParticleNum);
    }

    private void OnDestroy()
    {
        _bufferRead.Release();
        _bufferWrite.Release();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(_emitterCenter, _emitterSize);
        Gizmos.DrawWireCube(new Vector2(_domain.xMin, _domain.yMin), new Vector2(_domain.xMax, _domain.yMax));
    }


}
