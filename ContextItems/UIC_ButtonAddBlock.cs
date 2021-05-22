using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIC_ButtonAddBlock : MonoBehaviour, I_UIC_ContextItem, I_UIC_Object
{
    Button _button;
    public UIC_Entity entity;
    public string ID => "button";
    public Color objectColor { get; set; }
    public int Priority => -10;

    void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Action()
    {
        UIC_Manager.InstantiateEntityAtPosition(entity, entity.transform.position + new Vector3(15,15,0));
    }

    public void OnChangeSelection()
    {
        
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
