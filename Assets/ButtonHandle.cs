using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandle : MonoBehaviour
{
    FlockController controller;
    bool switcher = false;

    private void Start()
    {
; 
       controller = GameObject.Find("DroneManager").GetComponent<FlockController>(); 
    }
    public void Click()
    {
        if (!switcher)
        {
            switcher = true;
            controller.Spawn();
            
        }
        else
        {
            switcher = false;
            controller.DeSpawn();
            
        }
    }
}
