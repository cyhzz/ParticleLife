using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ColorSetting
{
    public Color a;
    public Color b;
    public float g;
}

[System.Serializable]
public struct Particle
{
    public float p_x;
    public float p_y;

    public float v_x;
    public float v_y;

    public int group_index;
}

public class Simulation : Singleton<Simulation>
{
    public Dictionary<Color, List<Translate2D>> particles = new Dictionary<Color, List<Translate2D>>();
    public List<ColorSetting> color_settings = new List<ColorSetting>();
    public GameObject prefab;
    public BoxCollider2D bound;
    public Transform grid;
    public Action OnSettingChange;

    public int count;
    public List<Color> color_presets;
    public Particle[] particles_array;

    void Start()
    {
        Application.targetFrameRate = 60;
        count = 2;
        Create(200, Color.yellow);
        Create(200, Color.red);
    }

    public void AddColor()
    {
        if (count >= color_presets.Count)
        {
            return;
        }
        count++;
        ReSimulate();
    }

    public void RemoveColor()
    {
        if (count == 1)
        {
            return;
        }
        count--;
        ReSimulate();
    }

    public void ReSimulate()
    {
        grid.ClearChild();
        particles.Clear();
        color_settings.Clear();

        for (int i = 0; i < count; i++)
        {
            Create(200, color_presets[i]);
        }
    }

    public ComputeShader computer_shader;

    void OnUpdateGPU()
    {
        UpdateParticleArray();

        ComputeBuffer buffer = new ComputeBuffer(particles_array.Length, sizeof(float) * 4 + sizeof(int));

        buffer.SetData(particles_array);

        computer_shader.SetBuffer(0, "particles", buffer);
        computer_shader.SetInt("width", particles.Keys.Count);
        computer_shader.SetInt("total", particles_array.Length);

        var lst = particles.Keys.ToList();
        var colors = new float[lst.Count * lst.Count];
        for (int i = 0; i < lst.Count; i++)
        {
            for (int j = 0; j < lst.Count; j++)
            {
                colors[i * lst.Count + j] = color_settings.Find(c => c.a == lst[i] && c.b == lst[j]).g;
            }
        }
        ComputeBuffer g_buffer = new ComputeBuffer(colors.Length, sizeof(float));
        g_buffer.SetData(colors);
        computer_shader.SetBuffer(0, "gs", g_buffer);

        computer_shader.Dispatch(0, particles_array.Length / 10, 1, 1);

        buffer.GetData(particles_array);
        int index = 0;
        foreach (var item in particles)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                item.Value[i].velocity = new Vector2(particles_array[index].v_x, particles_array[index].v_y);
                index++;
            }
        }

        buffer.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        // for (int i = 0; i < color_settings.Count; i++)
        // {
        //     Rule(particles[color_settings[i].a], particles[color_settings[i].b], color_settings[i].g);
        // }
        OnUpdateGPU();
    }

    Vector2 RandomPos()
    {
        return new Vector2(UnityEngine.Random.Range(-bound.size.x / 2, bound.size.x / 2),
        UnityEngine.Random.Range(-bound.size.y / 2, bound.size.y / 2));
    }

    public void Create(int num, Color color)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject p = Instantiate(prefab, RandomPos(), Quaternion.identity, grid);
            p.GetComponent<SpriteRenderer>().color = color;

            if (particles.ContainsKey(color))
            {
                particles[color].Add(p.GetComponent<Translate2D>());
            }
            else
            {
                particles.Add(color, new List<Translate2D>() { p.GetComponent<Translate2D>() });
            }
            p.GetComponent<Translate2D>().min = bound.bounds.min;
            p.GetComponent<Translate2D>().max = bound.bounds.max;
        }

        var lst = particles.Keys.ToList();

        for (int i = 0; i < lst.Count; i++)
        {
            for (int j = 0; j < lst.Count; j++)
            {
                if (color_settings.Any(c => c.a == lst[i] && c.b == lst[j]))
                {
                    continue;
                }
                color_settings.Add(new ColorSetting()
                {
                    a = lst[i],
                    b = lst[j]
                });
                OnSettingChange?.Invoke();
            }
        }
        InitParticleArray();
    }

    void UpdateParticleArray()
    {
        int index = 0;
        foreach (var item in particles)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                particles_array[index].v_x = item.Value[i].velocity.x;
                particles_array[index].v_y = item.Value[i].velocity.y;
                particles_array[index].p_x = item.Value[i].transform.position.x;
                particles_array[index].p_y = item.Value[i].transform.position.y;
                index++;
            }
        }
    }

    void InitParticleArray()
    {
        int count = particles.Values.SelectMany(c => c).Count();
        particles_array = new Particle[count];
        int index = 0;
        int g_index = 0;
        foreach (var item in particles)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                particles_array[index] = new Particle()
                {
                    v_x = item.Value[i].velocity.x,
                    v_y = item.Value[i].velocity.y,
                    p_x = item.Value[i].transform.position.x,
                    p_y = item.Value[i].transform.position.y,
                    group_index = g_index
                };
                index++;
            }
            g_index++;
        }
    }

    public void Rule(List<Translate2D> particles1, List<Translate2D> particles2, float g)
    {
        for (int i = 0; i < particles1.Count; i++)
        {
            var a = particles1[i];
            float fx = 0;
            float fy = 0;
            for (int j = 0; j < particles2.Count; j++)
            {
                var b = particles2[j];

                float dx = a.transform.position.x - b.transform.position.x;
                float dy = a.transform.position.y - b.transform.position.y;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                if (d > 0 && d < 10)
                {
                    float F = g * 1 / d;
                    fx += (F * dx);
                    fy += (F * dy);
                }
            }
            a.velocity = (a.velocity + new Vector2(fx, fy)) * 0.5f;
        }
    }
}
