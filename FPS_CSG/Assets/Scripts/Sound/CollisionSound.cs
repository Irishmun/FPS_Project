using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource)), RequireComponent(typeof(Rigidbody))]
public class CollisionSound : MonoBehaviour
{
    [SerializeField, Tooltip("If the object can be destroyed or not.")]
    private bool Destroyable = false;
    [Header("Sound settings")]
    [SerializeField, Tooltip("What sounds this object should make on collisions.")]
    private MaterialSoundScriptableObject Sounds;
    [SerializeField]
    private float MinSoftCollisionVelocity = 1f;
    [SerializeField]
    private float MinHardCollisionVelocity = 10f;

    private Rigidbody _rb;
    private AudioSource _Audio;
    private float _CurrentVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _Audio = GetComponent<AudioSource>();
        if (MinHardCollisionVelocity > 0)
        {
            //ensure that soft collision sounds always play at lower speed than hard collision sounds
            MinSoftCollisionVelocity = MinSoftCollisionVelocity > MinHardCollisionVelocity ? MinHardCollisionVelocity - 0.1f : MinSoftCollisionVelocity;
        }
    }

    private void FixedUpdate()
    {
        _CurrentVelocity = _rb.velocity.magnitude;
        //Debug.Log($"{gameObject.name}'s velocity: {_CurrentVelocity}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Sounds != null)
        {
            if (_CurrentVelocity >= MinSoftCollisionVelocity)
            {
                if (_CurrentVelocity < MinHardCollisionVelocity)
                {
                    //play soft sound
                    Debug.Log("Soft Collision on " + gameObject.name);
                    Sounds.PlayCollisionSound(MaterialSoundScriptableObject.CollisionSpeed.Soft, _Audio);
                }
                else
                {
                    //play hard sound
                    Debug.Log("Hard Collision on " + gameObject.name);
                    Sounds.PlayCollisionSound(MaterialSoundScriptableObject.CollisionSpeed.Hard, _Audio);
                }
            }
        }
    }

    public void PlayDestroySound()
    {
        Sounds.PlayCollisionSound(MaterialSoundScriptableObject.CollisionSpeed.Destructive, _Audio);
    }
}
