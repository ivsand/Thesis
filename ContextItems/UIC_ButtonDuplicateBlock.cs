using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIC_ButtonDuplicateBlock : MonoBehaviour, I_UIC_ContextItem, I_UIC_Object
{
    Button _button;
    public string ID => "button";
    public Color objectColor { get; set; }
    public int Priority => -10;

    void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Action()
    {
        for (int i = UIC_Manager.selectedUIObjectsList.Count - 1; i >= 0; i--)
        {
            UIC_Entity entityToDuplicate = UIC_Manager.selectedUIObjectsList[i] as UIC_Entity;
            if (entityToDuplicate)
            {
                UIC_Manager.InstantiateEntityAtPosition(entityToDuplicate, entityToDuplicate.transform.position + new Vector3(15, -15));
            }
        }
        UIC_ContextMenu.UpdateContextMenu();
    }

    public void OnChangeSelection()
    {
        gameObject.SetActive(UIC_Manager.selectedUIObjectsList.Count > 0);
    }

    void OnEnable()
    {
        _button.onClick.AddListener(Action);
    }

    void OnDisable()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
