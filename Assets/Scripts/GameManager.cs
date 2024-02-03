using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    protected GameObject playerPrefab;

    public List<GameObject> players;

    // Start is called before the first frame update
    void Start()
    {
        players = new List<GameObject>();
        int i = 0;
        foreach(InputDevice device in InputSystem.devices)
        {
            if (device.description.deviceClass != "Keyboard" &&
                device.description.deviceClass != "Mouse")
            {
                Debug.Log("dev id: " + device.deviceId);
                GameObject newplayer = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                newplayer.GetComponentInChildren<PlayerController>().gamepad_id = device.deviceId;
                players.Add(newplayer);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
