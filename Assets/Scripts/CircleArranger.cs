
using UnityEngine;

public class CircleArranger : MonoBehaviour
{
    public RectTransform[] items;
    public float radius = 200f;

    void Start()
    {
        float angleStep = 360f / items.Length;
        for (int i = 0; i < items.Length; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;
            items[i].anchoredPosition = pos;
        }
    }
}
