using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public float speed = 1;
    public List<Transform> points;
    private bool isMoving = false;
    private Vector3 destPos;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, destPos, Time.deltaTime * speed);
        }
    }

    public IEnumerator MoveCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Debug.Log("move");
                yield return new WaitForSeconds(3f);
                yield return MoveToPointCoroutine(points[i].position);
            }
        }
    }

    public IEnumerator MoveToPointCoroutine(Vector3 point)
    {
        isMoving = true;
        destPos = point;
        GetComponent<Animator>().SetBool("flying", true);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, destPos) < 0.1f);
        GetComponent<Animator>().SetBool("flying", false);
    }
}
