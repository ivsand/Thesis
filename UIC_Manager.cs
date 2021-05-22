using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIC_Manager : MonoBehaviour
{
    // list of entities in the scene, used to improve performance of detections 
    static List<UIC_Entity> _entityList;
    public static List<UIC_Entity> EntityList { get => _entityList; }

    // list of connections in the scene, used to improve performance of detections
    static List<UIC_Connection> _connectionsList;
    public static List<UIC_Connection> ConnectionsList { get => _connectionsList; }

    public static UIC_LineRenderer uiLineRenderer;
    public static List<UIC_Line> UILinesList { get => uiLineRenderer.UILines; }

    public static Canvas canvas;
    public static RectTransform canvasRectTransform;

    // v1.3 - reference to rendermode
    public static RenderMode CanvasRenderMode
    {
        get
        {
            return canvas.renderMode;
        }
    }
    public static Camera mainCamera;

    // list of selected uic objects, used for single or multi selection
    public static List<I_UIC_Selectable> selectedUIObjectsList = new List<I_UIC_Selectable>();
    public static I_UIC_Object clickedUIObject;

    public static T GetClickedObjectOfType<T>()
    {
        if (clickedUIObject is T)
            return (T)clickedUIObject;
        else
            return default(T);
    }

    static UIC_Pointer _pointer;

    // singleton pattern to make it easier to acess variables from the inspector  
    static UIC_Manager instance;
    public static UIC_Manager Instance
    {
          get => instance ?? (instance = FindObjectOfType<UIC_Manager>());
    }

    [Header("Logic")]
    public bool replaceConnectionByReverse;
    float _maxPointerDetectionDistance = 30;
    public float MaxPointerDetectionDistance
    {
        get
        {
            return _maxPointerDetectionDistance * canvasRectTransform.localScale.x;
        }
        set
        {
            _maxPointerDetectionDistance = value;
        }
    }

    [Header("Connection Settings (for new liens)")]
    public bool disableConnectionClick = false;
    public int globalLineWidth = 2;
    public Color globalLineDefaultColor = Color.white;
    public UIC_Connection.LineTypeEnum globalLineType;
    [Header("- line caps")]
    public UIC_Line.CapTypeEnum globalCapStartType;
    public float globalCapStartSize;
    public Color globalCapStartColor;
    public float globalCapStartAngleOffset;
    public UIC_Line.CapTypeEnum globalCapEndType;
    public float globalCapEndSize;
    public Color globalCapEndColor;
    public float globalCapEndAngleOffset;

    private void OnEnable()
    {
        InitUILineRenderer();
        instance = instance ? instance : this;
    }

    void OnValidate()
    {
        instance = instance ? instance : this;
    }

    void Awake()
    {
        instance = null;
        canvas = GetComponent<Canvas>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        _pointer = GetComponentInChildren<UIC_Pointer>();
        mainCamera = Camera.main;
    }

    void Start()
    {
        _entityList = new List<UIC_Entity>();
        EntityList.AddRange(FindObjectsOfType<UIC_Entity>());
        _connectionsList = new List<UIC_Connection>();

        InitUILineRenderer();
    }

    void InitUILineRenderer()
    {
        // todo --> test remove this first getcomponentinchildren, there is another already if it is null
        uiLineRenderer = GetComponentInChildren<UIC_LineRenderer>();
        if (!uiLineRenderer)
        {
            uiLineRenderer = GetComponentInChildren<UIC_LineRenderer>();

            if (!uiLineRenderer)
            {
                uiLineRenderer = Instantiate(Resources.Load("UIC_LineRenderer") as UIC_LineRenderer, transform.position, Quaternion.identity, transform);
                uiLineRenderer.name = "UIC_LineRenderer";
            }
        }
    }

    public static void AddLine(UIC_Line line)
    {
        if (!uiLineRenderer.UILines.Contains(line))
            uiLineRenderer.UILines.Add(line);
    }

    public static void RemoveLine(UIC_Line line)
    {
        if (uiLineRenderer.UILines.Contains(line))
            uiLineRenderer.UILines.Remove(line);
    }

    [System.Obsolete("Obsolete, use InstantiateEntityAtPosition instead")]
    public static void InstantiateEntity(UIC_Entity entityToInstantiate)
    {
        GameObject go = Instantiate(entityToInstantiate.gameObject, new Vector3(200, 100), Quaternion.identity, canvas.transform);
        AddEntity(go.GetComponent<UIC_Entity>());
    }

    // v1.3 - new method instantiate Entity at position
    public static void InstantiateEntityAtPosition(UIC_Entity entityToInstantiate, Vector3 position)
    {
        GameObject go = Instantiate(entityToInstantiate.gameObject, new Vector3(200, 100), Quaternion.identity, canvas.transform);
        AddEntity(go.GetComponent<UIC_Entity>());

        go.transform.position = position;
    }

    public static void AddEntity(UIC_Entity entityToAdd)
    {
        if (!EntityList.Contains(entityToAdd))
        {
            EntityList.Add(entityToAdd);
        }
    }

    public static void UpdateEntityList()
    {
        _entityList = new List<UIC_Entity>();
        EntityList.AddRange(FindObjectsOfType<UIC_Entity>());
    }

    // v1.3 - UIC_Manager.AddConnection return UIC_Connection
    public static UIC_Connection AddConnection(UIC_Node node0, UIC_Node node1, UIC_Connection.LineTypeEnum lineType = UIC_Connection.LineTypeEnum.Spline)
    {
        UIC_Connection previousConnectionWithSameNode = NodesAreConnected(node0, node1);
        if (previousConnectionWithSameNode != null)
        {
            if (UIC_Manager.Instance.replaceConnectionByReverse)
            {
                previousConnectionWithSameNode.Remove();
                return previousConnectionWithSameNode;
            }
            else
            {
                return previousConnectionWithSameNode;
            }
        }

        UIC_Connection _connection = CreateConnection(node0, node1, lineType);
        ConnectionsList.Add(_connection);

        node0.connectionsList.Add(_connection);
        node1.connectionsList.Add(_connection);

        AddLine(_connection.line);

        _connection.line.width = Instance.globalLineWidth;
        _connection.line.defaultColor = Instance.globalLineDefaultColor;
        _connection.line.color = Instance.globalLineDefaultColor;

        _connection.line.SetCap(UIC_Line.CapIDEnum.Start, Instance.globalCapStartType, Instance.globalCapStartSize, Instance.globalCapStartColor, Instance.globalCapStartAngleOffset);
        _connection.line.SetCap(UIC_Line.CapIDEnum.End, Instance.globalCapEndType, Instance.globalCapEndSize, Instance.globalCapEndColor, Instance.globalCapEndAngleOffset);

        _connection.UpdateLine();

        return _connection;
    }

    public static UIC_Connection NodesAreConnected(UIC_Node node0, UIC_Node node1)
    {
        foreach (UIC_Connection connection in ConnectionsList)
        {
            if ((node0 == connection.node0 && node1 == connection.node1) ||
                    (node0 == connection.node1 && node1 == connection.node0))
                return connection;
        }
        return null;
    }

    static UIC_Connection CreateConnection(UIC_Node node0, UIC_Node node1, UIC_Connection.LineTypeEnum lineType)
    {
        UIC_Connection _connection = new UIC_Connection(node0, node1, lineType);
        _connection.line = new UIC_Line();
        _connection.line.width = 2;

        return _connection;
    }

    public void RemoveUIObject()
    {
        for (int i = UIC_Manager.selectedUIObjectsList.Count - 1; i >= 0; i--)
        {
            UIC_Manager.selectedUIObjectsList[i].Remove();
        }
    }

    public static UIC_Connection FindClosestConnectionToPosition(Vector3 position, float maxDistance)
    {
        float minDist = Mathf.Infinity;
        UIC_Connection closestConnection = null;
        foreach (UIC_Connection connection in UIC_Manager.ConnectionsList)
        {
            int connectionPointsCount = connection.line.points.Count;
            if (connectionPointsCount > 0)
            {
                // v1.2 - changed from DistanceToSpline(obsolete) to DistanceToConnection, a general and more precise way to find distance to connections independent of the lineType
                for (int i = 1; i < connectionPointsCount; i++)
                {
                    float distance = UIC_Utility.DistanceToConnectino(connection, position, maxDistance);
                    if (distance < minDist)
                    {
                        closestConnection = connection;
                        minDist = distance;
                    }
                }
            }
        }

        return closestConnection;
    }

}

