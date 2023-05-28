using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPresenter : Singleton<MenuPresenter>
{
    public GameObject prefab;
    public Transform grid;

    public void Present(Color a)
    {
        grid.ClearChild();
        var lst = Simulation.instance.color_settings.FindAll(c => c.a == a);

        for (int i = 0; i < lst.Count; i++)
        {
            GameObject g = Instantiate(prefab, grid);
            g.transform.Find("a").GetComponent<Image>().color = lst[i].a;
            g.transform.Find("b").GetComponent<Image>().color = lst[i].b;

            var tag = lst[i];
            g.GetComponentInChildren<Slider>().SetValueWithoutNotify(tag.g);
            g.transform.Find("text").GetComponent<TextMeshProUGUI>().text = System.Math.Round(tag.g, 3).ToString();
            g.GetComponentInChildren<Slider>().onValueChanged.AddListener((c) =>
            {
                tag.g = c;
                g.transform.Find("text").GetComponent<TextMeshProUGUI>().text = System.Math.Round(c, 3).ToString();
            });
            g.transform.Find("R").GetComponent<Button>().onClick.AddListener(() =>
            {
                g.GetComponentInChildren<Slider>().value = 0;
            });
        }
    }
}
