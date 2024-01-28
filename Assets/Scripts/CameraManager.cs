using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public bool shaking = false;
    public Vector3 orignalPosition;
    public float magnitude = .1f;

    
    // Start is called before the first frame update
    void Start()
    {
        shaking = false;
        orignalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (shaking)
        {   
            float x = orignalPosition.x + Random.Range(-1f, 1f) * magnitude;
            float y = orignalPosition.y + Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, y, -10f);
        }
        else 
        {
            transform.position = orignalPosition;
        }
    }
    /*
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 orignalPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = orignalPosition.x + Random.Range(-1f, 1f) * magnitude;
            float y = orignalPosition.y + Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, y, -10f);
            elapsed += Time.deltaTime;
            yield return 0;
        }
        transform.position = orignalPosition;
    }*/

}
