using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{

    public string obj_name = "temp";
    public float value = 10;
    public AudioClip collected_sfx;
    public float collected_sfx_volume = 1.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // animation?? dunno
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (other.GetComponent<Player>().alive)
            {
                //Debug.Log("Collided with player");
                // player executes code 
                other.GetComponent<Player>().OnCollect(this);
                StartCoroutine(CollectCoroutine());
            }
        }
    }

    public IEnumerator CollectCoroutine()
    {
        // Hide sprite
        GetComponent<SpriteRenderer>().enabled = false;

        // Play SFX
        AudioSource audio_src = GetComponent<AudioSource>();
        audio_src.clip = collected_sfx;
        audio_src.volume = collected_sfx_volume;
        audio_src.pitch = Random.Range(0.8f, 1.2f);
        audio_src.Play();

        // Destroy object after SFX plays completely
        yield return new WaitForSeconds(collected_sfx.length);
        //Destroy(gameObject);
        
        this.GetComponent<Renderer>().enabled = false;
        this.GetComponent<Collider2D>().enabled = false;
        
        //Respawn(5f);
    }

    public void Respawn(float delay)
    {
        StopCoroutine("RespawnTimer");
        StartCoroutine("RespawnTimer", delay);
    }

    public IEnumerator RespawnTimer(float delay)
    {
        yield return new WaitForSeconds(delay);

        this.GetComponent<Renderer>().enabled = true;
        this.GetComponent<Collider2D>().enabled = true;
    }
}
