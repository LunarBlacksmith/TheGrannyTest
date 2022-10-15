using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelCollisionManager : MonoBehaviour
{
    [Tooltip("This particle system should be the one that controls the smoke coming from the tyres when burst (ie. going over the spike traps.)")]
    [SerializeField] private ParticleSystem _smokeParticleSystem;

    // the module within the Particle System that controls the emission variables.
    private ParticleSystem.EmissionModule _psEmissionModule;

    private Collider objectCollidedWith;

    /// <summary>
    /// Public property to get access to the Smoke Particle System prefab the spike trap object is linked to.
    /// </summary>
    public ParticleSystem SmokeParticleSystem { get { return _smokeParticleSystem; } set { _smokeParticleSystem = value; } }

    private void Awake()
    {
        // setting the local variable for emission module component to the smoke PS prefab's emission module
        _psEmissionModule = SmokeParticleSystem.emission;
        // disabling its emission on game beginning
        _psEmissionModule.enabled = false;

        Debug.Log($"Awake has been run on the Spike Trap." +
            $"\nEmission Module state: enabled({_psEmissionModule.enabled}).");
    }
    // ------------------------------------------------------------------------------------------------

    private void Update()
    {
        if (objectCollidedWith != null)
        {
            Debug.DrawLine(GetComponent<Collider>().bounds.center, objectCollidedWith.transform.position, Color.blue, 1f);
        }
    }

    // other is the Collider of the gameobject being collided with, not this object's Collider.
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("<color=blue>OnTriggerEnter()</color> has been called in <color=green>SpikeManager</color>.");

        objectCollidedWith = other;

        // if the Collider this object collided with is of the type WheelCollider
        // meaning - if a car wheel drove over the spikes
        //other.gameObject.tag == "SpikeTrap"
        if (other is BoxCollider)
        {
            Debug.Log($"<color=blue>other.tag</color> is <color=green>SpikeTrap</color> is true.");
            // enable emission of the prefab's particle system (shouldn't display since not actually in the scene)
            _psEmissionModule.enabled = true;
            Vector3 spawnPos = transform.position;
            spawnPos.x -= 0.5f;

            Debug.Log("Instantiating smoke...");
            Instantiate(SmokeParticleSystem, spawnPos, Quaternion.Euler(-90f, 0f, 0f), transform);

            Debug.Log($"Disabling the Smoke Particle System prefab's emission...");

            // disable emission of the prefab's particle system after instantiation
            _psEmissionModule.enabled = false;
        }

        Debug.Log($"<color=blue>OnTriggerEnter()</color> has reached the end of its method body.");
    }
}
