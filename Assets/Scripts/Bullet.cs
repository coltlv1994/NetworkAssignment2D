using Unity.Netcode;
using Unity.XR.OpenVR;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private float m_maxSpeed = 20.0f;
    private float m_timeToDie = 1.5f;

    private bool m_isActive = true;

    private bool m_owner;

    public void Init(Vector3 p_initialPosition, Vector3 p_heading, bool p_owner)
    {
        transform.position = p_initialPosition;
        transform.up = p_heading;
        m_owner = p_owner;
        m_timeToDie = 1.5f;
        m_isActive = true;
        this.GetComponent<Collider2D>().enabled = true;
    }

    public void UpdateInPool(float deltaTime)
    {
        if (m_isActive == false)
        {
            return;
        }

        transform.position += transform.up.normalized * m_maxSpeed * Time.deltaTime;
        m_timeToDie -= deltaTime;

        if (m_timeToDie < 0)
        {
            this.SetActive(false);
        }
    }

    public bool IsActive()
    {
        return m_isActive;
    }

    public void SetActive(bool p_isActive)
    {
        m_isActive = p_isActive;
        if (m_isActive == false)
        {
            transform.position = new Vector3(0, 0, -20); // out of scope
        }
        else
        {
            this.GetComponent<Collider2D>().enabled = true;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        this.GetComponent<Collider2D>().enabled = false;

        Debug.Log("Hit!");

        if (m_isActive == false)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            Player e = null;
            collision.TryGetComponent<Player>(out e);
            if (e != null && (e.IsOwner != m_owner))
            {
                // an enemy is hit
                //m_owner.Score();
                Debug.Log("Score!");
                SetActive(false);
            }
        }
    }
}
