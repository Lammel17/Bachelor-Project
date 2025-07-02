using UnityEngine;

public class LayingOnGround : MonoBehaviour
{
    [SerializeField] private Transform m_root;
    [SerializeField] private Transform m_pelvis;
    [SerializeField] private Transform m_chest;
    [SerializeField] private Transform m_centerBone;
    [SerializeField] private Vector2 m_minDistToGround = new Vector2(0.2f,0.5f);
    [SerializeField] private float m_raycastLenght = 0.8f;
    [SerializeField] private LayerMask m_environmentLayer;

    private Vector3 m_averageNormal = Vector3.zero;

    private float m_initialPelvisChestDistY = 0;


    private void Awake()
    {
        m_initialPelvisChestDistY = Mathf.Abs(m_pelvis.position.y - m_chest.position.y);
    }

    void Update()
    {
        if (m_chest.position.y > m_pelvis.position.y + 0.5f) return;

        float hits = 0;

        RaycastHit hit0;
        bool hasHit0 = false;
        Vector3 raycastOrigin0 = new Vector3(m_centerBone.position.x, m_centerBone.position.y, m_centerBone.position.z);
        if (Physics.Raycast(raycastOrigin0, Vector3.down, out hit0, m_raycastLenght, m_environmentLayer))
        { hasHit0 = true; hits++; }


        RaycastHit hit1;
        RaycastHit hit2;
        RaycastHit hit3;
        RaycastHit hit4;
        bool hasHit1 = false;
        bool hasHit2 = false;
        bool hasHit3 = false;
        bool hasHit4 = false;
        Vector3 raycastOrigin1 = new Vector3(m_centerBone.position.x + 0.2f, m_centerBone.position.y + 0.4f, m_centerBone.position.z);
        Vector3 raycastOrigin2 = new Vector3(m_centerBone.position.x - 0.2f, m_centerBone.position.y + 0.4f, m_centerBone.position.z);
        Vector3 raycastOrigin3 = new Vector3(m_centerBone.position.x, m_centerBone.position.y + 0.4f, m_centerBone.position.z + 0.2f);
        Vector3 raycastOrigin4 = new Vector3(m_centerBone.position.x, m_centerBone.position.y + 0.4f, m_centerBone.position.z - 0.2f);

        if (Physics.Raycast(raycastOrigin1, Vector3.down, out hit1, m_raycastLenght + 0.4f, m_environmentLayer))
        { hasHit1 = true; hits++; }
        if (Physics.Raycast(raycastOrigin2, Vector3.down, out hit2, m_raycastLenght + 0.4f, m_environmentLayer))
        { hasHit2 = true; hits++; }
        if (Physics.Raycast(raycastOrigin3, Vector3.down, out hit3, m_raycastLenght + 0.4f, m_environmentLayer))
        { hasHit3 = true; hits++; }
        if (Physics.Raycast(raycastOrigin4, Vector3.down, out hit4, m_raycastLenght + 0.4f, m_environmentLayer))
        { hasHit4 = true; hits++; }

        //Debug.Log(m_chest.position.y - m_pelvis.position.y);
        //Debug.Log(hits);

        if (hits < 2) return;

        if (hasHit0) m_averageNormal += hit0.normal;
        if (hasHit1) m_averageNormal += hit1.normal;
        if (hasHit2) m_averageNormal += hit2.normal;
        if (hasHit3) m_averageNormal += hit3.normal;
        if (hasHit4) m_averageNormal += hit4.normal;

        m_averageNormal = m_averageNormal.normalized;


        Quaternion rootRotOfAnimXY = Quaternion.LookRotation(-new Vector3(m_root.forward.x, 0, m_root.forward.z), Vector3.up);

        Quaternion desiredRootRot = Quaternion.FromToRotation(Vector3.up, m_averageNormal) * rootRotOfAnimXY;

        desiredRootRot = Quaternion.Slerp(desiredRootRot, m_root.rotation, Mathf.InverseLerp(m_minDistToGround.x, m_minDistToGround.y, m_chest.position.y - m_pelvis.position.y));

        m_root.rotation = desiredRootRot; 
    }
}
