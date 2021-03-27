using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrustumManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var camera = GetComponent<Camera>();
        Vector3[] frustumCorners = new Vector3[4];
        //camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), 1f, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        //Debug.Log("FRUSTUM CORNERS: (" + frustumCorners[0] + frustumCorners[1] + frustumCorners[2] + frustumCorners[3]);

        for (int i = 0; i < 4; i++)
        {
            var worldSpaceCorner = camera.transform.TransformVector(frustumCorners[i]);
            Debug.DrawRay(camera.transform.position, worldSpaceCorner, Color.blue);
        }
    }
}
