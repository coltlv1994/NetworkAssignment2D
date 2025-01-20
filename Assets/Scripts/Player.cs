using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{

    Vector3 m_xoyAxisVector = Vector3.zero;
    float m_maxSpeed = 5.0f;
    float m_timeBeforeNextBullet = 1.0f / 2.0f; // 2 round(s) per second
    float m_bulletTimer = 0.0f;

    public List<Bullet> m_bulletPool = new List<Bullet>();

    public Bullet bulletBase;

    public override void OnNetworkSpawn()
    { 
        m_bulletTimer = m_timeBeforeNextBullet;
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            // update bullet timer
            m_bulletTimer -= Time.deltaTime;

            if (m_bulletTimer <= 0.0f)
            {
                // fire a bullet
                FireBullet();

                // reset bullet timer
                m_bulletTimer = m_timeBeforeNextBullet;
            }
            m_xoyAxisVector.x = Input.GetAxis("Horizontal");
            m_xoyAxisVector.y = Input.GetAxis("Vertical");
            m_xoyAxisVector.Normalize();

            transform.position += m_xoyAxisVector * m_maxSpeed * Time.deltaTime;

            transform.up = m_xoyAxisVector; // rotate the spaceship

            foreach (Bullet bullet in m_bulletPool)
            {
                bullet.UpdateInPool(Time.deltaTime);
            }
        }
    }

    public void FireBullet()
    {
        bool m_bulletFired = false;

        Vector3 m_bulletStartPosition = transform.position + transform.up.normalized * 2.5f;

        // fire a bullet, first check if there is any "available bullet"
        foreach (Bullet bullet in m_bulletPool)
        {
            if (bullet.IsActive() == false)
            {
                // reuse this bullet
                bullet.Init(m_bulletStartPosition, transform.up, this);
                m_bulletFired = true;
                break;
            }
        }

        if (m_bulletFired == false && m_bulletPool.Count < 20)
        {
            // no available bullets and no more than 20 in the wild
            // fire a "new" bullet
            Bullet bulletFired = Instantiate(bulletBase, m_bulletStartPosition, Quaternion.identity);
            bulletFired.Init(m_bulletStartPosition, transform.up, this);
            m_bulletPool.Add(bulletFired);
            m_bulletFired = true;
        }
    }

    public void Score()
    {
        // call to server for scoring one points

    }
}
