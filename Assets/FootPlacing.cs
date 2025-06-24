using Unity.VisualScripting;
using UnityEngine;

public class FootPlacing : MonoBehaviour
{
    [SerializeField] private Transform m_footBoneLeft;
    [SerializeField] private Transform m_footBoneRight;
    [SerializeField] private Transform m_rootPelvis;


    [SerializeField] private LayerMask m_environmentLayer;
    [SerializeField] private float m_yOffsetToAnkle = 0;
    private float m_raycastHeightOffset = 0.6f;

    private Quaternion m_initialFootRot;


    void Awake()
    {
        m_initialFootRot = m_footBoneLeft.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        Vector3 leftFootPos = m_footBoneLeft.position;
        Quaternion leftFootRot = m_footBoneLeft.rotation;
        Vector3 rightFootPos = m_footBoneRight.position;
        Quaternion rightFootRot = m_footBoneRight.rotation;

        RaycastHit hitL;
        Vector3 raycastOriginL = new Vector3(m_footBoneLeft.position.x, transform.position.y + m_raycastHeightOffset, m_footBoneLeft.position.z);
        Debug.DrawLine(raycastOriginL, raycastOriginL + Vector3.down * m_raycastHeightOffset * 2, Color.red);
        if (Physics.Raycast(raycastOriginL, Vector3.down, out hitL, m_raycastHeightOffset * 2, m_environmentLayer))
        {
            leftFootPos = new Vector3(m_footBoneLeft.position.x, hitL.point.y + m_yOffsetToAnkle, m_footBoneLeft.position.z);
            leftFootRot = Quaternion.FromToRotation(Vector3.up, hitL.normal) * Quaternion.LookRotation(-new Vector3(m_footBoneLeft.forward.x, 0, m_footBoneLeft.forward.z), Vector3.up) *m_initialFootRot ;
        }

        RaycastHit hitR;
        Vector3 raycastOriginR = new Vector3(m_footBoneRight.position.x, transform.position.y + m_raycastHeightOffset, m_footBoneRight.position.z);
        Debug.DrawLine(raycastOriginR, raycastOriginR + Vector3.down * m_raycastHeightOffset * 2, Color.red);
        if (Physics.Raycast(raycastOriginR, Vector3.down, out hitR, m_raycastHeightOffset * 2, m_environmentLayer))
        {
            rightFootPos = new Vector3(m_footBoneRight.position.x, hitR.point.y + m_yOffsetToAnkle, m_footBoneRight.position.z);
            rightFootRot = Quaternion.FromToRotation(Vector3.up, hitR.normal) * Quaternion.LookRotation(-new Vector3(m_footBoneRight.forward.x, 0, m_footBoneRight.forward.z), Vector3.up) * m_initialFootRot;
        }

        m_rootPelvis.position = new Vector3(m_rootPelvis.position.x, Mathf.Max(hitL.point.y, hitR.point.y) - Mathf.Abs(hitL.point.y - hitR.point.y), m_rootPelvis.position.z);

        m_footBoneLeft.position = leftFootPos;
        m_footBoneLeft.rotation = leftFootRot;
        m_footBoneRight.position = rightFootPos;
        m_footBoneRight.rotation = rightFootRot;




    }
}
