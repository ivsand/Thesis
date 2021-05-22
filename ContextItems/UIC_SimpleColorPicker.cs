using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIC_SimpleColorPicker : MonoBehaviour, I_UIC_ContextItem, I_UIC_Object
{
    Transform _colorsPanel;
    Button[] _colorButtons;

    public string ID => "color picker";
    public Color objectColor { get; set; }

    public int Priority => -10;

    public void Action()
    {

    }

    public void OnChangeSelection()
    {
        gameObject.SetActive(UIC_Manager.selectedUIObjectsList.Count > 0);
    }

    void SetColor(Button _button)
    {
        foreach (I_UIC_Object obj in UIC_Manager.selectedUIObjectsList)
        {
            obj.objectColor = _button.image.color;
        }
    }

    void Awake()
    {
        _colorsPanel = transform.GetChild(1);
        _colorButtons = new Button[_colorsPanel.childCount];

        for (int i = 0; i < _colorButtons.Length; i++)
        {
            _colorButtons[i] = _colorsPanel.GetChild(i).GetComponent<Button>();
        }
    }

    void OnEnable()
    {
        foreach (Button button in _colorButtons)
        {
            button.onClick.AddListener(delegate { SetColor(button); });
        }
    }

    void OnDisable()
    {
        foreach (Button button in _colorButtons)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
