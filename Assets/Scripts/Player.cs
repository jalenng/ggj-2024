using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Image fart_indicator;
    protected Rigidbody2D body;
    protected AudioSource audioSource;
    protected CapsuleCollider2D col;
    
    public bool grounded = false;

    [SerializeField] LayerMask groundmask;

    [SerializeField] protected GameObject fart_emitter;

    [SerializeField] protected Sprite default_sprite;
    [SerializeField] protected Sprite nervous_sprite;
    [SerializeField] protected Sprite[] sprite_sizes;
    //[SerializeField] protected Sprite laughing_sprite;

    [SerializeField] protected Image fartBar;
    [SerializeField] protected Image tickleBar;

    //IEnumerator handle_fart;
    IEnumerator handle_tickle_cd;
    Vector2 respawnPos;
    public  bool alive = true;


    [SerializeField] protected bool farting = false;
    [SerializeField] protected bool deflating = false;
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
    [SerializeField] protected float deflate_rate = 1f;
    [SerializeField] protected float fartPower = 200f;
    [SerializeField] protected float tickleMeter = 0f;
    //[SerializeField] protected float fartMeter = 0f;
    public float fartMeter = 0f;

    public bool disableClick = false;

    // SFX
    [SerializeField] protected List<AudioClip> fartAudioClips;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        body = this.GetComponent<Rigidbody2D>();
        audioSource = this.GetComponent<AudioSource>();
        col = this.GetComponent<CapsuleCollider2D>();
        alive = true;
        deflating = false;
        farting = false;
        grounded = false;

        //handle_fart = Farting(fart_duration);
        handle_tickle_cd = TickleCD(tickle_cooldown);

        numFarts = 1f;
        tickleMeter = 0;
        fartMeter = 0;
        if (fartBar != null) fartBar.fillAmount = 0;
        if (tickleBar != null) tickleBar.fillAmount = 0;

        this.spriteRenderer.sprite = default_sprite;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (deflating)
        {
            // check for fart meter size
            if (fartMeter > 0)
            {
                // reduce fart meter and update sprite (rate is 50/s from FixedUpdate)
                fartMeter = Mathf.Max(fartMeter - deflate_rate, 0);
                UpdateSprite();

                if (fartBar != null) 
                    fartBar.fillAmount = fartMeter / 100;

                // update movement if farting
                if (farting)
                {
                    // reduce fart meter to 0 over 2 seconds
                    fartMeter = Mathf.Max(fartMeter - 1f, 0);
                    if (fartBar != null) fartBar.fillAmount = fartMeter / 100;
                    UpdateSprite();

                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 diff = new Vector2(transform.position.x, transform.position.y) - mousePos;
                    diff.Normalize();

                    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    body.velocity = Vector3.zero; // zero out velocity (tweak this for jump feel)
                    body.AddForce(diff * fartPower);
                }
            }
            else 
            {
                if (farting)
                {
                    EndFart();
                }
                deflating = false;
            }
        }
        
        if (!grounded && alive)
        {
            Vector2 point = new Vector2(transform.position.x - col.size.x/2, transform.position.y - col.size.y/2);
            RaycastHit2D hit = Physics2D.Raycast(point, Vector2.down, 1f, groundmask);

            
            Vector2 point2 = new Vector2(transform.position.x + col.size.x/2, transform.position.y - col.size.y/2);
            RaycastHit2D hit2 = Physics2D.Raycast(point2, Vector2.down, 1f, groundmask);

            //Debug.Log(col.size.x + ", " + col.size.y);
            //Debug.Log(point);

            if ((hit.collider && hit.distance <= 0.1f) || (hit2.collider && hit2.distance <= 0.1f))
            {
                
                Ground();
            }
        }
    }

    void Ground()
    {
        grounded = true;
        //Debug.Log(":GROUND");

        body.velocity = Vector2.zero;
        transform.rotation = Quaternion.identity;

        respawnPos = transform.position;

        deflate_rate = 20f;
        deflating = true;
        
        if (farting)
            EndFart();

        numFarts = 1;
        
        Color temp = fart_indicator.color;
        temp.a = 1f;
        fart_indicator.color = temp;

    }

    // .. 
    // OnCollect() -- executes when player collects food
    // ..
    public void OnCollect(Collectable obj)
    {
        // only eat if meter not full
        if (alive)
        {
            //Debug.Log("collected object " + obj.obj_name);
            UpdateFartMeter(obj.value);
            UpdateSprite();

            numFarts = 1;
            
            Color temp = fart_indicator.color;
            temp.a = 1f;
            fart_indicator.color = temp;
            // Collectable handles Destroy
        }
    }


    // ..
    // OnTickle() -- AddForce to player based on mouse position; Increase Player size
    // .. 
    protected void OnTickle()
    {
        // disable tickling while farting or recent tickle
        
        if (canTickle)
        {
            if (farting )
            {
                EndFart();
                deflating = false;
            }
            else if (!farting && deflating)
            {
                deflating = false;
                fartMeter = 0;
                UpdateSprite();
            }
            
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
                if (fartMeter + 100/numJumps > 100)
                    HandleDeath();
                else
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
    public void OnFart(bool overrideCheck = false)
    {
        //if (fartMeter == 100)
        if (overrideCheck || (numFarts > 0 && fartMeter > 0) && !farting)
        {
            //Debug.Log("right click to fart");
            // handle timer for fart duration
            //StopCoroutine(handle_fart);
            farting = true;
            deflate_rate = 1f;
            deflating = true;
            body.gravityScale = fart_grav_scale;

            // fart_emitter.SetActive(true);

            numFarts = 0;

            Color temp = fart_indicator.color;
            temp.a = 0.5f;
            fart_indicator.color = temp;

            //spriteRenderer.sprite = nervous_sprite;

            // camera shake
            //StartCoroutine(Camera.main.GetComponent<CameraManager>().Shake(2f, .1f));
            //Camera.main.GetComponent<CameraManager>().shaking = true;
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("MainMenu"))
            {
                ProCamera2DShake camshake = Camera.main.GetComponent<ProCamera2DShake>();
                camshake.ConstantShake(camshake.ConstantShakePresets[2]);
            }
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
            Debug.Log("WARNING: NO FARTS LEFT");
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

        grounded = false;

        yield return new WaitForSeconds(.2f);

        canTickle = true;
        body.gravityScale = 1;
    }

    public void UpdateSprite()
    {

        spriteRenderer.sprite = sprite_sizes[ (int)Mathf.Min(fartMeter / 10f, 8f)];

        // change collider size based on sprite size
        //col.size = spriteRenderer.bounds.size;
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
    public void HandleDeath()
    {
        Debug.Log("DIE");
        alive = false;
        body.gravityScale = 1f;

        // TEMP DEATH; REPLACE THIS WITH ANIMATION LATER
        //deflating = true;
        //spriteRenderer.color = new Color(204f, 241f, 142f, 1f);
        GetComponent<Animator>().enabled =true;
        GetComponent<Animator>().Play("exploding");
        
        fartMeter = 0;
        fartBar.fillAmount = 0;
        
        StartCoroutine("Respawn", .5f);
    }

    public IEnumerator Respawn(float delay)
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Collectable"))
        {
            g.GetComponent<Collectable>().Respawn(delay);
        }
        yield return new WaitForSeconds(delay);
        
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(delay);
        
        transform.position = respawnPos;
        spriteRenderer.enabled = true;
        

        alive = true;
        GetComponent<Animator>().enabled = false;

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

        if (!disableClick) {
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



}
