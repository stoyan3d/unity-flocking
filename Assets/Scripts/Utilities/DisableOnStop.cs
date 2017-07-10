using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnStop : MonoBehaviour {

    ParticleSystem particle;

	// Use this for initialization
	void Start () {
        particle = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
		if (particle.isStopped)
        {
            SimplePool.Despawn(gameObject);
        }
	}
}
