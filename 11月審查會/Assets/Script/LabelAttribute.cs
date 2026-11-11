using UnityEngine;

public class LabelAttribute : PropertyAttribute
{
    public readonly string label;
    public readonly bool hasMin;
    public readonly bool hasMax;
    public readonly float min;
    public readonly float max;

    public LabelAttribute(string label)
    {
        this.label = label;
    }

    // 只有最小值（取代 [Min]）
    public LabelAttribute(string label, float min)
    {
        this.label = label;
        this.hasMin = true;
        this.min = min;
    }

    // 有最小與最大值，顯示成滑桿（取代 [Range]）
    public LabelAttribute(string label, float min, float max)
    {
        this.label = label;
        this.hasMin = true;
        this.hasMax = true;
        this.min = min;
        this.max = max;
    }
}