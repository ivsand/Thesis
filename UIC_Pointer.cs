using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIC_Pointer : MonoBehaviour
{
    public KeyCode clickKey;
    public KeyCode secondaryKey;

    Image _image;
    Canvas _pointerCanvas;

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    UIC_LineRenderer _uILineRenderer;
    UIC_Line _draggedConnectionUILine;
    UISpline _draggedConnectionUISpline;

    public Sprite iconDefault;
    public Sprite iconHold;

    Vector3 _initialMousePos;

    List<I_UIC_Object> _orderedObjectsList = new List<I_UIC_Object>();

    // v1.3 - corrected pointer position for canvas render mode overlay and camera
    public static Vector3 PointerPosition { get => Input.mousePosition; }//instance.transform.position; }
    Camera _mainCamera;

    public UnityEvent e_OnPointerDownFirst;
    public UnityEvent e_OnPointerDownLast;

    public UnityEvent e_OnDragFirst;
    public UnityEvent e_OnDragLast;

    public UnityEvent e_OnPointerUpFirst;
    public UnityEvent e_OnPointerUpLast;
    // OBS.: event First and Last means that it is called prior or after all the actions on the event

    void OnValidate()
    {
        Init();
    }

    static UIC_Pointer instance;
    public static UIC_Pointer Instance
    {
        get => instance ?? (instance = FindObjectOfType<UIC_Pointer>());
    }

    void Awake()
    {
        instance = null;
        _mainCamera = UIC_Manager.mainCamera;
    }

    void Start()
    {
        e_OnPointerDownFirst = e_OnPointerDownFirst ?? new UnityEvent();
        e_OnPointerDownLast = e_OnPointerDownLast ?? new UnityEvent();
        e_OnDragFirst = e_OnDragFirst ?? new UnityEvent();
        e_OnDragLast = e_OnDragLast ?? new UnityEvent();
        e_OnPointerUpFirst = e_OnPointerUpFirst ?? new UnityEvent();
        e_OnPointerUpLast = e_OnPointerUpLast ?? new UnityEvent();

        Cursor.visible = false;
        FollowMouse();
        Init();

        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = FindObjectOfType<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = FindObjectOfType<EventSystem>();
    }

    public void Init()
    {
        instance = instance ? instance : this;

        _image = _image ? _image : GetComponent<Image>() ? GetComponent<Image>() : gameObject.AddComponent<Image>();
        _image.raycastTarget = false;
        _pointerCanvas = _pointerCanvas ? _pointerCanvas : GetComponent<Canvas>() ? GetComponent<Canvas>() : gameObject.AddComponent<Canvas>();
        _pointerCanvas.overrideSorting = true;
        _pointerCanvas.sortingOrder = 999; // pointer on top of everything makes dragged entity being also on top of everithing

        _uILineRenderer = FindObjectOfType<UIC_LineRenderer>();
        _draggedConnectionUISpline = new UISpline();
        _draggedConnectionUILine = new UIC_Line();
        _draggedConnectionUILine.color = new Color32(0x00, 0xFF, 0xFF, 0xFF);
        _uILineRenderer.UILines.Add(_draggedConnectionUILine);
    }

    void Update()
    {
        FollowMouse();

        if (Input.GetKeyDown(clickKey))
        {
            OnPointerDown();
        }

        if (Input.GetKey(clickKey))
        {
            if (_initialMousePos != transform.position)
                OnDrag();
        }

        if (Input.GetKeyUp(clickKey))
        {
            OnPointerUp();
        }

    }

    private void OnDisable()
    {
        e_OnPointerDownFirst.RemoveAllListeners();
        e_OnPointerDownLast.RemoveAllListeners();
        e_OnDragFirst.RemoveAllListeners();
        e_OnDragLast.RemoveAllListeners();
        e_OnPointerUpFirst.RemoveAllListeners();
        e_OnPointerUpLast.RemoveAllListeners();
    }

    void FollowMouse()
    {
        if (UIC_Manager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
        {
            transform.position = Input.mousePosition;
            return;
        }
        else if (UIC_Manager.CanvasRenderMode == RenderMode.ScreenSpaceCamera)
        {
            var screenPoint = Input.mousePosition;
            screenPoint.z = 10.0f; //distance of the plane from the camera
            transform.position = _mainCamera.ScreenToWorldPoint(screenPoint);
        }
    }

    public void OnPointerDown()
    {
        e_OnPointerDownFirst.Invoke();

		// UIC_Manager.clickedUIObject<DragItem>.enabled = true;


        _image.sprite = iconHold;

        SelectCloserUIObject();
        UIC_ContextMenu.UpdateContextMenu();
        _initialMousePos = transform.position;

        e_OnPointerDownLast.Invoke();
    }

    public void OnDrag()
    {	


        e_OnDragFirst.Invoke();

        if (UIC_Manager.clickedUIObject is I_UIC_Draggable)
            (UIC_Manager.clickedUIObject as I_UIC_Draggable).OnDrag();

        if (UIC_Manager.clickedUIObject is UIC_Entity)
        {
            foreach (I_UIC_Selectable obj in UIC_Manager.selectedUIObjectsList)
            {
                if (obj is UIC_Entity)
                {
                    (obj as I_UIC_Draggable).OnDrag();
                }
            }
        }

        e_OnDragLast.Invoke();
    }

    public void OnPointerUp()
    {
		// GameObject varGameObject = UIC_Manager.clickedUIObject;
		// varGameObject.GetComponent<DragItem>().enabled = false;


       e_OnPointerUpFirst.Invoke();

       _image.sprite = iconDefault;

        if (UIC_Manager.clickedUIObject is I_UIC_Clickable)
            (UIC_Manager.clickedUIObject as I_UIC_Clickable).OnPointerUp();

        foreach (I_UIC_Object uiObject in UIC_Manager.selectedUIObjectsList)
        {
            if (uiObject is I_UIC_Clickable)
                (uiObject as I_UIC_Clickable).OnPointerUp();
        }

        e_OnPointerUpLast.Invoke();
    }


    public static void UnselectAllUIObjects()
    {
        if (!Input.GetKey(instance.secondaryKey))
        {
            for (int i = UIC_Manager.selectedUIObjectsList.Count - 1; i >= 0; i--)
            {
                UIC_Manager.selectedUIObjectsList[i].Unselect();
            }
        }

    }

	


    public List<RaycastResult> RaycastUI()
    {
        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = PointerPosition;
        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();
        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);

        return results;
    }

    private static int SortByPriority(I_UIC_Object o1, I_UIC_Object o2)
    {
        return o2.Priority.CompareTo(o1.Priority);
    }

    public List<I_UIC_Object> ReorderFoundObjectsToPriority(List<I_UIC_Object> objectsList)
    {
        objectsList.Sort(SortByPriority);
        return objectsList;
    }

    public List<I_UIC_Object> OrderedObjectsUnderPointer()
    {
        List<I_UIC_Object> orderedObjects = new List<I_UIC_Object>();

        List<RaycastResult> results = RaycastUI();

        I_UIC_Object uiObject = null;

        foreach (RaycastResult result in results)
        {
            uiObject = result.gameObject.GetComponent<I_UIC_Object>();

            if (uiObject != null)
            {
                if (!(uiObject is I_UIC_Clickable) || !(uiObject as I_UIC_Clickable).DisableClick)
                    orderedObjects.Add(uiObject);
            }
        }
        uiObject = UIC_Manager.FindClosestConnectionToPosition(PointerPosition, 15);
        if (uiObject != null)
            if (!(uiObject as I_UIC_Clickable).DisableClick)
                orderedObjects.Add(uiObject);

        orderedObjects.Sort(SortByPriority);

        return orderedObjects;
    }

    public I_UIC_Object FindObjectCloserToPointer()
    {
        _orderedObjectsList = OrderedObjectsUnderPointer();

        if (_orderedObjectsList.Count > 0)
        {
            if (!(_orderedObjectsList[0] is I_UIC_ContextItem))
                UnselectAllUIObjects();

            return _orderedObjectsList[0];
        }
        else
        {
            UnselectAllUIObjects();
            return null;
        }
    }

    public void SelectCloserUIObject()
    {
        UIC_Manager.clickedUIObject = FindObjectCloserToPointer();
        if (UIC_Manager.clickedUIObject is I_UIC_Clickable)
        {
            (UIC_Manager.clickedUIObject as I_UIC_Clickable).OnPointerDown();
        }
    }

    public static UIC_Node FindClosestNodeOfOppositPolarity(Vector2 position, float maxDistance, UIC_Node draggedNode)
    {
        float minDist = Mathf.Infinity;
        UIC_Node closestNode = null;
        foreach (UIC_Entity entity in UIC_Manager.EntityList)
        {
            if ((entity == draggedNode.entity && entity.enableSelfConnection) || entity != draggedNode.entity)
            {
                foreach (UIC_Node node in entity.nodeList)
                {
                    if (draggedNode != node && node.haveSpots && draggedNode.haveSpots)
                    {
                        if (node.polarityType != draggedNode.polarityType || node.polarityType == UIC_Node.PolarityTypeEnum._all)
                        {
                            float distance = Vector2.Distance(position, node.transform.position);
                            if (distance < minDist && distance <= maxDistance / UIC_Manager.uiLineRenderer.rectTransform.localScale.x)
                            {
                                closestNode = node;
                                minDist = distance;
                            }
                        }
                    }
                }
            }
        }

        return closestNode;
    }

}
