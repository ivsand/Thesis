using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIC_DropdownGlobalLineType : MonoBehaviour, I_UIC_ContextItem, I_UIC_Object
{
    Dropdown _dropdown;

    public string ID => "dropdown";
    public Color objectColor { get; set; }

    public int Priority => -9;

    public void Action()
    {

    }

    public void OnChangeSelection()
    {

    }

    private void SetLineType(int arg0)
    {
        UIC_Connection.LineTypeEnum parsed_enum = (UIC_Connection.LineTypeEnum)System.Enum.Parse(typeof(UIC_Connection.LineTypeEnum), _dropdown.options[_dropdown.value].text);
        UIC_Manager.Instance.globalLineType = parsed_enum;
    }

    void SetDropdown()
    {
        _dropdown.value = _dropdown.options.FindIndex(option => option.text == UIC_Manager.Instance.globalLineType.ToString());
    }

    void OnEnable()
    {
        SetDropdown();
        _dropdown.onValueChanged.AddListener(SetLineType);
    }

    void OnDisable()
    {
        _dropdown.onValueChanged.RemoveAllListeners();
    }

    void Awake()
    {
        _dropdown = GetComponent<Dropdown>();
    }

    void Start()
    {
        string[] _names = System.Enum.GetNames(typeof(UIC_Connection.LineTypeEnum));
        List<string> _options = new List<string>();
        _options.AddRange(_names);

        _dropdown.options.Clear();
        _dropdown.AddOptions(_options);
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
