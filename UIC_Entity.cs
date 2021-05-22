using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIC_Entity : MonoBehaviour, I_UIC_Object, I_UIC_Selectable, I_UIC_Draggable, I_UIC_Clickable
{
    [Header("Logic")]
    [SerializeField]
    bool _enableDrag;
    public bool EnableDrag { get => _enableDrag; set => _enableDrag = value; }
    public bool enableSelfConnection;

    RectTransform _rectTransform;

    [Header("Elements")]
    public List<UIC_Node> nodeList;

  

    public string ID => name;

    public Vector3[] Handles { get; set; }
    public Color objectColor { get => _image.color; set => _image.color = value; }

    public int Priority => 0;

    public bool DisableClick { get; set; }

    Image _image;

    void Awake()
    {
        DisableClick = false;
        _image = GetComponent<Image>();
    }

    void OnValidate()
    {
        Init();
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {

    _rectTransform = _rectTransform ? _rectTransform : GetComponent<RectTransform>();

        nodeList = new List<UIC_Node>();
        nodeList.AddRange(transform.GetComponentsInChildren<UIC_Node>());
    }

    public void UpdateConnections()
    {
        foreach (UIC_Node node in nodeList)
        {
            foreach (UIC_Connection conn in node.connectionsList)
            {
                conn.UpdateLine();
            }
        }
    }

    public void Select()
    {
        if (!UIC_Manager.selectedUIObjectsList.Contains(this))
        {
            UIC_Manager.selectedUIObjectsList.Add(this);
        }
    }

    public void Unselect()
    {
        if (UIC_Manager.selectedUIObjectsList.Contains(this))
        {
            UIC_Manager.selectedUIObjectsList.Remove(this);
        }
    }

    public void Remove()
    {
        Unselect();

        foreach (UIC_Node node in nodeList)
        {
            node.RemoveAllConnections();
        }

        if (UIC_Manager.EntityList.Contains(this))
        {
            UIC_Manager.EntityList.Remove(this);
        }

        Destroy(gameObject);
    }

    public void OnPointerDown()
    {
        if (!UIC_Manager.selectedUIObjectsList.Contains(this))
        {
            Select();
        }
        else
        {
            Unselect();
        }
    }

    public void OnPointerUp()
    {
        // v1.2 - using static Manager instance to set parent of dropped entity
		transform.SetParent(UIC_Manager.Instance.transform);
        UpdateConnections();
    }

    public void OnDrag()
    {
        if (EnableDrag)
        {
            Select();
            transform.SetParent(UIC_Pointer.Instance.transform.GetChild(0));

            UpdateConnections();
        }
    }
    

    public List<UIC_Entity> GetConnectedEntities()
    {
        List<UIC_Entity> connectedEntities = new List<UIC_Entity>();
        foreach (UIC_Node node in nodeList)
        {
            foreach (UIC_Connection conn in node.connectionsList)
            {
                UIC_Entity connEntitiy = conn.node0 != node ? conn.node0.entity : conn.node1.entity;
                connectedEntities.Add(connEntitiy);
            }
        }
        return connectedEntities;
    }
}
