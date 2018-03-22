using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParticleSystemBase : MonoBehaviour {

    public const int THREAD_NUM_X = 1024;

    public List<ParticleSystemBase> _updaters = new List<ParticleSystemBase>();

    protected ComputeBuffer _bufferRead;
    protected ComputeBuffer _bufferWrite;

    
	void Start () {

        _updaters.ForEach(updater => {



        });



	}
	
	void Update () {
		
	}
}
