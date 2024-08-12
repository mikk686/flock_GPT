using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFieldController : MonoBehaviour
{


    TMP_InputField field;
    public FlockController FlockController;

    private void Start()
    {
        field = GetComponent<TMP_InputField>();
        Debug.Log(field);
    }



    public void OnClick()
    {
        var x = int.Parse(field.text);
        FlockController.m_numberOfDrones = x;
        Debug.Log(FlockController.m_numberOfDrones);
    }
 
}
