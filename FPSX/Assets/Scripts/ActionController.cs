using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionController : MonoBehaviour
{

    public HandgunScriptLPFP gunController;
    public EnemiesController enemiesController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))// && gunController.getIsReloading() == false)
        {
            Debug.Log("SNAP");
            enemiesController.snap();
        }
    }
}
