using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ControlPresenter : MonoBehaviour
{
    public Simulation sim;
    public GameObject prefab;

    public Transform grid;
    public float radius;
    public float line_gap;

    public GameObject line_prefab;
    public Transform line_grid;

    void Start()
    {
        sim.OnSettingChange += Refresh;
    }

    void Update()
    {
        // Refresh();
    }

    public void Refresh()
    {
        line_grid.ClearChild();
        grid.ClearChild();
        var lst = sim.particles.Keys.ToList();
        float step = 360 / lst.Count;

        for (int i = 0; i < lst.Count; i++)
        {
            GameObject g = Instantiate(prefab, grid);
            float degrees = step * i + 90;
            float rad = Mathf.Deg2Rad * degrees;
            Vector2 v = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            g.transform.localPosition = v * radius;
            g.GetComponent<Image>().color = lst[i];
            g.GetComponent<LineGragger>().setting = lst[i];
        }

        for (int i = 0; i < lst.Count; i++)
        {
            for (int j = 0; j < lst.Count; j++)
            {
                LinkCell(grid.GetChild(i), grid.GetChild(j));
                LinkCell(grid.GetChild(j), grid.GetChild(i));
            }
        }
    }

    GameObject LinkCell(Transform a, Transform b)
    {
        GameObject line = Instantiate(line_prefab, line_grid);
        Vector2 pos_a = a.position2D().World2RectLocal();
        Vector2 pos_b = b.position2D().World2RectLocal();

        Vector2 offset = Vector2.Perpendicular(pos_b - pos_a).normalized * line_gap;
        line.GetComponent<UILineRenderer>().Points = new Vector2[2]{
                pos_a+offset,
                pos_b+offset
            };
        return line;
    }
}
