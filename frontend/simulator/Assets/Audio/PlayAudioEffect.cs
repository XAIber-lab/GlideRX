using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioEffect : MonoBehaviour
{
    public float maximumSpeed = 270f;
    private Rigidbody sailplane;
    private void Start()
    {
        sailplane = transform.parent.GetComponent<Rigidbody>();
    }

    void Update()
    {
        float alpha = sailplane.velocity.magnitude / maximumSpeed;
        if (alpha > 1)
            alpha = 1;

        this.GetComponent<AudioSource>().volume = alpha;
    }
}
