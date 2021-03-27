using UnityEngine;

public class Enemy : MonoBehaviour
{

    public float health = 20f;
    public AudioSource damageAudio;
    public AudioSource markAudio;
    public AudioSource snapAudio;
    public EnemiesController controller;
    private Rigidbody rb;

    public float deathForce;

    private void Start()
    {
        //controller = transform.parent.gameObject.GetComponent<EnemiesController>();
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die(hitDirection);
        }
        else
        {
            damageAudio.Play();
        }
    }

    public void TakeMarker(Vector3 snapForce)
    {
        controller.addMarkedEnemy((this, snapForce));
        markAudio.Play();
    }


    //regular death
    public void Die(Vector3 hitDirection)
    {
        //destroyAudio.Play();
        //controller = transform.parent.gameObject.GetComponent<EnemiesController>();
        if (controller != null)
        {
            //Debug.Log("TEST");
            if (this.tag == "Enemy")
            {
                controller.playAudioAtPosition(transform.position);
                Destroy(gameObject);
            }
            else if (this.tag == "HumanEnemy" && hitDirection != null)
            {
                int counter = 0;
                //Debug.Log("TEST2");
                foreach (Rigidbody body in this.GetComponentsInChildren<Rigidbody>())
                {
                    //Debug.Log(body.gameObject.name);
                    counter++;
                    string name = body.gameObject.name;
                    
                    //if (name == "B-head" || name == "B-chest")
                    //{
                    body.AddForce(hitDirection * deathForce);
                    //Debug.Log("TEST4");
                    body.AddTorque(new Vector3(20f, 20f, 20f));
                    //}
                }
                controller.playHumanAudioAtPosition(transform.position);
                //Debug.Log(counter);
                Destroy(gameObject, 3f);
            }
        }
        
    }

    //death after snapping
    public void SnapDie(Vector3 snapForce)
    {
        //controller = transform.parent.gameObject.GetComponent<EnemiesController>();
        if (controller != null)
        {
            controller.playSnapAudioAtPosition(transform.position);

            if (rb != null)
            {
                rb.AddForce(snapForce);
                //Debug.Log(snapForce);
                rb.AddTorque(new Vector3(20f, 20f, 20f));
            }
            else
            {

                foreach (Rigidbody body in this.GetComponentsInChildren<Rigidbody>())
                {
                    string name = body.gameObject.name;
                    if (name == "B-head" || name == "B-chest")
                    {
                        body.AddForce(snapForce);
                        //Debug.Log(snapForce);
                        body.AddTorque(new Vector3(20f, 20f, 20f));
                    }
                }
            }
        }
        Destroy(gameObject, 3f);
    }


}
