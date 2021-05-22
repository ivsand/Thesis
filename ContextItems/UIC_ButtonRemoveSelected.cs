using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIC_ButtonRemoveSelected : MonoBehaviour, I_UIC_ContextItem, I_UIC_Object
{
    Button _button;
    public string ID => "button";
    public Color objectColor { get; set; }
    public int Priority => -10;

    public void Action()
    {
        for (int i = UIC_Manager.selectedUIObjectsList.Count - 1; i >= 0; i--)
        {
            UIC_Manager.selectedUIObjectsList[i].Remove();
        }
        UIC_ContextMenu.UpdateContextMenu();
    }

    void Awake()
    {
        _button = GetComponent<Button>();
    }

    void OnEnable()
    {
        _button.onClick.AddListener(Action);
    }

    void OnDisable()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void OnChangeSelection()
    {
        gameObject.SetActive(UIC_Manager.selectedUIObjectsList.Count > 0);
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
