using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{

    // default cursor (feather)
    public static Texture2D cursor1;
    
    // offset for cursor focus point (top left corner)
    public static Vector2 offset = Vector2.zero;

    void Awake()
    {
        cursor1 = Resources.Load("feather_cursor.png") as Texture2D;
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
