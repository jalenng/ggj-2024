using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPlayer : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject clickInstruction;
    private Rigidbody2D rb2d;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.angularVelocity = 50f;
        rb2d.velocity = Random.insideUnitCircle.normalized * 5f;

        player = GetComponent<Player>();
        player.fartMeter = 100;
        player.UpdateSprite();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Handle bouncing at edge of viewport
        bool willBounce = false;
        Vector2 velocity = rb2d.velocity;
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1)
        {
            velocity.x *= -1;
            willBounce = true;
        }
        if (viewportPos.y < 0 || viewportPos.y > 1)
        {
            velocity.y *= -1;
            willBounce = true;
        }
        if (willBounce)
        {
            velocity = Quaternion.Euler(0, 0, Random.Range(-1f, 1f)) * velocity;
        }
        rb2d.velocity = velocity;
    }
    public void Shrink()
    {
        GetComponent<Animator>().SetTrigger("shrink");
    }
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            StartCoroutine(FartCoroutine());
        }
    }
    public IEnumerator FartCoroutine()
    {
        clickInstruction.SetActive(false);
        player.fartMeter = 100;
        player.OnFart(true);

        // Wait for anim before loading 1st lvl
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(1);
    }
}
