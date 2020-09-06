using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudAnimation : MonoBehaviour
{
    public float m_speed;
    public Vector3 m_goalPosition;

    private bool m_goingTo; // New position
    private Vector3 m_initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        m_initialPosition = transform.position;
        m_goingTo = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_goingTo)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_goalPosition, m_speed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, m_initialPosition, m_speed * Time.deltaTime);
        }

        if (Mathf.Abs(transform.position.x - m_goalPosition.x) < 1 && Mathf.Abs(transform.position.y - m_goalPosition.y) < 1)
        {
            m_goingTo = !m_goingTo;
        }
    }
}
