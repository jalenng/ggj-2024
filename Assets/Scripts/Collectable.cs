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
            //Debug.Log("Collided with player");
            // player executes code 
            if (other.GetComponent<Player>().fartMeter < 100)
            {
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
        Destroy(gameObject);
    }
}
