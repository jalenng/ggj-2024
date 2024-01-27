using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;
    
    [SerializeField] protected Sprite default_sprite;
    [SerializeField] protected Sprite nervous_sprite;
    [SerializeField] protected float ticklePower = 3f;
    [SerializeField] protected float scaleChange = .05f; // 20 jumps = double size
    //[]
    protected Rigidbody2D body;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        body = this.GetComponent<Rigidbody2D>();
        this.spriteRenderer.sprite = default_sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // OnTickle() -- AddForce to player based on mouse position; Increase Player size
    void OnTickle()
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
            
            body.velocity = Vector3.zero; // zero out velocity (might tweak this for jump feel)
            
            body.AddForce(diff * ticklePower, ForceMode2D.Impulse);
        }

        // modify scale
        transform.localScale += Vector3.one * scaleChange;
    }

    // OnFart() -- Add large force to player
    void OnFart()
    {
        Debug.Log("right click");
    }


    void OnMouseOver()
    {
        // change animation for hovering
        spriteRenderer.sprite = nervous_sprite;

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


    // when mouse exits collider
    void OnMouseExit()
    {
        // change sprite
        this.spriteRenderer.sprite = default_sprite;
    }

}
