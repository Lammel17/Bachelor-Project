using EditorAttributes;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class FootPlacing : MonoBehaviour
{
    [SerializeField] private Transform m_player;
    private float m_skinWidth = 0;
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
    [SerializeField] private float m_maxGrundHightDifference = 1f;
    [SerializeField] private float m_minHipGroundDist = 0.2f;
    private float m_raycastHeightOffset = 0.6f;

    private float m_lastLeftFootY;
    private float m_lastRightFootY;
    private float m_lastRootY;

    private Vector3 m_desiredLeftFootPos;
    private Vector3 m_desiredRightFootPos;
    private Quaternion m_desiredLeftFootRot;
    private Quaternion m_desiredRightFootRot;

    float m_leftAnkleHeight = 0;
    float m_rightAnkleHeight = 0;
    private float m_thighLenght = 0;
    private float m_shinLenght = 0;
    private float m_groundHightDifferenceWeighted = 0;
    bool m_isHightDifferenceOfGroundsIsTooBig = false;

    private float m_leftGroundHeight = 0;
    private float m_rightGroundHeight = 0;

    private Quaternion m_initialFootRot;

    [SerializeField][Range(0, 1)] private float m_weight;
    [SerializeField] private float m_footAdjustSpeed = 1f;

    void Awake()
    {
        m_skinWidth = m_player.GetComponent<CharacterController>().skinWidth;
        m_initialFootRot = m_footBoneLeft.rotation;
        m_thighLenght = (m_shinBoneLeft.position - m_thighBoneLeft.position).magnitude;
        m_shinLenght = (m_shinBoneLeft.position - m_footBoneLeft.position).magnitude;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        m_leftAnkleHeight = Mathf.Abs(m_footBoneLeft.position.y - (m_player.position.y - m_skinWidth));
        m_rightAnkleHeight = Mathf.Abs(m_footBoneRight.position.y - (m_player.position.y - m_skinWidth));

        //m_desiredLeftFootPos = m_footBoneLeft.position;
        //m_desiredRightFootPos = m_footBoneRight.position;
        m_desiredLeftFootRot = m_footBoneLeft.rotation;
        m_desiredRightFootRot = m_footBoneRight.rotation;
        m_groundHightDifferenceWeighted = m_maxGrundHightDifference;
        m_leftGroundHeight = m_footBoneLeft.position.y - m_leftAnkleHeight;
        m_rightGroundHeight = m_footBoneRight.position.y - m_rightAnkleHeight;

        float raycastLenght = 2.5f; //beware, if its too short, m_isHightDifferenceOfGroundsIsTooBig will be not correct
        bool hasGroundL = false;
        bool hasGroundR = false;
        RaycastHit hitL;
        RaycastHit hitR;
        Vector3 raycastOriginL = new Vector3(m_footBoneLeft.position.x, transform.position.y + m_raycastHeightOffset, m_footBoneLeft.position.z);
        Vector3 raycastOriginR = new Vector3(m_footBoneRight.position.x, transform.position.y + m_raycastHeightOffset, m_footBoneRight.position.z);
        if (Physics.Raycast(raycastOriginL, Vector3.down, out hitL, m_raycastHeightOffset * raycastLenght, m_environmentLayer))
            hasGroundL = true;
        if (Physics.Raycast(raycastOriginR, Vector3.down, out hitR, m_raycastHeightOffset * raycastLenght, m_environmentLayer))
            hasGroundR = true;
        Debug.DrawLine(raycastOriginL, raycastOriginL + Vector3.down * m_raycastHeightOffset * 2, Color.red);
        Debug.DrawLine(raycastOriginR, raycastOriginR + Vector3.down * m_raycastHeightOffset * 2, Color.red);


        if (hasGroundL)             m_leftGroundHeight = hitL.point.y;
        else if (hasGroundR)    {   m_leftGroundHeight = hitR.point.y;  hitL = hitR; }
        if (hasGroundR)             m_rightGroundHeight = hitR.point.y;
        else if (hasGroundL)    {   m_rightGroundHeight = hitL.point.y; hitR = hitL; }


        if (!hasGroundL && !hasGroundR) return;

        if (Mathf.Abs(m_leftGroundHeight - m_rightGroundHeight) > m_maxGrundHightDifference)
        {
            float weightForHightDiffTooBig = Mathf.Max(0, 2 - Mathf.Pow((Mathf.Abs(m_leftGroundHeight - m_rightGroundHeight) / m_maxGrundHightDifference), 2));
            m_groundHightDifferenceWeighted = m_maxGrundHightDifference * weightForHightDiffTooBig;
        }

        if (m_groundHightDifferenceWeighted != m_maxGrundHightDifference && m_leftGroundHeight < m_rightGroundHeight)       m_leftGroundHeight  = m_rightGroundHeight - m_groundHightDifferenceWeighted;
        else if (m_groundHightDifferenceWeighted != m_maxGrundHightDifference && m_leftGroundHeight > m_rightGroundHeight)  m_rightGroundHeight = m_leftGroundHeight - m_groundHightDifferenceWeighted;

        if (m_leftGroundHeight > m_rightGroundHeight)
        {
            float OriginalHipDistToGround = m_thighBoneLeft.position.y - m_leftGroundHeight;
            float notOriginalHipDistToGround = OriginalHipDistToGround - (m_leftGroundHeight - m_rightGroundHeight);
            m_weight = Mathf.InverseLerp(0.3f, 0.5f, notOriginalHipDistToGround); ////////////////////////////////////////////////////Diese zahlen siend noch magisch
            Debug.Log(notOriginalHipDistToGround);
            m_rightGroundHeight = Mathf.Lerp(m_leftGroundHeight, m_rightGroundHeight, m_weight);
        }

        if (m_leftGroundHeight < m_rightGroundHeight)
        {
            float OriginalHipDistToGround = m_thighBoneRight.position.y - m_rightGroundHeight;
            float notOriginalHipDistToGround = OriginalHipDistToGround - (m_rightGroundHeight - m_leftGroundHeight);
            m_weight = Mathf.InverseLerp(0.3f, 0.5f, notOriginalHipDistToGround);
            m_leftGroundHeight = Mathf.Lerp(m_rightGroundHeight, m_leftGroundHeight, m_weight);
        }

        //float OriginalDistToGround = Mathf.Max(Mathf.Min(m_thighBoneLeft.position.y - m_leftGroundHeight, m_thighBoneRight.position.y - m_rightGroundHeight ), m_minHipGroundDist);
        //float notOriginalDistToGround = Mathf.Max(Mathf.Max(m_thighBoneLeft.position.y - m_leftGroundHeight, m_thighBoneRight.position.y - m_rightGroundHeight ), m_minHipGroundDist);
        //Debug.Log(notOriginalDistToGround);

        CalculateAndSetHipHeight();

        CalculateDesiredFootPosAndRotationOnGround(ref hitL, ref hitR, ref hasGroundL, ref hasGroundR);

        CalculateAndSetThightAndShinRotations();

        SetFootPositionAndRotation();


    }






    private void CalculateDesiredFootPosAndRotationOnGround(ref RaycastHit hitL, ref RaycastHit hitR, ref bool hasGroundL, ref bool hasGroundR)
    {
        float leftY = Mathf.Lerp(m_lastLeftFootY, m_leftGroundHeight + m_leftAnkleHeight, Time.deltaTime * m_footAdjustSpeed);
        float rightY = Mathf.Lerp(m_lastRightFootY, m_rightGroundHeight + m_rightAnkleHeight, Time.deltaTime * m_footAdjustSpeed);
        m_desiredLeftFootPos = new Vector3(m_footBoneLeft.position.x, leftY, m_footBoneLeft.position.z);
        m_desiredRightFootPos = new Vector3(m_footBoneRight.position.x, rightY, m_footBoneRight.position.z);

        Quaternion desiredGroundRotL = Quaternion.FromToRotation(Vector3.up, hitL.normal) * Quaternion.LookRotation(-new Vector3(m_footBoneLeft.forward.x, 0, m_footBoneLeft.forward.z), Vector3.up) * m_initialFootRot;
        Quaternion desiredGroundRotR = Quaternion.FromToRotation(Vector3.up, hitR.normal) * Quaternion.LookRotation(-new Vector3(m_footBoneRight.forward.x, 0, m_footBoneRight.forward.z), Vector3.up) * m_initialFootRot;

        m_desiredLeftFootRot = Quaternion.Slerp(desiredGroundRotL, m_desiredLeftFootRot/*this is before change*/, Mathf.InverseLerp(0, m_footRotationSnappyness, (m_leftAnkleHeight - m_baseOffsetGroundToAnkleY)));
        m_desiredRightFootRot = Quaternion.Slerp(desiredGroundRotR, m_desiredRightFootRot/*this is before change*/, Mathf.InverseLerp(0, m_footRotationSnappyness, (m_rightAnkleHeight - m_baseOffsetGroundToAnkleY)));

    }




    private void CalculateAndSetHipHeight()
    {
        float lowerGround = Mathf.Min(m_leftGroundHeight, m_rightGroundHeight);
        float rootY = (lowerGround > m_root.position.y) ? m_root.position.y : lowerGround;
        rootY = Mathf.Lerp(m_lastRootY, rootY, Time.deltaTime * m_footAdjustSpeed);
        m_root.position = new Vector3(m_root.position.x, rootY, m_root.position.z);

        m_lastRootY = m_root.position.y;
    }






    private void CalculateAndSetThightAndShinRotations()
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

        m_lastLeftFootY = m_footBoneLeft.position.y;
        m_lastRightFootY = m_footBoneRight.position.y;
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
