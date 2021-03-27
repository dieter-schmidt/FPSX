using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class U10PS_DissolveOverTime : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    //added by me - used to restore after object dissolves
    //Material[] originalMats;

    public float speed = .5f;

    private void Start(){
        meshRenderer = this.GetComponent<MeshRenderer>();
        skinnedMeshRenderer = this.GetComponent<SkinnedMeshRenderer>();
        //originalMats = meshRenderer.materials;
    }

    private float t = 0.0f;
    private void Update(){
        if (skinnedMeshRenderer != null)
        {
            processSkinnedMesh();
        }
        else
        {
            processMesh();
        }
    }

    private void processMesh()
    {
        Material[] mats = meshRenderer.materials;

        mats[0].SetFloat("_Cutoff", Mathf.Sin(t * speed));
        t += Time.deltaTime;

        // Unity does not allow meshRenderer.materials[0]...
        meshRenderer.materials = mats;

        //aded by me - reset
        if (Mathf.Abs(mats[0].GetFloat("_Cutoff")) > 0.90f)
        {
            t = 0.0f;
            //mats[0].SetFloat("_Cutoff", 0f);
        }
    }

    private void processSkinnedMesh()
    {
        Material[] mats = skinnedMeshRenderer.materials;

        foreach (Material mat in mats)
        {
            mat.SetFloat("_Cutoff", Mathf.Sin(t * speed));
            t += Time.deltaTime;
        }

        // Unity does not allow meshRenderer.materials[0]...
        meshRenderer.materials = mats;

        //aded by me - reset
        if (Mathf.Abs(mats[0].GetFloat("_Cutoff")) > 0.90f)
        {
            t = 0.0f;
            //mats[0].SetFloat("_Cutoff", 0f);
        }
    }
}
