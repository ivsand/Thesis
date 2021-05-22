using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIC_Node : MonoBehaviour, I_UIC_Object, I_UIC_Draggable, I_UIC_Clickable
{
    public Type type;
    public enum PolarityTypeEnum { _in, _out, _all };
    [Header("Logic")]
    public PolarityTypeEnum polarityType;
    [HideInInspector]
    public UIC_Entity entity;

    public int maxConnections = 0; // 0 - no limit
    
    [Header("Visuals")]
    public Sprite iconUnconnected;
    public Sprite iconConnected;

    public Color iconColorDefault;
    public Color iconColorHover;
    public Color iconColorSelected;
    public Color iconColorConnected;

    Text text;
    Image imageCurrentIcon;

    [Header("Elements")]
    public Transform spLineControlPointTranform;

    public bool haveSpots
    {
        get
        {
            return maxConnections == 0 || connectionsList.Count < maxConnections;
        }
    }

    public string ID => string.Format("{0}'s Node ({1})", entity.name, name);

    public Color objectColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int Priority => 2;

    public bool EnableDrag { get; set; }

    public bool DisableClick => false;

    UIC_Line connectionUILine;
    UISpline connectionUISpline;

    UIC_LineRenderer uILineRenderer;

    [HideInInspector]
    public UIC_Node lastFoundNode;
    UIC_Node closestFoundNode;

    public List<UIC_Connection> connectionsList;

    void OnValidate()
    {
        Init();
    }

    void Awake()
    {

    }

    void Start()
    {
        Init();

        imageCurrentIcon.color = iconColorDefault;
        connectionsList = new List<UIC_Connection>();

        connectionUILine = new UIC_Line();
    }

    public void Init()
    {
        text = transform.GetChild(0).GetComponent<Text>();
        imageCurrentIcon = transform.GetChild(0).GetComponent<Image>();
        spLineControlPointTranform = transform.GetChild(1);

        //int multiplier = 1;
        if (polarityType == PolarityTypeEnum._in)
        {
            name = "In Node";
        }
        else if (polarityType == PolarityTypeEnum._out)
        {
            name = "Out Node";
        }
        else if (polarityType == PolarityTypeEnum._all)
        {
            name = "In-Out Node";
        }

        imageCurrentIcon.sprite = iconUnconnected;

        entity = GetParentEntity(transform);
        if (entity != null && !entity.nodeList.Contains(this))
            entity.nodeList.Add(this);

        connectionUISpline = new UISpline();

        uILineRenderer = FindObjectOfType<UIC_LineRenderer>();
    }

    UIC_Entity GetParentEntity(Transform child)
    {
        if (child && child.gameObject.activeSelf)
        {
            UIC_Entity parentEntity = GetComponentInParent<UIC_Entity>();
            if (parentEntity != null)
                return parentEntity;
            else
                return GetParentEntity(child.parent);
        }
        else return null;
    }

    public void SetIcon()
    {
        if (connectionsList.Count > 0)
        {
            imageCurrentIcon.sprite = iconConnected;
            imageCurrentIcon.color = iconColorConnected;
        }
        else
        {
            imageCurrentIcon.sprite = iconUnconnected;
            imageCurrentIcon.color = iconColorDefault;
        }
    }

    // v1.2.1 - UIC_Node.ConnectTo return UIC_Connection
    public UIC_Connection ConnectTo(UIC_Node otherNode)
    {
        UIC_Connection _connection = null;

        if (otherNode.haveSpots && this.haveSpots)
        {
            _connection = UIC_Manager.AddConnection(this, otherNode, UIC_Manager.Instance.globalLineType);
            otherNode.SetIcon();
        }
        SetIcon();

        return _connection;
    }

    public void Remove()
    {
        RemoveAllConnections();
        entity.nodeList.Remove(this);
        Destroy(gameObject);
    }

    public void RemoveAllConnections()
    {
        for (int i = connectionsList.Count - 1; i >= 0; i--)
        {
            connectionsList[i].Remove();
        }

        SetIcon();
    }

    bool _pointerDown = false;
    public void OnPointerDown()
    {
        _pointerDown = true;
        imageCurrentIcon.color = iconColorSelected;
        UIC_Manager.AddLine(connectionUILine);

        connectionUILine.width = UIC_Manager.Instance.globalLineWidth;
        connectionUILine.defaultColor = UIC_Manager.Instance.globalLineDefaultColor;
        connectionUILine.color = UIC_Manager.Instance.globalLineDefaultColor;

        connectionUILine.SetCap(UIC_Line.CapIDEnum.Start, UIC_Manager.Instance.globalCapStartType, UIC_Manager.Instance.globalCapStartSize,
                            UIC_Manager.Instance.globalCapStartColor, UIC_Manager.Instance.globalCapStartAngleOffset);
        connectionUILine.SetCap(UIC_Line.CapIDEnum.End, UIC_Manager.Instance.globalCapEndType, UIC_Manager.Instance.globalCapEndSize,
                            UIC_Manager.Instance.globalCapEndColor, UIC_Manager.Instance.globalCapEndAngleOffset);
    }

    public void OnPointerUp()
    {
        _pointerDown = false;
        imageCurrentIcon.color = iconColorDefault;
        connectionUILine.points.Clear();
        UIC_Manager.RemoveLine(connectionUILine);

        closestFoundNode = UIC_Pointer.FindClosestNodeOfOppositPolarity(UIC_Pointer.Instance.transform.position, UIC_Manager.Instance.MaxPointerDetectionDistance, this);
        if (closestFoundNode)
        {
            closestFoundNode.imageCurrentIcon.color = closestFoundNode.iconColorDefault;
            ConnectTo(closestFoundNode);
        }

        SetIcon();

        lastFoundNode = null;
    }

    // v1.3 - line poinst adjusted to canvas render mode overlay and camera
    Vector3[] LinePoints
    {
        get
        {
            if (UIC_Manager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
            {
                return new Vector3[] {
                    transform.position,
                    spLineControlPointTranform.position,
                    closestFoundNode ? closestFoundNode.spLineControlPointTranform.position : UIC_Pointer.PointerPosition,
                    closestFoundNode ? closestFoundNode.transform.position : UIC_Pointer.PointerPosition };
            }
            else if (UIC_Manager.CanvasRenderMode == RenderMode.ScreenSpaceCamera)
            {
                return new Vector3[] {
                    UIC_Manager.mainCamera.WorldToScreenPoint(transform.position),
                    UIC_Manager.mainCamera.WorldToScreenPoint(spLineControlPointTranform.position),
                    closestFoundNode ? UIC_Manager.mainCamera.WorldToScreenPoint(closestFoundNode.spLineControlPointTranform.position) : UIC_Pointer.PointerPosition,
                    closestFoundNode ? UIC_Manager.mainCamera.WorldToScreenPoint(closestFoundNode.transform.position) : UIC_Pointer.PointerPosition };
            }

            return new Vector3[] {
                transform.position,
                spLineControlPointTranform.position,
                closestFoundNode ? closestFoundNode.spLineControlPointTranform.position : UIC_Pointer.PointerPosition,
                closestFoundNode ? closestFoundNode.transform.position : UIC_Pointer.PointerPosition };
        }
    }

    public void OnDrag()
    {
        if (lastFoundNode)
            lastFoundNode.SetIcon();

        closestFoundNode = UIC_Pointer.FindClosestNodeOfOppositPolarity(UIC_Pointer.Instance.transform.position, UIC_Manager.Instance.MaxPointerDetectionDistance, this);

        if (closestFoundNode)
        {
            closestFoundNode.imageCurrentIcon.color = closestFoundNode.iconColorSelected;
            lastFoundNode = closestFoundNode;
        }

        if (UIC_Manager.Instance.globalLineType == UIC_Connection.LineTypeEnum.Spline)
        {
            connectionUISpline.SetControlPoints(
                LinePoints[0],
                LinePoints[1],
                LinePoints[2],
                LinePoints[3]);
            connectionUILine.SetPoints(connectionUISpline.points);
        }
        else if (UIC_Manager.Instance.globalLineType == UIC_Connection.LineTypeEnum.Z_Shape)
        {
            connectionUILine.SetPoints(new Vector2[] { 
                LinePoints[0],
                (LinePoints[1] + LinePoints[0])/2,
                (LinePoints[2] + LinePoints[3])/2,
                LinePoints[3] });
        }
        else if (UIC_Manager.Instance.globalLineType == UIC_Connection.LineTypeEnum.Line)
        {
            connectionUILine.SetPoints(new Vector2[] { 
                LinePoints[0], 
                LinePoints[3] });
        }
    }

    public List<UIC_Entity> GetConnectedEntities()
    {
        List<UIC_Entity> connectedEntities = new List<UIC_Entity>();
        foreach (UIC_Connection conn in connectionsList)
        {
            UIC_Entity connEntitiy = conn.node0 != this ? conn.node0.entity : conn.node1.entity;
            connectedEntities.Add(connEntitiy);
        }

        return connectedEntities;
    }

    public List<UIC_Connection> GetOutConnections()
    {
        List<UIC_Connection> outConnections = new List<UIC_Connection>();
        foreach (UIC_Connection conn in connectionsList)
        {
            if (conn.node0 == this)
                outConnections.Add(conn);
        }
        return outConnections;
    }

    public List<UIC_Connection> GetInConnections()
    {
        List<UIC_Connection> outConnections = new List<UIC_Connection>();
        foreach (UIC_Connection conn in connectionsList)
        {
            if (conn.node0 != this)
                outConnections.Add(conn);
        }
        return outConnections;
    }
}
