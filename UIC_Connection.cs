using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIC_Connection : I_UIC_Object, I_UIC_Selectable, I_UIC_Clickable
{
    // connection nodes, node0 start & node1 end 
    public UIC_Node node0;
    public UIC_Node node1;

    public enum LineTypeEnum { Spline, Z_Shape, Line }
    public LineTypeEnum lineType;
    public UIC_Line line;

    public Vector3[] handles = new Vector3[2];

    public UIC_Connection(UIC_Node node0, UIC_Node node1, LineTypeEnum lineType)
    {
        this.node0 = node0;
        this.node1 = node1;
        this.lineType = lineType; 
    }

    public string ID => string.Format("Connection ({0} - {1})", node0.entity ? node0.entity.name : "null", node1.entity ? node1.entity.name : "null");

    public Vector3[] Handles
    {
        get
        {
            handles[0] = node0.spLineControlPointTranform.position;
            handles[1] = node1.spLineControlPointTranform.position;
            return handles;
        }
        set => handles = value;
    }
    public Color objectColor
    {
        get => line.defaultColor;
        set
        {
            line.color = value;
            line.defaultColor = value;
        }
    }

    public int Priority => 1;

    public bool DisableClick => UIC_Manager.Instance.disableConnectionClick;

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

    }

    public void Remove()
    {
        Unselect();

        node0.connectionsList.Remove(this);
        node1.connectionsList.Remove(this);

        node0.SetIcon();
        node1.SetIcon();

        UIC_Manager.RemoveLine(this.line);
        UIC_Manager.ConnectionsList.Remove(this);
    }

    public void Select()
    {
        line.color = line.selectedColor;
        if (!UIC_Manager.selectedUIObjectsList.Contains(this))
        {
            UIC_Manager.selectedUIObjectsList.Add(this);
        }
    }

    public void Unselect()
    {
        line.color = line.defaultColor;
        if (UIC_Manager.selectedUIObjectsList.Contains(this))
        {
            UIC_Manager.selectedUIObjectsList.Remove(this);
        }
    }

    public UISpline connectionUISpline;

    // v1.3 - line poinst adjusted to canvas render mode overlay and camera
    Vector3[] LinePoints
    {
        get
        {
            if (UIC_Manager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
            {
                return new Vector3[] {
                    node0.transform.position,
                    Handles[0],
                    Handles[1],
                    node1.transform.position };
            }
            else if (UIC_Manager.CanvasRenderMode == RenderMode.ScreenSpaceCamera)
            {
                return new Vector3[] {
                    UIC_Manager.mainCamera.WorldToScreenPoint(node0.transform.position),
                    UIC_Manager.mainCamera.WorldToScreenPoint(Handles[0]),
                    UIC_Manager.mainCamera.WorldToScreenPoint(Handles[1]),
                    UIC_Manager.mainCamera.WorldToScreenPoint(node1.transform.position) };
            }

            return new Vector3[] { 
                node0.transform.position, 
                Handles[0], 
                Handles[1], 
                node1.transform.position };
        }
    }

    public void UpdateLine()
    {
        if (lineType == LineTypeEnum.Spline)
        {
            connectionUISpline = new UISpline();
            connectionUISpline.SetControlPoints(
                LinePoints[0],
                LinePoints[1],
                LinePoints[2],
                LinePoints[3]);
            line.SetPoints(connectionUISpline.points);
        }
        if (lineType == LineTypeEnum.Z_Shape)
        {
            line.SetPoints(new Vector2[] { 
                LinePoints[0],
                (LinePoints[1] + LinePoints[0])/2,
                (LinePoints[2] + LinePoints[3])/2,
                LinePoints[3] });
        }
        if (lineType == LineTypeEnum.Line)
        {
            line.SetPoints(new Vector2[] {
                LinePoints[0], 
                LinePoints[3] });
        }
    }

    public List<UIC_Connection> GetCrossedConnections()
    {
        List<UIC_Connection> crossedConnections = new List<UIC_Connection>();

        foreach (UIC_Connection conn in UIC_Manager.ConnectionsList)
        {
            if (UIC_Utility.ConnectionsIntersect(conn, this))
                if (!(conn.node0 == node0 || conn.node1 == node1 || conn.node0 == node1 || conn.node1 == node0))
                    crossedConnections.Add(conn);
        }

        return crossedConnections;
    }

    public UIC_Node GetOppositeNode(UIC_Node node)
    {
        return node0 == node ? node1 : node0;
    }
}
