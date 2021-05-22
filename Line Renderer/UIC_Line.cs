using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UIC_Line
{
    public CapTypeEnum capStartType;
    public float capStartSize;
    public Color capStartColor;
    public float capStartAngleOffset;
    public CapTypeEnum capEndType;
    public float capEndSize;
    public Color capEndColor;
    public float capEndAngleOffset;

    public string ID;
    public float width;

    public float startWidth;
    public float endWidth;

    public Color color;
    public List<Vector2> points;

    public Color selectedColor = new Color32(0x7f, 0x5a, 0xf0, 0xff);
    public Color hoverColor = new Color32(0x2c, 0xb6, 0x7d, 0xff);
    public Color defaultColor = new Color32(0xff, 0xff, 0xfe, 0xff);

    public UIC_Line()
    {
        color = defaultColor;
        width = 2;
        points = new List<Vector2>();
    }

    /// <summary>
    /// Clear all points and add new points to the line
    /// </summary>
    /// <param name="newPoints"></param>
    public void SetPoints(Vector2[] newPoints)
    {
        if (newPoints != null)
        {
            points.Clear();
            points.AddRange(newPoints);
        }
    }

    public enum CapTypeEnum { none, Square, Circle, Triangle };
    public enum CapIDEnum { Start, End };
    public void SetCap(CapIDEnum capID, CapTypeEnum capType, float capSize = 5, Color? capColor = null, float angleOffset = 0)
    {
        if (capID == CapIDEnum.Start)
        {
            this.capStartSize = capSize;
            this.capStartType = capType;
            this.capStartColor = capColor ?? Color.white;
            this.capStartAngleOffset = angleOffset;
        }
        else
        {
            this.capEndSize = capSize;
            this.capEndType = capType;
            this.capEndColor = capColor ?? Color.white;
            this.capEndAngleOffset = angleOffset;
        }
    }
}
