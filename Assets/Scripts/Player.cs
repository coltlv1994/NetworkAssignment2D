using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{

    Vector3 m_xoyAxisVector = Vector3.zero;
    float m_maxSpeed = 5.0f;
    float m_timeBeforeNextBullet = 1.0f / 2.0f; // 2 round(s) per second
    float m_bulletTimer = 0.0f;

    public NetworkVariable<int> m_serverScore = new NetworkVariable<int>();
    public NetworkVariable<int> m_clientScore = new NetworkVariable<int>();

    public List<Bullet> m_bulletPool = new List<Bullet>();

    public Bullet bulletBase;

    [SerializeField] private TextMeshProUGUI m_selfScore;
    public TMPro.TMP_Text m_enemyScore;

    public override void OnNetworkSpawn()
    { 
        m_bulletTimer = m_timeBeforeNextBullet;
        if (IsServer)
        {
            m_serverScore.Value = 0;
            m_clientScore.Value = 0;
        }
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
        }

        if (IsHost)
        {
            foreach (Bullet bullet in m_bulletPool)
            {
                bullet.UpdateInPool(Time.deltaTime);
            }
        }
    }

    public void FireBullet()
    {
        Vector3 m_bulletStartPosition = transform.position + transform.up.normalized * 2.5f;
        if (IsHost)
        {
            FireBullet(m_bulletStartPosition, transform.up, true);
        }
        else
        {
            FireBulletsOnServerRpc(m_bulletStartPosition, transform.up);
        }
    }



    [Rpc(SendTo.Server)]
    void FireBulletsOnServerRpc(Vector3 p_position, Vector3 p_heading)
    {
        // server gonna fire bullet for client
        FireBullet(p_position, p_heading, false);
    }

    void FireBullet(Vector3 p_position, Vector3 p_heading, bool p_isOwner)
    {
        bool m_bulletFired = false;

        // fire a bullet, first check if there is any "available bullet"
        foreach (Bullet bullet in m_bulletPool)
        {
            if (bullet.IsActive() == false)
            {
                // reuse this bullet
                bullet.Init(p_position, p_heading, p_isOwner);
                m_bulletFired = true;
                break;
            }
        }

        if (m_bulletFired == false && m_bulletPool.Count < 20)
        {
            // no available bullets and no more than 20 in the wild
            // fire a "new" bullet
            Bullet bulletFired = Instantiate(bulletBase, p_position, Quaternion.identity);
            bulletFired.Init(p_position, p_heading, p_isOwner);
            var instanceNetworkObject = bulletFired.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn();
            m_bulletPool.Add(bulletFired);

            m_bulletFired = true;
        }
    }

    public void Score()
    {
        // call to server for scoring one points

    }

    [Rpc(SendTo.Server)]
    void ClientScoreNotifyServerRpc()
    {

    }
    public void OnSomeValueChanged()
    {
    }
}
