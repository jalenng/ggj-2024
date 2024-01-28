using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D body;

    [SerializeField] protected GameObject fart_sprite;

    [SerializeField] protected Sprite default_sprite;
    [SerializeField] protected Sprite nervous_sprite;
    //[SerializeField] protected Sprite laughing_sprite;

    [SerializeField] protected Image fartBar;
    [SerializeField] protected Image tickleBar;

    IEnumerator handle_fart;
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

    [SerializeField] protected float fartPower = 300f;    
    [SerializeField] protected float tickleMeter = 0f;
    [SerializeField] protected float fartMeter = 0f;
    
    

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        body = this.GetComponent<Rigidbody2D>();
        alive = true;
        farting = false;

        handle_fart = Farting(fart_duration);
        handle_tickle_cd = TickleCD(tickle_cooldown);
        
        tickleMeter = 0;
        fartMeter = 0;
        fartBar.fillAmount = 0;
        tickleBar.fillAmount = 0;

        this.spriteRenderer.sprite = default_sprite;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (tickleMeter > 0)
        {
            tickleMeter -= 1f;
            tickleBar.fillAmount = tickleMeter / 100;
        }

        if (tickleMeter > 100)
        {
            HandleDeath();

        }

        // reduce fart meter to 0 over 2 seconds
        if (farting)// && transform.localScale.x > 1)
        {
            fartMeter = Mathf.Max (fartMeter - 1f, 0);
            //transform.localScale = Vector3.one * (fartMeter/100 + 1);
            fartBar.fillAmount = fartMeter / 100;


            // continuous raycast for automatic movement during fart
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
            if (hit)
            {
                Vector2 diff = new Vector2(transform.position.x, transform.position.y) - hit.point;
                //Debug.Log("point: " + hit.point + " => " + diff);
                diff.Normalize();

                float angle = Mathf.Atan2(diff.y,diff.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                
                body.velocity = Vector3.zero; // zero out velocity (tweak this for jump feel)
                
                body.AddForce(diff * fartPower);
            }
        }

    }


    // .. 
    // OnCollect() -- executes when player collects food
    // ..
    public void OnCollect(Collectable obj)
    {
        // only eat if meter not full
        if(alive && fartMeter < 100)
        {
            //Debug.Log("collected object " + obj.obj_name);
            UpdateFartMeter(obj.value);
            Object.Destroy(obj.gameObject);
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
                spriteRenderer.sprite = nervous_sprite;
                UpdateFartMeter(ticklePower);
                UpdateTickleMeter(ticklePower * tickleMultiplier);
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
            
            //
            // TO IMPLEMENT --- 
            farting = true;
            body.gravityScale = fart_grav_scale;
            fart_sprite.SetActive(true);
            spriteRenderer.sprite = nervous_sprite;

            StartCoroutine(Camera.main.GetComponent<CameraManager>().Shake(2f, .2f));

            // handle timer for fart duration
            StopCoroutine(handle_fart);
            StartCoroutine(handle_fart);
        }
        else {
            Debug.Log("WARNING: Tried to fart with not enough gas.");
        }
    }

    protected IEnumerator Farting(float val)
    {
        //Debug.Log("Start Farting");

        // reset the iterator
        handle_fart = Farting(fart_duration);
        
        yield return new WaitForSeconds(val);
        
        //Debug.Log("Finish Farting");        
        farting = false;
        body.gravityScale = 1;
        fart_sprite.SetActive(false);

    }

    protected IEnumerator TickleCD(float cd)
    {
        // reset iterator
        handle_tickle_cd = TickleCD(tickle_cooldown);

        canTickle = false;
        body.gravityScale = fart_grav_scale;
        
        for(int i = 0; i < 5; i++)
        {
            body.velocity *= .8f;
            yield return new WaitForSeconds(.05f);
        }

        body.velocity = Vector2.zero;

        yield return new WaitForSeconds(.25f);
        
        canTickle = true;
        body.gravityScale = 1;
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
    protected void HandleDeath(){
        Debug.Log("DIE");
        alive = false;
        //OnFart();
        spriteRenderer.sprite = default_sprite;
    }


    // ..
    // OnMouseOver() -- handles various click events (tickle + fart)
    // ..
    void OnMouseOver()
    {
        // change animation when mouse is hovering over player
        if (alive)
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
        if (farting)
            this.spriteRenderer.sprite = default_sprite;
    }

    
}
