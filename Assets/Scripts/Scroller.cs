using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroller : MonoBehaviour
{
    // Scroll the main texture based on time

    public float m_speed = 0.5f;
    private Renderer m_renderer;

    void Start()
    {
        m_renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        m_renderer.material.SetTextureOffset("_MainTex", new Vector2(Time.time * m_speed, 0));
    }
}
