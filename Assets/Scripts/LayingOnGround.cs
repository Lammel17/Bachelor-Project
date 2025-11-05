using UnityEngine;

public class LayingOnGround : MonoBehaviour
{

    [SerializeField] private Transform m_root;
    [SerializeField] private Transform m_centerBone;

    [SerializeField] private Transform m_pelvis;
    [SerializeField] private Transform m_chest;
    [Tooltip("[Y Axis Dist between (average of pelvis & chest) and ground] Dist below x threshhold: considered to snap to ground normal. Dist above y threshhold: considered not snapped to ground normal. (best case: (0.2f,0.5f))")]
    [SerializeField] private Vector2 m_minDistToGround = new Vector2(0.2f, 0.5f);
    [SerializeField] private float m_raycastLenght = 0.8f;
    [SerializeField] private LayerMask m_environmentLayer;
    [Space]

    [SerializeField] private bool m_affectFootPlacing = false;
    [SerializeField] private FootPlacing m_footPlacing;

    private Vector3 m_averageNormal = Vector3.zero;

    private float m_initialPelvisChestDistY = 0;


    private void Awake()
    {
        m_initialPelvisChestDistY = Mathf.Abs(m_pelvis.position.y - m_chest.position.y);
    }

    void LateUpdate()
    {
        if ((((m_chest.position.y + m_pelvis.position.y) / 2) - m_root.position.y) > m_minDistToGround.y) 
        { 
            if (m_root.rotation != this.transform.rotation)
            {
                m_root.rotation = this.transform.rotation;
                if (m_affectFootPlacing)
                    m_footPlacing.SetWeightByLayingOnGround(1);

            }

            return; 
        }

        float startHeightOffset = 0.25f;
        float hits = 0;


        RaycastHit hit0;
        RaycastHit hit1;
        RaycastHit hit2;
        RaycastHit hit3;
        RaycastHit hit4;
        bool hasHit0 = false;
        bool hasHit1 = false;
        bool hasHit2 = false;
        bool hasHit3 = false;
        bool hasHit4 = false;
        Vector3 raycastOrigin0 = new Vector3(m_centerBone.position.x, m_centerBone.position.y + startHeightOffset, m_centerBone.position.z);
        Vector3 raycastOrigin1 = new Vector3(m_centerBone.position.x, m_centerBone.position.y + startHeightOffset, m_centerBone.position.z + 0.2f);
        Vector3 raycastOrigin2 = new Vector3(m_centerBone.position.x + 0.2f, m_centerBone.position.y + startHeightOffset, m_centerBone.position.z);
        Vector3 raycastOrigin3 = new Vector3(m_centerBone.position.x, m_centerBone.position.y + startHeightOffset, m_centerBone.position.z - 0.2f);
        Vector3 raycastOrigin4 = new Vector3(m_centerBone.position.x - 0.2f, m_centerBone.position.y + startHeightOffset, m_centerBone.position.z);

        if (Physics.Raycast(raycastOrigin0, Vector3.down, out hit0, m_raycastLenght + startHeightOffset, m_environmentLayer))
        { hasHit0 = true; hits++; }
        if (Physics.Raycast(raycastOrigin1, Vector3.down, out hit1, m_raycastLenght + startHeightOffset, m_environmentLayer))
        { hasHit1 = true; hits++; }
        if (Physics.Raycast(raycastOrigin2, Vector3.down, out hit2, m_raycastLenght + startHeightOffset, m_environmentLayer))
        { hasHit2 = true; hits++; }
        if (Physics.Raycast(raycastOrigin3, Vector3.down, out hit3, m_raycastLenght + startHeightOffset, m_environmentLayer))
        { hasHit3 = true; hits++; }
        if (Physics.Raycast(raycastOrigin4, Vector3.down, out hit4, m_raycastLenght + startHeightOffset, m_environmentLayer))
        { hasHit4 = true; hits++; }

        //Debug.Log(m_chest.position.y - m_pelvis.position.y);
        //Debug.Log(hits);
        Debug.DrawLine(raycastOrigin0, raycastOrigin0 + Vector3.down * (m_raycastLenght + startHeightOffset), Color.red);
        Debug.DrawLine(raycastOrigin1, raycastOrigin1 + Vector3.down * (m_raycastLenght + startHeightOffset), Color.red);
        Debug.DrawLine(raycastOrigin2, raycastOrigin2 + Vector3.down * (m_raycastLenght + startHeightOffset), Color.red);
        Debug.DrawLine(raycastOrigin3, raycastOrigin3 + Vector3.down * (m_raycastLenght + startHeightOffset), Color.red);
        Debug.DrawLine(raycastOrigin4, raycastOrigin4 + Vector3.down * (m_raycastLenght + startHeightOffset), Color.red);


        Vector3 v = Vector3.zero;
        if (hits >= 5) 
        {
            v = Vector3.Cross(hit1.point - hit0.point, hit2.point - hit0.point);
            m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
            v = Vector3.Cross(hit2.point - hit0.point, hit3.point - hit0.point);
            m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
            v = Vector3.Cross(hit3.point - hit0.point, hit4.point - hit0.point);
            m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
            v = Vector3.Cross(hit4.point - hit0.point, hit1.point - hit0.point);
            m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;

        }
        else if (hits == 4)
        {
            if (!hasHit0)
            {
                v = Vector3.Cross(hit1.point - hit2.point, hit3.point - hit2.point);
                m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                v = Vector3.Cross(hit2.point - hit3.point, hit4.point - hit3.point);
                m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                v = Vector3.Cross(hit3.point - hit4.point, hit1.point - hit4.point);
                m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                v = Vector3.Cross(hit4.point - hit1.point, hit2.point - hit1.point);
                m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
            }
            else
            {
                if (!hasHit1)
                {
                    v = Vector3.Cross(hit2.point - hit0.point, hit3.point - hit0.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                    v = Vector3.Cross(hit3.point - hit0.point, hit4.point - hit0.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                    v = Vector3.Cross(hit2.point - hit3.point, hit4.point - hit3.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                }
                else if (!hasHit2)
                {
                    v = Vector3.Cross(hit3.point - hit0.point, hit4.point - hit0.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                    v = Vector3.Cross(hit4.point - hit0.point, hit1.point - hit0.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                    v = Vector3.Cross(hit3.point - hit4.point, hit1.point - hit4.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                }
                else if (!hasHit3)
                {
                    v = Vector3.Cross(hit1.point - hit0.point, hit2.point - hit0.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                    v = Vector3.Cross(hit4.point - hit0.point, hit1.point - hit0.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                    v = Vector3.Cross(hit4.point - hit1.point, hit2.point - hit1.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                }
                else if (!hasHit4)
                {
                    v = Vector3.Cross(hit1.point - hit0.point, hit2.point - hit0.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                    v = Vector3.Cross(hit2.point - hit0.point, hit3.point - hit0.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                    v = Vector3.Cross(hit1.point - hit2.point, hit3.point - hit2.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                }
            }
        }
        else if (hits == 3)
        {
            if (!hasHit0)
            {
                if (!hasHit1)
                {
                    v = Vector3.Cross(hit2.point - hit3.point, hit4.point - hit3.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                }
                else if (!hasHit2)
                {
                    v = Vector3.Cross(hit3.point - hit4.point, hit1.point - hit4.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                }
                else if (!hasHit3)
                {
                    v = Vector3.Cross(hit4.point - hit1.point, hit2.point - hit1.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                }
                else if (!hasHit4)
                {
                    v = Vector3.Cross(hit1.point - hit2.point, hit3.point - hit2.point);
                    m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
                }
            }
            else if (hasHit1 && hasHit2)
            {
                v = Vector3.Cross(hit1.point - hit0.point, hit2.point - hit0.point);
                m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
            }
            else if (hasHit2 && hasHit3)
            {
                v = Vector3.Cross(hit2.point - hit0.point, hit3.point - hit0.point);
                m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
            }
            else if (hasHit3 && hasHit4)
            {
                v = Vector3.Cross(hit3.point - hit0.point, hit4.point - hit0.point);
                m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
            }
            else if (hasHit4 && hasHit1)
            {
                v = Vector3.Cross(hit4.point - hit0.point, hit1.point - hit0.point);
                m_averageNormal += v.y > 0 ? v : v.y < 0 ? -v : Vector3.zero;
            }
            else
            {
                if (hasHit0) m_averageNormal += hit0.normal;
                if (hasHit1) m_averageNormal += hit1.normal;
                if (hasHit2) m_averageNormal += hit2.normal;
                if (hasHit3) m_averageNormal += hit3.normal;
                if (hasHit4) m_averageNormal += hit4.normal;
            }

        }
        else if (hits <= 2)
        {
            m_averageNormal = Vector3.up;
        }

        m_averageNormal = m_averageNormal.normalized;

        Quaternion desiredRootRot = Quaternion.FromToRotation(Vector3.up, m_averageNormal) * this.transform.rotation;

        test.rotation = desiredRootRot;

        float weight = Mathf.InverseLerp(m_minDistToGround.x, m_minDistToGround.y, ((m_chest.position.y + m_pelvis.position.y) / 2) - m_root.position.y);

        if(m_affectFootPlacing) 
            m_footPlacing.SetWeightByLayingOnGround(weight);

        desiredRootRot = Quaternion.Slerp(desiredRootRot, this.transform.rotation, weight);
        //Debug.Log(Mathf.InverseLerp(m_minDistToGround.x, m_minDistToGround.y, ((m_chest.position.y + m_pelvis.position.y) / 2) - m_root.position.y));

        m_root.rotation = desiredRootRot;
    }

    public Transform test;
}
