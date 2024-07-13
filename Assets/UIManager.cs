using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TouchManager touchManager;

    void Start (){
        touchManager = GetComponent<TouchManager>();
    }

    public void Delete(){

        Debug.Log("Delete");
        touchManager.singleSelectUI.transform.SetParent(null);
        GameObject.Destroy(touchManager.selectedObjects[0]);
        touchManager.OnCellDeselect();
    }

    public void Rename(){

        Debug.Log("Rename");
        
        TMP_InputField inputField = touchManager.selectedObjects[0].GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.interactable = true;
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    public void Connecting(){

        Debug.Log("Connect");
        touchManager.CreateConnection();
    }
}
