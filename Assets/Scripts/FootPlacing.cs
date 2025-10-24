using EditorAttributes;
using GD.MinMaxSlider;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class FootPlacing : MonoBehaviour
{
    [Tooltip("turn foot rotation on and off, because this takes the most calculations")]
    [SerializeField] private bool m_stopAll = false;
    [SerializeField] private bool m_applyFootRot = true;
    [SerializeField] private bool m_applyHipHeight = true;
    [SerializeField] private bool m_applyInverserseKinematics = true;
    [Space]
    [SerializeField] private Transform m_player;
    [Space] 
    [SerializeField] private Transform m_leftFootUpHelper;
    [SerializeField] private Transform m_rightFootUpHelper;
    [Space]
    [SerializeField] private Transform m_footBoneLeft;
    [SerializeField] private Transform m_shinBoneLeft;
    [SerializeField] private Transform m_thighBoneLeft;
    [Space]
    [SerializeField] private Transform m_footBoneRight;
    [SerializeField] private Transform m_shinBoneRight;
    [SerializeField] private Transform m_thighBoneRight;
    [Space]
    [SerializeField] private Transform m_rootBone;
    [Space]
    [Space]

    [SerializeField] private float m_raycastLenght = 2.5f; //beware, if its too short, m_isHightDifferenceOfGroundsIsTooBig will be not correct

    [SerializeField] private LayerMask m_environmentLayer;
    [Space]
    [Space]
    [Tooltip("[Y Axis Dist of original ground to original Ankle heigh] Dist below x threshhold: considered to snap to ground normal. Dist above y threshhold: considered not snapped to ground normal. (best case: (0, 0.01f))")]
    [SerializeField][GD.MinMaxSlider.MinMaxSlider(0, 1)] private Vector2 m_minFootDistToOrigHight = new Vector2(0, 0.05f);
    [Tooltip("[Angle in degree to World.up] Angle below x threshhold: considered to snap to ground normal. Angle above y threshhold: considered not snapped to ground normal. (best case: (1, 7f))")]
    [SerializeField][GD.MinMaxSlider.MinMaxSlider(0, 90)] private Vector2 m_minFootAngleToOrigAngle = new Vector2(1, 7f);
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_leftFootIsGroundedWeight = 1;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_rightFootIsGroundedWeight = 1;
    [Space]
    [Space]
    [Tooltip("this is the distance between the footBone origin (the ankle) and the ground when the foot is perfectly standing")]
    [SerializeField] private float m_baseOffsetGroundToAnkleY = 0;
    [Tooltip("[Y Axis Dist between the hip and the highest ground] Dist below x threshhold: considered as hip is too close to the new higher ground. Dist above y threshhold: considered as hip is far enough away of the higher ground. (best case: (0.3f, 0.5f))")]
    [SerializeField][GD.MinMaxSlider.MinMaxSlider(0, 1)] private Vector2 m_minDistHipGround = new Vector2(0.3f, 0.5f);
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_overallWeight = 1;
    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_weightByLayingOnGround = 1;
    [Space]
    [Space]
    [SerializeField] private float m_footHeightAdjustSpeed = 60f;
    [SerializeField] private float m_footRotationAdjustSpeed = 30f;
    [SerializeField] private float m_rootHeightAdjustSpeed = 30f;

    private float m_skinWidth = 0;
    private float m_maxStepHeight = 0;
    private Vector2 m_footTooHighAboveGroundToMatterThreshhold = Vector2.zero;

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

    private float m_leftGroundHeight = 0;
    private float m_rightGroundHeight = 0;

    private Quaternion m_initialFootRot;

    [Space]
    [SerializeField] private SimpleRetargeting m_retargetingScript;


    //FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF for the future to implement: make the script stop for entitys far away,since its not needed when they far away

    void Awake()
    {
        m_skinWidth = m_player.GetComponent<CharacterController>().skinWidth;
        m_maxStepHeight = m_player.GetComponent<CharacterController>().stepOffset;
        m_initialFootRot = m_footBoneLeft.rotation;
        m_thighLenght = (m_shinBoneLeft.position - m_thighBoneLeft.position).magnitude;
        m_shinLenght = (m_shinBoneLeft.position - m_footBoneLeft.position).magnitude;

        m_lastLeftFootY = m_footBoneLeft.position.y;
        m_lastRightFootY = m_footBoneRight.position.y;
        m_lastLeftFootRot = m_footBoneLeft.rotation;
        m_lastRightFootRot = m_footBoneRight.rotation;

        m_footTooHighAboveGroundToMatterThreshhold = new Vector2(m_maxStepHeight, m_maxStepHeight * 2);
    }




    public void SetWeightActive(bool active)
    {
        if (active) 
            m_overallWeight  = 1;
        else
            m_overallWeight = 0;

    }

    public void SetWeightByLayingOnGround(float weight)
    {
        m_weightByLayingOnGround = weight;
    }





    void LateUpdate()
    {
        if (m_retargetingScript != null) m_retargetingScript.DoTheRetargeting();

        if (m_stopAll)
            return;
        m_leftAnkleHeight = Mathf.Abs(m_footBoneLeft.position.y - (m_player.position.y - m_skinWidth) + 0.003f);
        m_rightAnkleHeight = Mathf.Abs(m_footBoneRight.position.y - (m_player.position.y - m_skinWidth) + 0.003f);

        m_desiredLeftFootPos = m_footBoneLeft.position;
        m_desiredRightFootPos = m_footBoneRight.position;
        m_desiredLeftFootRot = m_footBoneLeft.rotation;
        m_desiredRightFootRot = m_footBoneRight.rotation;
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


        if (m_leftGroundHeight > m_rightGroundHeight)   WeightGroundByConditions(ref m_leftGroundHeight, ref m_rightGroundHeight, ref m_leftAnkleHeight, ref m_rightAnkleHeight, m_thighBoneLeft.position.y, m_thighBoneRight.position.y);
        else                                            WeightGroundByConditions(ref m_rightGroundHeight, ref m_leftGroundHeight, ref m_rightAnkleHeight, ref m_leftAnkleHeight, m_thighBoneRight.position.y, m_thighBoneLeft.position.y );

        void WeightGroundByConditions(ref float higherGroundHeight, ref float lowerGroundHeight, ref float ankleHeightOfHigherGround, ref float ankleHeightOfLowerGround, float thightBoneHeightOfHigherGround, float thightBoneHeightOfLowerGround)
        {   
            // those values are before they were set, so the thightbone is higher bc ist befor its set down.
            float closerHipDistToGround = thightBoneHeightOfHigherGround - higherGroundHeight - (higherGroundHeight - lowerGroundHeight);

            // for the case that booth feet would be projected lower than the characterController StepHeightOffset, then it should not 
            float weight = Mathf.InverseLerp( -(m_maxStepHeight - 0.05f), -(m_maxStepHeight + 0.05f),higherGroundHeight - (m_player.position.y - m_skinWidth));


            // for the case that the hightened foot side is too close to the hip, then the character should be liftet
            float weightForLowerGroundHeight = Mathf.Max(Mathf.InverseLerp(m_minDistHipGround.y, m_minDistHipGround.x, closerHipDistToGround), weight);
            lowerGroundHeight = Mathf.Lerp(lowerGroundHeight, m_player.position.y - m_skinWidth,  weightForLowerGroundHeight);
            // for the case that the lower foot is way above ground to matter at all
            float weightForLowerGroundHeight_footTooHighAboveGround = Mathf.InverseLerp(m_footTooHighAboveGroundToMatterThreshhold.x, m_footTooHighAboveGroundToMatterThreshhold.y * 2, ankleHeightOfLowerGround);
            lowerGroundHeight = Mathf.Lerp(lowerGroundHeight,  m_player.position.y - m_skinWidth, weightForLowerGroundHeight_footTooHighAboveGround);


            // for the case that the higher foot would be projected on a platform higher than the characterController StepHeightOffset, then it should not
            float weightForHigherGroundHeight = Mathf.Max(Mathf.InverseLerp(m_maxStepHeight - 0.1f, m_maxStepHeight, higherGroundHeight - (m_player.position.y - m_skinWidth)), weight);
            higherGroundHeight = Mathf.Lerp(higherGroundHeight, m_player.position.y - m_skinWidth, weightForHigherGroundHeight);
            // for the case that the higher foot is way above ground to matter at all
            float weightForHigherGroundHeight_footTooHighAboveGround = Mathf.InverseLerp(m_footTooHighAboveGroundToMatterThreshhold.x, m_footTooHighAboveGroundToMatterThreshhold.y * 2, ankleHeightOfHigherGround);
            higherGroundHeight = Mathf.Lerp(higherGroundHeight, m_player.position.y - m_skinWidth, weightForHigherGroundHeight_footTooHighAboveGround);

        }

        #endregion


        Vector3 rootPosByAnim = m_rootBone.position;
        float weighting = Mathf.Min(m_overallWeight, m_weightByLayingOnGround);
        
        CalculateAndSetHipHeight();
        m_rootBone.position = Vector3.Lerp(rootPosByAnim, m_rootBone.position, m_weightByLayingOnGround);

        CalculateDesiredFootPosAndRotationOnGround(ref hitL, ref hitR, ref hasGroundL, ref hasGroundR);
        m_desiredLeftFootPos = Vector3.Lerp(m_footBoneLeft.position, m_desiredLeftFootPos, m_weightByLayingOnGround);
        m_desiredRightFootPos = Vector3.Lerp(m_footBoneRight.position, m_desiredRightFootPos, m_weightByLayingOnGround);

        CalculateAndSetThightAndShinRotations();

    }






    private void CalculateAndSetHipHeight()
    {
        if (!m_applyHipHeight)
            return;

        float lowerGround = 0;
        float ankleHightOflowerGround = 0;
        if (m_leftGroundHeight <= m_rightGroundHeight)
        {
            lowerGround = m_leftGroundHeight;
            ankleHightOflowerGround = m_leftAnkleHeight;
        }
        else
        {
            lowerGround = m_rightGroundHeight;
            ankleHightOflowerGround = m_rightAnkleHeight;
        }

        float rootY =  m_rootBone.position.y;
        if (lowerGround <= m_rootBone.position.y)
            rootY = Mathf.Lerp(m_rootBone.position.y, lowerGround, Mathf.InverseLerp(m_footTooHighAboveGroundToMatterThreshhold.y * 2, m_footTooHighAboveGroundToMatterThreshhold.x, ankleHightOflowerGround));

        rootY = Mathf.Lerp(m_lastRootY, rootY, Time.deltaTime * m_rootHeightAdjustSpeed);
        m_rootBone.position = new Vector3(m_rootBone.position.x, rootY, m_rootBone.position.z);

        m_lastRootY = m_rootBone.position.y;
    }






    private void CalculateDesiredFootPosAndRotationOnGround(ref RaycastHit hitL, ref RaycastHit hitR, ref bool hasGroundL, ref bool hasGroundR)
    {
        float leftY = Mathf.Lerp(m_lastLeftFootY, m_leftGroundHeight + m_leftAnkleHeight, Time.deltaTime * m_footHeightAdjustSpeed);
        float rightY = Mathf.Lerp(m_lastRightFootY, m_rightGroundHeight + m_rightAnkleHeight, Time.deltaTime * m_footHeightAdjustSpeed);

        m_desiredLeftFootPos = new Vector3(m_footBoneLeft.position.x, leftY, m_footBoneLeft.position.z);
        m_desiredRightFootPos = new Vector3(m_footBoneRight.position.x, rightY, m_footBoneRight.position.z);

        m_lastLeftFootY = leftY;
        m_lastRightFootY = rightY;




        if (!m_applyFootRot) return; ////IDEA: maybe only when moving

        Quaternion leftFootRotOfAnim = Quaternion.LookRotation(-new Vector3(m_footBoneLeft.forward.x, 0, m_footBoneLeft.forward.z), Vector3.up) * m_initialFootRot;
        Quaternion rightFootRotOfAnim = Quaternion.LookRotation(-new Vector3(m_footBoneRight.forward.x, 0, m_footBoneRight.forward.z), Vector3.up) * m_initialFootRot;
        Quaternion desiredGroundRotL = Quaternion.FromToRotation(Vector3.up, hasGroundL ?  hitL.normal : Vector3.up) * leftFootRotOfAnim;
        Quaternion desiredGroundRotR = Quaternion.FromToRotation(Vector3.up, hasGroundR ? hitR.normal : Vector3.up) * rightFootRotOfAnim;

        m_leftFootIsGroundedWeight =  Mathf.Max(Mathf.InverseLerp(m_minFootDistToOrigHight.x, m_minFootDistToOrigHight.y, (m_leftAnkleHeight  - m_baseOffsetGroundToAnkleY)),       Mathf.InverseLerp(m_minFootAngleToOrigAngle.x, m_minFootAngleToOrigAngle.y, Vector3.Angle(Vector3.up, m_leftFootUpHelper.up)));
        m_rightFootIsGroundedWeight = Mathf.Max(Mathf.InverseLerp(m_minFootDistToOrigHight.x, m_minFootDistToOrigHight.y, (m_rightAnkleHeight - m_baseOffsetGroundToAnkleY)),       Mathf.InverseLerp(m_minFootAngleToOrigAngle.x, m_minFootAngleToOrigAngle.y, Vector3.Angle(Vector3.up, m_rightFootUpHelper.up)));

        m_desiredLeftFootRot = Quaternion.Slerp(desiredGroundRotL, m_footBoneLeft.rotation , m_leftFootIsGroundedWeight);
        m_desiredRightFootRot = Quaternion.Slerp(desiredGroundRotR, m_footBoneRight.rotation , m_rightFootIsGroundedWeight);

        m_desiredLeftFootRot = Quaternion.Slerp(m_lastLeftFootRot, m_desiredLeftFootRot, m_footRotationAdjustSpeed * Time.deltaTime);
        m_desiredRightFootRot = Quaternion.Slerp(m_lastRightFootRot, m_desiredRightFootRot, m_footRotationAdjustSpeed * Time.deltaTime);

        //SetFootRotation
        m_footBoneLeft.rotation = m_desiredLeftFootRot;
        m_footBoneRight.rotation = m_desiredRightFootRot;

        m_lastLeftFootRot = m_desiredLeftFootRot;
        m_lastRightFootRot = m_desiredRightFootRot;

    }






    private void CalculateAndSetThightAndShinRotations()
    {
        if (!m_applyInverserseKinematics)
        {
            m_footBoneLeft.position = m_desiredLeftFootPos;
            m_footBoneRight.position = m_desiredRightFootPos;
            return;
        }

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


        float CalculateAngle(float boneLenght, float otherBoneLenght, float hipFootDist)
        {
            float semiPerimeter = (boneLenght + otherBoneLenght + hipFootDist) / 2;
            if (boneLenght + otherBoneLenght <= hipFootDist) { /*Debug.Log(boneLenght + otherBoneLenght); Debug.Log(hipFootDist);*/ return 0.01f; }

            float area = Mathf.Sqrt(semiPerimeter * (semiPerimeter - boneLenght) * (semiPerimeter - otherBoneLenght) * (semiPerimeter - hipFootDist));
            float triangleHeight = area * 2 * (1 / hipFootDist);

            return Mathf.Asin(triangleHeight / boneLenght) * Mathf.Rad2Deg;

        }
    }






}
