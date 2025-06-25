using Unity.VisualScripting;
using UnityEngine;

public class FootPlacing : MonoBehaviour
{
    [SerializeField] private Transform m_footBoneLeft;
    [SerializeField] private Transform m_shinBoneLeft;
    [SerializeField] private Transform m_thighBoneLeft;
    [SerializeField] private Transform m_footBoneRight;
    [SerializeField] private Transform m_shinBoneRight;
    [SerializeField] private Transform m_thighBoneRight;
    [SerializeField] private Transform m_rootPelvis;


    [SerializeField] private LayerMask m_environmentLayer;
    [SerializeField] private float m_yOffsetToAnkle = 0;
    private float m_raycastHeightOffset = 0.6f;

    private float m_thighLenght = 0;
    private float m_shinLenght = 0;

    private Quaternion m_initialFootRot;


    void Awake()
    {
        m_initialFootRot = m_footBoneLeft.rotation;
        m_thighLenght = (m_shinBoneLeft.position - m_thighBoneLeft.position).magnitude;
        m_shinLenght = (m_shinBoneLeft.position - m_footBoneLeft.position).magnitude;
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


        //////////////////////





        Vector3 leftKneeNormal = Vector3.Cross(m_shinBoneLeft.position - m_thighBoneLeft.position, m_shinBoneLeft.position - leftFootPos).normalized;
        Vector3 leftThightUp = -Vector3.Cross(m_shinBoneLeft.position - m_thighBoneLeft.position, leftKneeNormal).normalized;
        Vector3 leftShinUp = -Vector3.Cross(leftFootPos - m_shinBoneLeft.position, leftKneeNormal).normalized;
        //Debug.DrawLine(m_shinBoneLeft.position - leftKneeNormal/2, m_shinBoneLeft.position + leftKneeNormal/2, Color.red);


        float leftHipFootDist = (leftFootPos - m_thighBoneLeft.position).magnitude;
        m_thighBoneLeft.rotation = Quaternion.LookRotation(leftFootPos - m_thighBoneLeft.position, leftThightUp) * Quaternion.LookRotation(Vector3.down);
        m_thighBoneLeft.RotateAround(m_thighBoneLeft.position, leftKneeNormal, CalculateAngle(m_thighLenght, m_shinLenght, leftHipFootDist));
        m_shinBoneLeft.rotation = Quaternion.LookRotation(leftFootPos - m_shinBoneLeft.position, leftShinUp) * Quaternion.LookRotation(Vector3.down);
        //m_shinBoneLeft.RotateAround(m_shinBoneLeft.position, leftKneeNormal, 90 - CalculateAngle(m_shinLenght, m_thighLenght, leftHipFootDist));


        //Debug.DrawLine(m_thighBoneLeft.position, m_thighBoneLeft.position + (m_footBoneLeft.position - m_thighBoneLeft.position).normalized * leftHipFootDist, Color.green);
        //Debug.DrawLine(m_shinBoneLeft.position, m_shinBoneLeft.position + (m_footBoneLeft.position - m_shinBoneLeft.position).normalized * m_shinLenght, Color.blue);
        //Debug.DrawLine(m_thighBoneLeft.position, m_thighBoneLeft.position + (m_shinBoneLeft.position - m_thighBoneLeft.position).normalized * m_thighLenght, Color.red);
        //Debug.DrawLine(m_thighBoneLeft.position, m_thighBoneLeft.position + Quaternion.LookRotation(leftFootPos - m_thighBoneLeft.position, Vector3.up) * Vector3.forward, Color.red);




        Vector3 rightKneeNormal = Vector3.Cross(m_shinBoneRight.position - m_thighBoneRight.position, m_shinBoneRight.position - rightFootPos).normalized;
        Vector3 rightThightUp = -Vector3.Cross(m_shinBoneRight.position - m_thighBoneRight.position, rightKneeNormal).normalized;
        Vector3 rightShinUp = -Vector3.Cross(rightFootPos - m_shinBoneRight.position, rightKneeNormal).normalized;

        float rightHipFootDist = (rightFootPos - m_thighBoneRight.position).magnitude;
        m_thighBoneRight.rotation = Quaternion.LookRotation(rightFootPos - m_thighBoneRight.position, rightThightUp) * Quaternion.LookRotation(Vector3.down);
        m_thighBoneRight.RotateAround(m_thighBoneRight.position, rightKneeNormal, CalculateAngle(m_thighLenght, m_shinLenght, rightHipFootDist));
        m_shinBoneRight.rotation = Quaternion.LookRotation(rightFootPos - m_shinBoneRight.position, rightShinUp) * Quaternion.LookRotation(Vector3.down);




        m_footBoneLeft.position = leftFootPos;
        m_footBoneRight.position = rightFootPos;

        m_footBoneLeft.rotation = leftFootRot;
        m_footBoneRight.rotation = rightFootRot;


    }


    private float CalculateAngle( float boneLenght, float otherBoneLenght, float hipFootDist)
    {
        float semiPerimeter = (boneLenght + otherBoneLenght + hipFootDist)/2;
        if (semiPerimeter <= boneLenght || semiPerimeter <= otherBoneLenght || semiPerimeter <= hipFootDist) return 0; 

        float area = Mathf.Sqrt( semiPerimeter * (semiPerimeter - boneLenght) * (semiPerimeter - otherBoneLenght) * (semiPerimeter - hipFootDist));
        float triangleHeight = area * 2 * (1/hipFootDist);

        return Mathf.Asin(triangleHeight / boneLenght) * Mathf.Rad2Deg;

    }


}
