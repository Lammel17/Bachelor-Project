using EditorAttributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class FootPlacing : MonoBehaviour
{
    [SerializeField] private bool m_applyFootRot = true;
    [Space]
    [SerializeField] private Transform m_player;
    private float m_skinWidth = 0;
    [Space] 
    [SerializeField] private Transform m_leftFootUp;
    [SerializeField] private Transform m_rightFootUp;
    [Space]
    [SerializeField] private Transform m_footBoneLeft;
    [SerializeField] private Transform m_shinBoneLeft;
    [SerializeField] private Transform m_thighBoneLeft;
    [SerializeField] private Transform m_footBoneRight;
    [SerializeField] private Transform m_shinBoneRight;
    [SerializeField] private Transform m_thighBoneRight;
    [SerializeField] private Transform m_root;

    [SerializeField] private float m_raycastLenght = 2.5f; //beware, if its too short, m_isHightDifferenceOfGroundsIsTooBig will be not correct

    [SerializeField] private LayerMask m_environmentLayer;
    [SerializeField] private float m_baseOffsetGroundToAnkleY = 0;
    [SerializeField] private float m_footRotationSnappyness = 0.01f;
    //[SerializeField] private float m_maxGroundHightDifference = 0.5f;
    [SerializeField][GD.MinMaxSlider.MinMaxSlider(0, 1)] private Vector2 m_minDistHipGround = new Vector2(0.3f, 0.5f);
    private float m_raycastHeightOffset = 0.6f;

    private float m_lastLeftFootY;
    private float m_lastRightFootY;
    Quaternion m_lastLeftFootRot;
    Quaternion m_lastRightFootRot;
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
    [SerializeField] private float m_footRotAdjustSpeed = 1f;
    [SerializeField] private float m_rootAdjustSpeed = 1f;

    void Awake()
    {
        m_skinWidth = m_player.GetComponent<CharacterController>().skinWidth;
        m_initialFootRot = m_footBoneLeft.rotation;
        m_thighLenght = (m_shinBoneLeft.position - m_thighBoneLeft.position).magnitude;
        m_shinLenght = (m_shinBoneLeft.position - m_footBoneLeft.position).magnitude;

        m_lastLeftFootY = m_footBoneLeft.position.y;
        m_lastRightFootY = m_footBoneRight.position.y;
        m_lastLeftFootRot = m_footBoneLeft.rotation;
        m_lastRightFootRot = m_footBoneRight.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        m_leftAnkleHeight = Mathf.Abs(m_footBoneLeft.position.y - (m_player.position.y - m_skinWidth) + 0.003f);
        m_rightAnkleHeight = Mathf.Abs(m_footBoneRight.position.y - (m_player.position.y - m_skinWidth) + 0.003f);

        m_desiredLeftFootPos = m_footBoneLeft.position;
        m_desiredRightFootPos = m_footBoneRight.position;
        m_desiredLeftFootRot = m_footBoneLeft.rotation;
        m_desiredRightFootRot = m_footBoneRight.rotation;
        //m_groundHightDifferenceWeighted = m_maxGroundHightDifference;
        m_leftGroundHeight = m_footBoneLeft.position.y - m_leftAnkleHeight;
        m_rightGroundHeight = m_footBoneRight.position.y - m_rightAnkleHeight;

        #region Setting Ground Height

        bool hasGroundL = false;
        bool hasGroundR = false;
        RaycastHit hitL;
        RaycastHit hitR;
        Vector3 raycastOriginL = new Vector3(m_footBoneLeft.position.x, transform.position.y + m_raycastHeightOffset, m_footBoneLeft.position.z);
        Vector3 raycastOriginR = new Vector3(m_footBoneRight.position.x, transform.position.y + m_raycastHeightOffset, m_footBoneRight.position.z);
        if (Physics.Raycast(raycastOriginL, Vector3.down, out hitL, m_raycastHeightOffset * m_raycastLenght, m_environmentLayer))
            hasGroundL = true;
        if (Physics.Raycast(raycastOriginR, Vector3.down, out hitR, m_raycastHeightOffset * m_raycastLenght, m_environmentLayer))
            hasGroundR = true;
        Debug.DrawLine(raycastOriginL, raycastOriginL + Vector3.down * m_raycastHeightOffset * 2, Color.red);
        Debug.DrawLine(raycastOriginR, raycastOriginR + Vector3.down * m_raycastHeightOffset * 2, Color.red);


        if (hasGroundL) m_leftGroundHeight = hitL.point.y;
        else { m_leftGroundHeight = m_player.position.y - m_skinWidth;  }
        if (hasGroundR) m_rightGroundHeight = hitR.point.y;
        else   { m_rightGroundHeight = m_player.position.y - m_skinWidth; }

        if (!hasGroundL && !hasGroundR) return;
        //if (hasGroundL && hasGroundR && Mathf.Abs(hitL.point.y - hitR.point.y) < 0.003f && Vector3.Angle(hitL.normal, Vector3.up) < 0.5f && Vector3.Angle(hitR.normal, Vector3.up) < 0.5f) { Debug.Log("EEEEEEEE"); return; } // cant use, because it needs to be smoothed

        if (m_leftGroundHeight > m_rightGroundHeight)   AdjustGroundHeightWhenHipgetsTooCloseToHigherFootGround(ref m_leftGroundHeight, ref m_rightGroundHeight, m_thighBoneLeft.position.y, m_thighBoneRight.position.y);
        else                                            AdjustGroundHeightWhenHipgetsTooCloseToHigherFootGround(ref m_rightGroundHeight, ref m_leftGroundHeight, m_thighBoneRight.position.y, m_thighBoneLeft.position.y );

        void AdjustGroundHeightWhenHipgetsTooCloseToHigherFootGround(ref float higherGroundHeight, ref float lowerGroundHeight, float thightBoneHeightOfHigherGround, float thightBoneHeightOfLowerGround)
        {   
            // those values are before they were set, so the thightbone is higher bc ist befor its set down.
            float closerHipDistToGround = thightBoneHeightOfHigherGround - higherGroundHeight - (higherGroundHeight - lowerGroundHeight);
            m_weight = Mathf.InverseLerp(m_minDistHipGround.x, m_minDistHipGround.y, closerHipDistToGround);  
            lowerGroundHeight = Mathf.Lerp(m_player.position.y - m_skinWidth, lowerGroundHeight, m_weight);
        }

        #endregion



        CalculateAndSetHipHeight();

        CalculateDesiredFootPosAndRotationOnGround(ref hitL, ref hitR, ref hasGroundL, ref hasGroundR);

        CalculateAndSetThightAndShinRotations();

        SetFootPositionAndRotation();


    }






    private void CalculateAndSetHipHeight()
    {
        float lowerGround = Mathf.Min(m_leftGroundHeight, m_rightGroundHeight);
        float rootY = (lowerGround > m_root.position.y) ? m_root.position.y : lowerGround;

        rootY = Mathf.Lerp(m_lastRootY, rootY, Time.deltaTime * m_rootAdjustSpeed);

        m_root.position = new Vector3(m_root.position.x, rootY, m_root.position.z);

        m_lastRootY = m_root.position.y;
    }






    private void CalculateDesiredFootPosAndRotationOnGround(ref RaycastHit hitL, ref RaycastHit hitR, ref bool hasGroundL, ref bool hasGroundR)
    {
        float leftY = Mathf.Lerp(m_lastLeftFootY, m_leftGroundHeight + m_leftAnkleHeight, Time.deltaTime * m_footAdjustSpeed);
        float rightY = Mathf.Lerp(m_lastRightFootY, m_rightGroundHeight +  m_rightAnkleHeight, Time.deltaTime * m_footAdjustSpeed);

        m_desiredLeftFootPos = new Vector3(m_footBoneLeft.position.x, leftY, m_footBoneLeft.position.z);
        m_desiredRightFootPos = new Vector3(m_footBoneRight.position.x, rightY, m_footBoneRight.position.z);

        m_lastLeftFootY = leftY;
        m_lastRightFootY = rightY;

        if (!m_applyFootRot) return; ////IDEA: maybe only when moving

        Quaternion leftFootRotOfAnim = Quaternion.LookRotation(-new Vector3(m_footBoneLeft.forward.x, 0, m_footBoneLeft.forward.z), Vector3.up) * m_initialFootRot;
        Quaternion rightFootRotOfAnim = Quaternion.LookRotation(-new Vector3(m_footBoneRight.forward.x, 0, m_footBoneRight.forward.z), Vector3.up) * m_initialFootRot;
        Quaternion desiredGroundRotL = Quaternion.FromToRotation(Vector3.up, hasGroundL ?  hitL.normal : Vector3.up) * leftFootRotOfAnim;
        Quaternion desiredGroundRotR = Quaternion.FromToRotation(Vector3.up, hasGroundR ? hitR.normal : Vector3.up) * rightFootRotOfAnim;

        float leftSlerpFactorByRotationOrDist =  Mathf.Max(Mathf.InverseLerp(0, m_footRotationSnappyness, (m_leftAnkleHeight  - m_baseOffsetGroundToAnkleY)),       Mathf.InverseLerp(1, 7, Vector3.Angle(Vector3.up, m_leftFootUp.up)));
        float rightSlerpFactorByRotationOrDist = Mathf.Max(Mathf.InverseLerp(0, m_footRotationSnappyness, (m_rightAnkleHeight - m_baseOffsetGroundToAnkleY)),       Mathf.InverseLerp(1, 7, Vector3.Angle(Vector3.up, m_rightFootUp.up)));
        //Debug.Log(Vector3.Angle(Vector3.up, test1.up));
 
        m_desiredLeftFootRot = Quaternion.Slerp(desiredGroundRotL, m_footBoneLeft.rotation , leftSlerpFactorByRotationOrDist);
        m_desiredRightFootRot = Quaternion.Slerp(desiredGroundRotR, m_footBoneRight.rotation , rightSlerpFactorByRotationOrDist);

        m_desiredLeftFootRot = Quaternion.Slerp(m_lastLeftFootRot, m_desiredLeftFootRot, m_footRotAdjustSpeed * Time.deltaTime);
        m_desiredRightFootRot = Quaternion.Slerp(m_lastRightFootRot, m_desiredRightFootRot, m_footRotAdjustSpeed * Time.deltaTime);


        m_lastLeftFootRot = m_desiredLeftFootRot;
        m_lastRightFootRot = m_desiredRightFootRot;

    }






    private void CalculateAndSetThightAndShinRotations()
    {
        Vector3 leftKneeNormal = Vector3.Cross(m_shinBoneLeft.position - m_thighBoneLeft.position, m_shinBoneLeft.position - m_footBoneLeft.position).normalized;
        Vector3 leftThightUp = -Vector3.Cross(m_shinBoneLeft.position - m_thighBoneLeft.position, leftKneeNormal).normalized;
        Vector3 leftShinUp = -Vector3.Cross(m_footBoneLeft.position - m_shinBoneLeft.position, leftKneeNormal).normalized;
        float leftHipFootDist = (m_desiredLeftFootPos - m_thighBoneLeft.position).magnitude;

        m_thighBoneLeft.rotation = Quaternion.LookRotation(m_desiredLeftFootPos - m_thighBoneLeft.position, leftThightUp) * Quaternion.LookRotation(Vector3.down);
        m_thighBoneLeft.RotateAround(m_thighBoneLeft.position, leftKneeNormal, CalculateAngle(m_thighLenght, m_shinLenght, leftHipFootDist));
        m_shinBoneLeft.rotation = Quaternion.LookRotation(m_desiredLeftFootPos - m_shinBoneLeft.position, leftShinUp) * Quaternion.LookRotation(Vector3.down);


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
        //unnecessary
        //m_footBoneLeft.position = m_desiredLeftFootPos; 
        //m_footBoneRight.position = m_desiredRightFootPos;

        m_footBoneLeft.rotation = m_desiredLeftFootRot;
        m_footBoneRight.rotation = m_desiredRightFootRot;
    }






    private float CalculateAngle( float boneLenght, float otherBoneLenght, float hipFootDist)
    {
        float semiPerimeter = (boneLenght + otherBoneLenght + hipFootDist)/2;
        if (boneLenght + otherBoneLenght <= hipFootDist) { /*Debug.Log(boneLenght + otherBoneLenght); Debug.Log(hipFootDist);*/ return 0.01f; } 

        float area = Mathf.Sqrt( semiPerimeter * (semiPerimeter - boneLenght) * (semiPerimeter - otherBoneLenght) * (semiPerimeter - hipFootDist));
        float triangleHeight = area * 2 * (1/hipFootDist);

        return Mathf.Asin(triangleHeight / boneLenght) * Mathf.Rad2Deg;

    }


}
