using EditorAttributes;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class FootPlacing : MonoBehaviour
{
    [SerializeField] private Transform m_player;
    [Space] 
    [SerializeField] private Transform m_footBoneLeft;
    [SerializeField] private Transform m_shinBoneLeft;
    [SerializeField] private Transform m_thighBoneLeft;
    [SerializeField] private Transform m_footBoneRight;
    [SerializeField] private Transform m_shinBoneRight;
    [SerializeField] private Transform m_thighBoneRight;
    [SerializeField] private Transform m_root;



    [SerializeField] private LayerMask m_environmentLayer;
    [SerializeField] private float m_baseOffsetGroundToAnkleY = 0;
    [SerializeField] private float m_footRotationSnappyness = 0;
    [SerializeField] private float m_maxFeetHightDifference = 1f;
    private float m_raycastHeightOffset = 0.6f;
    [SerializeField] [EditorAttributes.ReadOnly] [Range(0,1)] private float m_weight = 1f;

    private Vector3 m_desiredLeftFootPos;
    private Vector3 m_desiredRightFootPos;
    private Quaternion m_desiredLeftFootRot;
    private Quaternion m_desiredRightFootRot;

    float m_leftAnkleHeight = 0;
    float m_rightAnkleHeight = 0;
    private float m_thighLenght = 0;
    private float m_shinLenght = 0;
    private float m_hightDifferenceWeight = 0;
    bool m_isHightDifferenceOfFeetIsTooBig = false;

    private float m_leftGroundHeight = 0;
    private float m_rightGroundHeight = 0;

    private Quaternion m_initialFootRot;


    void Awake()
    {
        m_initialFootRot = m_footBoneLeft.rotation;
        m_thighLenght = (m_shinBoneLeft.position - m_thighBoneLeft.position).magnitude;
        m_shinLenght = (m_shinBoneLeft.position - m_footBoneLeft.position).magnitude;
        m_hightDifferenceWeight = m_maxFeetHightDifference;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        m_leftAnkleHeight = Mathf.Abs(m_footBoneLeft.position.y - m_player.position.y);
        m_rightAnkleHeight = Mathf.Abs(m_footBoneRight.position.y - m_player.position.y);

        m_desiredLeftFootPos = m_footBoneLeft.position;
        m_desiredRightFootPos = m_footBoneRight.position;
        m_desiredLeftFootRot = m_footBoneLeft.rotation;
        m_desiredRightFootRot = m_footBoneRight.rotation;
        m_hightDifferenceWeight = m_maxFeetHightDifference;


        CalculateDesiredFootPosAndRotationOnGround();

        CalculateHipHeight();

        SetThightAndShinRotations();

        SetFootPositionAndRotation();


    }






    private void CalculateDesiredFootPosAndRotationOnGround( )
    {
        float raycastLenght = 2.5f; //beware, if its too short, m_weight will not be smooth
        bool hasGroundL = false;
        bool hasGroundR = false;
        RaycastHit hitL;
        Vector3 raycastOriginL = new Vector3(m_footBoneLeft.position.x, transform.position.y + m_raycastHeightOffset, m_footBoneLeft.position.z);
        Debug.DrawLine(raycastOriginL, raycastOriginL + Vector3.down * m_raycastHeightOffset * 2, Color.red);
        m_leftGroundHeight = m_footBoneLeft.position.y - m_leftAnkleHeight;
        if (Physics.Raycast(raycastOriginL, Vector3.down, out hitL, m_raycastHeightOffset * raycastLenght, m_environmentLayer))
            hasGroundL = true;
        
        RaycastHit hitR;
        Vector3 raycastOriginR = new Vector3(m_footBoneRight.position.x, transform.position.y + m_raycastHeightOffset, m_footBoneRight.position.z);
        Debug.DrawLine(raycastOriginR, raycastOriginR + Vector3.down * m_raycastHeightOffset * 2, Color.red);
        m_rightGroundHeight = m_footBoneRight.position.y - m_rightAnkleHeight;
        if (Physics.Raycast(raycastOriginR, Vector3.down, out hitR, m_raycastHeightOffset * raycastLenght, m_environmentLayer))
            hasGroundR = true;

        if (!hasGroundL && !hasGroundR) return;

        if (hasGroundL) m_leftGroundHeight = hitL.point.y;
        else if (hasGroundR) { m_leftGroundHeight = hitR.point.y; hitL = hitR; }
        if (hasGroundR) m_rightGroundHeight = hitR.point.y;
        else if (hasGroundL) { m_rightGroundHeight = hitL.point.y; hitR = hitL; }

        m_isHightDifferenceOfFeetIsTooBig = Mathf.Abs(m_leftGroundHeight - m_rightGroundHeight) > m_maxFeetHightDifference;

        if (m_isHightDifferenceOfFeetIsTooBig)
        {
            m_weight = Mathf.Max(0, 2 - Mathf.Pow((Mathf.Abs(m_leftGroundHeight - m_rightGroundHeight) / m_maxFeetHightDifference),2));
            m_hightDifferenceWeight = m_maxFeetHightDifference * m_weight;
            Debug.Log(m_weight);

            if (m_leftGroundHeight < m_rightGroundHeight)
            {
                m_desiredLeftFootPos = new Vector3(m_footBoneLeft.position.x, m_rightGroundHeight - m_hightDifferenceWeight + m_leftAnkleHeight, m_footBoneLeft.position.z);
                m_desiredRightFootPos = new Vector3(m_footBoneRight.position.x, m_rightGroundHeight + m_rightAnkleHeight, m_footBoneRight.position.z);
            }
            else
            {
                m_desiredLeftFootPos = new Vector3(m_footBoneLeft.position.x, m_leftGroundHeight + m_leftAnkleHeight, m_footBoneLeft.position.z);
                m_desiredRightFootPos = new Vector3(m_footBoneRight.position.x, m_leftGroundHeight - m_hightDifferenceWeight + m_rightAnkleHeight, m_footBoneRight.position.z);
            }
        }
        else
        {
            m_desiredLeftFootPos = new Vector3(m_footBoneLeft.position.x, m_leftGroundHeight + m_leftAnkleHeight, m_footBoneLeft.position.z);
            m_desiredRightFootPos = new Vector3(m_footBoneRight.position.x, m_rightGroundHeight + m_rightAnkleHeight, m_footBoneRight.position.z);

            Quaternion desiredGroundRotL = Quaternion.FromToRotation(Vector3.up, hitL.normal) * Quaternion.LookRotation(-new Vector3(m_footBoneLeft.forward.x, 0, m_footBoneLeft.forward.z), Vector3.up) * m_initialFootRot;
            Quaternion desiredGroundRotR = Quaternion.FromToRotation(Vector3.up, hitR.normal) * Quaternion.LookRotation(-new Vector3(m_footBoneRight.forward.x, 0, m_footBoneRight.forward.z), Vector3.up) * m_initialFootRot;

            Debug.Log((m_leftAnkleHeight - m_baseOffsetGroundToAnkleY));
            m_desiredLeftFootRot = Quaternion.Slerp(desiredGroundRotL, m_desiredLeftFootRot, Mathf.InverseLerp(0, m_footRotationSnappyness, (m_leftAnkleHeight - m_baseOffsetGroundToAnkleY) ));
            m_desiredRightFootRot = Quaternion.Slerp(desiredGroundRotL, m_desiredRightFootRot, Mathf.InverseLerp(0, m_footRotationSnappyness, (m_rightAnkleHeight - m_baseOffsetGroundToAnkleY)));
        }


    }




    private void CalculateHipHeight()
    {
        if (m_isHightDifferenceOfFeetIsTooBig)
        {
            m_root.position = new Vector3(m_root.position.x, Mathf.Max(m_leftGroundHeight, m_rightGroundHeight) - m_hightDifferenceWeight, m_root.position.z);
        }
        else
            m_root.position = new Vector3(m_root.position.x, Mathf.Max(m_leftGroundHeight, m_rightGroundHeight) - Mathf.Abs(m_leftGroundHeight - m_rightGroundHeight), m_root.position.z);
    }






    private void SetThightAndShinRotations()
    {
        Vector3 leftKneeNormal = Vector3.Cross(m_shinBoneLeft.position - m_thighBoneLeft.position, m_shinBoneLeft.position - m_footBoneLeft.position).normalized;
        Vector3 leftThightUp = -Vector3.Cross(m_shinBoneLeft.position - m_thighBoneLeft.position, leftKneeNormal).normalized;
        Vector3 leftShinUp = -Vector3.Cross(m_footBoneLeft.position - m_shinBoneLeft.position, leftKneeNormal).normalized;
        //Debug.DrawLine(m_shinBoneLeft.position - leftKneeNormal/2, m_shinBoneLeft.position + leftKneeNormal/2, Color.red);
        float leftHipFootDist = (m_desiredLeftFootPos - m_thighBoneLeft.position).magnitude;


        m_thighBoneLeft.rotation = Quaternion.LookRotation(m_desiredLeftFootPos - m_thighBoneLeft.position, leftThightUp) * Quaternion.LookRotation(Vector3.down);
        m_thighBoneLeft.RotateAround(m_thighBoneLeft.position, leftKneeNormal, CalculateAngle(m_thighLenght, m_shinLenght, leftHipFootDist));
        m_shinBoneLeft.rotation = Quaternion.LookRotation(m_desiredLeftFootPos - m_shinBoneLeft.position, leftShinUp) * Quaternion.LookRotation(Vector3.down);
        //m_shinBoneLeft.RotateAround(m_shinBoneLeft.position, leftKneeNormal, 90 - CalculateAngle(m_shinLenght, m_thighLenght, leftHipFootDist));


        Vector3 rightKneeNormal = Vector3.Cross(m_shinBoneRight.position - m_thighBoneRight.position, m_shinBoneRight.position - m_footBoneRight.position).normalized;
        Vector3 rightThightUp = -Vector3.Cross(m_shinBoneRight.position - m_thighBoneRight.position, rightKneeNormal).normalized;
        Vector3 rightShinUp = -Vector3.Cross(m_footBoneRight.position - m_shinBoneRight.position, rightKneeNormal).normalized;
        float rightHipFootDist = (m_desiredRightFootPos - m_thighBoneRight.position).magnitude;

        m_thighBoneRight.rotation = Quaternion.LookRotation(m_desiredRightFootPos - m_thighBoneRight.position, rightThightUp) * Quaternion.LookRotation(Vector3.down);
        m_thighBoneRight.RotateAround(m_thighBoneRight.position, rightKneeNormal, CalculateAngle(m_thighLenght, m_shinLenght, rightHipFootDist));
        m_shinBoneRight.rotation = Quaternion.LookRotation(m_desiredRightFootPos - m_shinBoneRight.position, rightShinUp) * Quaternion.LookRotation(Vector3.down);

    }






    private void SetFootPositionAndRotation()
    {
        m_footBoneLeft.position = m_desiredLeftFootPos;
        m_footBoneRight.position = m_desiredRightFootPos;

        m_footBoneLeft.rotation = m_desiredLeftFootRot;
        m_footBoneRight.rotation = m_desiredRightFootRot;
    }






    private float CalculateAngle( float boneLenght, float otherBoneLenght, float hipFootDist)
    {
        float semiPerimeter = (boneLenght + otherBoneLenght + hipFootDist)/2;
        if (boneLenght + otherBoneLenght <= hipFootDist) return 0; 

        float area = Mathf.Sqrt( semiPerimeter * (semiPerimeter - boneLenght) * (semiPerimeter - otherBoneLenght) * (semiPerimeter - hipFootDist));
        float triangleHeight = area * 2 * (1/hipFootDist);

        return Mathf.Asin(triangleHeight / boneLenght) * Mathf.Rad2Deg;

    }


}
