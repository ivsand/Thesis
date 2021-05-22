using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIC_ObjectName : MonoBehaviour, I_UIC_ContextItem
{
    Text _text;

    public void Action()
    {

    }

    public void OnChangeSelection()
    {
        if (UIC_Manager.selectedUIObjectsList.Count <= 0)
        {
            _text.text = "--";
        }
        else if (UIC_Manager.selectedUIObjectsList.Count == 1)
        {
            _text.text = (UIC_Manager.selectedUIObjectsList[0] as I_UIC_Object).ID;
        }
        else
        {
            _text.text = string.Format("Multiple Objects ({0})", UIC_Manager.selectedUIObjectsList.Count);
        }

    }

    void Awake()
    {
        _text = transform.GetComponentInChildren<Text>();
    }
}
