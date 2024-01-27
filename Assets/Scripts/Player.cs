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

    public IEnumerator handle_fart;
    [SerializeField] protected bool alive = true;
    [SerializeField] protected bool farting = true;
    [SerializeField] protected float ticklePower = 4f;

    [SerializeField] protected float fartPower = 100f;
    [SerializeField] protected float fart_duration = 2f;
    [SerializeField] protected float scaleChange = .02f; // 50 jumps = double size

    [SerializeField] protected float tickleMeter = 0f;
    [SerializeField] protected float fartMeter = 0f;
    [SerializeField] protected static float max_fart = 100f;
    
    

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        body = this.GetComponent<Rigidbody2D>();
        alive = true;
        farting = false;

        handle_fart = Farting(fart_duration);
        
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
        if (farting && transform.localScale.x > 1)
        {
            fartMeter-= 0.75f;
            transform.localScale = Vector3.one * (fartMeter/100 + 1);
            fartBar.fillAmount = fartMeter / 100;
        }

    }


    // .. 
    // OnCollect() -- executes when player collects food
    // ..
    public void OnCollect(Collectable obj)
    {
        if(alive)
        {
            Debug.Log("collected object " + obj.obj_name);
            UpdateFartMeter(obj.value);
            Object.Destroy(obj.gameObject);
        }
    }


    // ..
    // OnTickle() -- AddForce to player based on mouse position; Increase Player size
    // .. 
    protected void OnTickle()
    {
        // disable tickling while farting
        if (!farting)
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
                UpdateTickleMeter(ticklePower * 5);
            }
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
            fart_sprite.SetActive(true);
            spriteRenderer.sprite = nervous_sprite;


            
            // handle timer for fart duration
            StartCoroutine(handle_fart);
        }
        else {
            Debug.Log("WARNING: Tried to fart with not enough gas.");
        }
    }

    protected IEnumerator Farting(float val)
    {
        yield return new WaitForSeconds(val);
        farting = false;
        fart_sprite.SetActive(false);
    }


    // ..
    // UpdateFartMeter() -- update fart bar and meter vals
    // ..
    protected void UpdateFartMeter(float val = 1)
    {
        if (fartMeter < 100)
        {
            fartMeter = Mathf.Min(fartMeter + val, max_fart);
            //transform.localScale += Vector3.one * val *  scaleChange;
            transform.localScale = Vector3.one * (fartMeter/100 + 1);
            fartBar.fillAmount = fartMeter / 100;
        }
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
        spriteRenderer.sprite = nervous_sprite;
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
        if (Input.GetMouseButtonDown(1))
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
