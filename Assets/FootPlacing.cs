using Unity.VisualScripting;
using UnityEngine;

public class FootPlacing : MonoBehaviour
{
    [SerializeField] private Transform m_footBone;
    [SerializeField] private LayerMask m_environmentLayer;
    [SerializeField] private float m_yOffsetToAnkle = 0;
    private float m_raycastHeightOffset = 0.6f;

    private Quaternion m_initialFootRot;


    void Awake()
    {
        m_initialFootRot = m_footBone.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        RaycastHit hit;
        Vector3 raycastOrigin = new Vector3(m_footBone.position.x, transform.position.y + m_raycastHeightOffset, m_footBone.position.z);
        Debug.DrawLine(raycastOrigin, raycastOrigin + Vector3.down * m_raycastHeightOffset * 2, Color.red);
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, m_raycastHeightOffset * 2, m_environmentLayer))
        {
            m_footBone.position = new Vector3(m_footBone.position.x, hit.point.y + m_yOffsetToAnkle, m_footBone.position.z);
            m_footBone.rotation = Quaternion.LookRotation(-new Vector3(m_footBone.forward.x, 0, m_footBone.forward.z), Vector3.up) * Quaternion.FromToRotation(Vector3.up, hit.normal) * m_initialFootRot ;
            //Debug.Log(hit.normal);
        }

    }
}
