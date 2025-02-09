using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class AnimationDelay : MonoBehaviour
{
    public float delay;

    private GameObject selfSailplane;
    // Start is called before the first frame update
    void Start()
    {
        selfSailplane = transform.GetChild(1).gameObject;
        selfSailplane.SetActive(false);
        StartCoroutine(playAnimationAfterSeconds(delay));
    }

    IEnumerator playAnimationAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        selfSailplane.SetActive(true);
        selfSailplane.transform.GetChild(0).GetComponent<SplineAnimate>().Play();
    }
}
