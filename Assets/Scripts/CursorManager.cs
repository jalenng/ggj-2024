using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{

    // default cursor (feather)
    public Texture2D cursor1;
    public Texture2D cursor2;
    
    // offset for cursor focus point (top left corner)
    public Vector2 offset = Vector2.zero;

    void Awake()
    {
        cursor1 = Resources.Load("feather1") as Texture2D;
        cursor2 = Resources.Load("feather2") as Texture2D;
    }
    // Start is called before the first frame update
    void Start()
    {   
        Cursor.SetCursor(cursor1, offset, CursorMode.Auto);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



}
