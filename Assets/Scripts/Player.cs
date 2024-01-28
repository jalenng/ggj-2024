using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com.LuisPedroFonseca.ProCamera2D;

public class Player : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D body;
    protected AudioSource audioSource;
    protected BoxCollider2D col;

    [SerializeField] protected GameObject fart_emitter;

    [SerializeField] protected Sprite default_sprite;
    [SerializeField] protected Sprite nervous_sprite;
    [SerializeField] protected Sprite[] sprite_sizes;
    //[SerializeField] protected Sprite laughing_sprite;

    [SerializeField] protected Image fartBar;
    [SerializeField] protected Image tickleBar;

    //IEnumerator handle_fart;
    IEnumerator handle_tickle_cd;

    [SerializeField] protected bool alive = true;
    [SerializeField] protected bool farting = true;
    [SerializeField] protected bool canTickle = true;
    [SerializeField] protected bool activeFarting = false;

    [SerializeField] protected static float tickle_cooldown = .5f;
    [SerializeField] protected static float tickle_grav_scale = 0f;
    [SerializeField] protected static float fart_duration = 2f;
    [SerializeField] protected static float fart_grav_scale = .1f;
    [SerializeField] protected static float max_fart = 100f;

    //[SerializeField] protected float scaleChange = .02f; // 50 jumps = double size
    [SerializeField] protected float ticklePower = 10f;
    [SerializeField] protected float tickleMultiplier = 4f; // how much tickle power influences tickle meter filling up

    public float numJumps = 3f;
    public float numFarts = 1f;
    [SerializeField] protected float fartPower = 200f;
    [SerializeField] protected float tickleMeter = 0f;
    [SerializeField] protected float fartMeter = 0f;

    // SFX
    [SerializeField] protected List<AudioClip> fartAudioClips;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        body = this.GetComponent<Rigidbody2D>();
        audioSource = this.GetComponent<AudioSource>();
        col = this.GetComponent<BoxCollider2D>();
        alive = true;
        farting = false;

        //handle_fart = Farting(fart_duration);
        handle_tickle_cd = TickleCD(tickle_cooldown);

        numFarts = 1f;
        tickleMeter = 0;
        fartMeter = 0;
        fartBar.fillAmount = 0;
        tickleBar.fillAmount = 0;

        this.spriteRenderer.sprite = default_sprite;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (alive)
        {
            // if (tickleMeter > 0)
            // {
            //     tickleMeter -= 1f;
            //     tickleBar.fillAmount = tickleMeter / 100;
            // }

            // if (tickleMeter > 100)
            // {
            //     HandleDeath();
            // }

            if (farting)
            {
                if (fartMeter > 0)
                {

                    // reduce fart meter to 0 over 2 seconds
                    fartMeter = Mathf.Max(fartMeter - 1f, 0);
                    fartBar.fillAmount = fartMeter / 100;
                    UpdateSprite();

                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 diff = new Vector2(transform.position.x, transform.position.y) - mousePos;
                    diff.Normalize();

                    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    body.velocity = Vector3.zero; // zero out velocity (tweak this for jump feel)
                    body.AddForce(diff * fartPower);
                }
                else
                {
                    EndFart();
                }
            }
        }
        else if (fartMeter > 0)
        {
            // reduce fart meter to 0 over 2 seconds
            fartMeter = Mathf.Max(fartMeter - 1f, 0);
            fartBar.fillAmount = fartMeter / 100;

            UpdateSprite();
        }
    }


    // .. 
    // OnCollect() -- executes when player collects food
    // ..
    public void OnCollect(Collectable obj)
    {
        // only eat if meter not full
        if (alive && fartMeter < 100)
        {
            //Debug.Log("collected object " + obj.obj_name);
            UpdateFartMeter(obj.value);
            // Collectable handles Destroy
        }
    }


    // ..
    // OnTickle() -- AddForce to player based on mouse position; Increase Player size
    // .. 
    protected void OnTickle()
    {
        // disable tickling while farting or recent tickle
        if (!farting && canTickle)
        {
            //Debug.Log("click on object");

            // raycast to find angle of force
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
            if (hit)
            {
                Vector2 diff = new Vector2(transform.position.x, transform.position.y) - hit.point;
                //Debug.Log("point: " + hit.point + " => " + diff);
                diff.Normalize();

                body.velocity = Vector3.zero; // zero out velocity (tweak this for jump feel)

                body.AddForce(diff * ticklePower, ForceMode2D.Impulse);
            }

            // modify fartMeter
            //transform.localScale += Vector3.one * scaleChange;
            if (alive)
            {
                if (fartMeter == 100)
                    HandleDeath();
                //spriteRenderer.sprite = nervous_sprite;
                UpdateFartMeter(100 / numJumps);
                //UpdateTickleMeter(ticklePower * tickleMultiplier);
                //UpdateTickleMeter(100/numJumps);
            }

            StartCoroutine(handle_tickle_cd);
        }
    }


    // ..
    // OnFart() -- Add large force to player
    // ..
    protected void OnFart()
    {
        if (fartMeter == 100)
        {
            //Debug.Log("right click to fart");

            // handle timer for fart duration
            //StopCoroutine(handle_fart);
            farting = true;
            body.gravityScale = fart_grav_scale;
            // fart_emitter.SetActive(true);

            //spriteRenderer.sprite = nervous_sprite;

            // camera shake
            //StartCoroutine(Camera.main.GetComponent<CameraManager>().Shake(2f, .1f));
            //Camera.main.GetComponent<CameraManager>().shaking = true;
            ProCamera2DShake camshake = Camera.main.GetComponent<ProCamera2DShake>();
            camshake.ConstantShake(camshake.ConstantShakePresets[2]);
            // fart_emitter.GetComponent<Animator>().SetBool("Farting",true);
            fart_emitter.GetComponent<ParticleSystem>().Play();
            // Handle SFX
            int randIdx = Random.Range(0, fartAudioClips.Count);
            audioSource.clip = fartAudioClips[randIdx];
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.loop = true;
            audioSource.Play();

            //StartCoroutine(handle_fart);
            //handle_fart = Farting(fart_duration); // reset the iterator
        }
        else
        {
            Debug.Log("WARNING: Tried to fart with not enough gas.");
        }
    }

    //protected IEnumerator Farting(float val)
    protected void EndFart()
    {
        //Camera.main.GetComponent<CameraManager>().shaking = false;
        //Debug.Log("Finish Farting");
        // fart_emitter.GetComponent<Animator>().SetBool("Farting",false);
        fart_emitter.GetComponent<ParticleSystem>().Stop();
        farting = false;
        body.gravityScale = 1;
        body.velocity = Vector2.zero;
        //fart_emitter.SetActive(false);
        audioSource.Stop();

    }

    protected IEnumerator TickleCD(float cd)
    {
        // reset iterator
        handle_tickle_cd = TickleCD(tickle_cooldown);

        canTickle = false;
        body.gravityScale = fart_grav_scale;

        for (int i = 0; i < 5; i++)
        {
            body.velocity *= .8f;
            yield return new WaitForSeconds(.05f);
        }

        body.velocity = Vector2.zero;

        yield return new WaitForSeconds(.25f);

        canTickle = true;
        body.gravityScale = 1;
    }

    protected void UpdateSprite()
    {
        // fml
        // if (fartMeter < 10)
        //     spriteRenderer.sprite = sprite_sizes[0];
        // else if (fartMeter < 20)
        //     spriteRenderer.sprite = sprite_sizes[1];
        // else if (fartMeter < 30)
        //     spriteRenderer.sprite = sprite_sizes[2];
        // else if (fartMeter < 40)
        //     spriteRenderer.sprite = sprite_sizes[3];
        // else if (fartMeter < 50)
        //     spriteRenderer.sprite = sprite_sizes[4];
        // else if (fartMeter < 60)
        //     spriteRenderer.sprite = sprite_sizes[5];
        // else if (fartMeter < 70)
        //     spriteRenderer.sprite = sprite_sizes[6];
        // else if (fartMeter < 80)
        //     spriteRenderer.sprite = sprite_sizes[7];
        // else
        //     spriteRenderer.sprite = sprite_sizes[8];

        spriteRenderer.sprite = sprite_sizes[(int)Mathf.Min(fartMeter / 10f, 8f)];

        // change collider size based on sprite size
        col.size = spriteRenderer.bounds.size;
    }


    // ..
    // UpdateFartMeter() -- update fart bar and meter vals
    // ..
    protected void UpdateFartMeter(float val = 1)
    {
        if (fartMeter < 100)
        {
            fartMeter = Mathf.Min(fartMeter + val, max_fart);
            //transform.localScale = Vector3.one * (fartMeter/100 + 1);
            fartBar.fillAmount = fartMeter / 100;

            UpdateSprite();
        }


        // Automatic Fart
        if (!activeFarting && fartMeter >= 100)
            OnFart();
    }


    // ..
    // UpdateTickleMeter() -- update tickle bar
    // ..
    protected void UpdateTickleMeter(float val = 1)
    {
        tickleMeter += val;
        //
    }


    // ..
    // HandleDeath() -- what happens when player dies
    // ..
    protected void HandleDeath()
    {
        Debug.Log("DIE");
        alive = false;
        body.gravityScale = 1f;
        //OnFart();
        //spriteRenderer.sprite = default_sprite;

        // TEMP change color to indicate death
        spriteRenderer.color = new Color(204f, 241f, 142f, 1f);
    }


    // ..
    // OnMouseOver() -- handles various click events (tickle + fart)
    // ..
    void OnMouseOver()
    {
        // change animation when mouse is hovering over player
        if (alive && fartMeter < 10)
        {
            spriteRenderer.sprite = nervous_sprite;
        }

        // on right click
        if (Input.GetMouseButtonDown(0))
        {
            OnTickle();
        }

        // on left click
        if (Input.GetMouseButtonDown(1) && activeFarting)
        {
            OnFart();
        }
    }


    // ..
    // when mouse exits collider
    // ..
    void OnMouseExit()
    {
        // change sprite
        if (!farting && fartMeter < 10)
            this.spriteRenderer.sprite = default_sprite;
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        if (body.velocity.y < 0)
        {
            body.velocity = Vector2.zero;
            transform.rotation = Quaternion.identity;
            fartMeter = 0;
            fartBar.fillAmount = 0;
            UpdateSprite();

            EndFart();
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (body.velocity.y < 0)
        {
            body.velocity = Vector2.zero;
            transform.rotation = Quaternion.identity;
            fartMeter = 0;
            fartBar.fillAmount = 0;
            UpdateSprite();

            EndFart();
        }
    }

}
