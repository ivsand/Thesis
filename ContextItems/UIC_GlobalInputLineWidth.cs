using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIC_GlobalInputLineWidth : MonoBehaviour, I_UIC_ContextItem, I_UIC_Object
{
    InputField _inputField;

    public string ID => throw new NotImplementedException();

    public Color objectColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int Priority => -10;

    public void Action()
    {

    }

    public void OnChangeSelection()
    {
        _inputField.text = UIC_Manager.Instance.globalLineWidth.ToString();
    }

    void Awake()
    {
        _inputField = GetComponentInChildren<InputField>();
    }

    private void SetWidth(string arg0)
    {
        UIC_Manager.Instance.globalLineWidth = int.Parse(_inputField.text);
    }

    void OnEnable()
    {
        _inputField.onValueChanged.AddListener(SetWidth);
    }

    void OnDisable()
    {
        _inputField.onValueChanged.RemoveAllListeners();
    }

    public void Remove()
    {
        throw new NotImplementedException();
    }
}
