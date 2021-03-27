using UnityEngine;

public class Gun : MonoBehaviour
{

    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 4f;
    public float impactForce = 30f;

    public Camera fpsCam;
    public GameObject impactEffect;
    public AudioSource gunAudio;


    private float nextTimeToFire = 0f;

    private int layerEnemy;


    void Start()
    {
        layerEnemy = LayerMask.NameToLayer("Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        //remove "Down" for automatic
        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        gunAudio.Play();

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Object Hit: " + hit.transform.name);

            //Enemy enemy = hit.transform.GetComponent<Enemy>();
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)//(hit.transform.gameObject.layer == layerEnemy)
            {
                enemy.TakeDamage(damage, -hit.normal);
            }

            //add force on hit in direction of impact
            //if (hit.rigidbody != null)
            //{
            //    hit.rigidbody.AddForce(-hit.normal * impactForce);
            //}

            //spawn impact particles
            GameObject impactObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactObject, 2f);
        }
    }
}
