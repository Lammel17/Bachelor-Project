using UnityEngine;
using System;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{

    [SerializeField] private InputActionAsset inputActions;
    private Action<InputAction.CallbackContext> PlayerInputAction = null;
    private Action ClearBufferAction = null;

    private Vector2 m_leftStick = new Vector2();
    private Vector2 m_rightStick = new Vector2();

    [SerializeField] private float m_inputBufferTime = 0.8f;

    private InputAction.CallbackContext m_lastBuffedInput = new();
    private Coroutine c_inputBufferCoroutine;
    private bool m_lastInputIsUnread = false;


    [Header("Input Action References")]
    private InputAction LeftStickActionRef;
    private InputAction RightStickActionRef;

    private InputAction R3ActionRef;
    private InputAction L3ActionRef;

    private InputAction R1ActionRef;
    private InputAction L1ActionRef;

    private InputAction R2ActionRef;
    private InputAction L2ActionRef;

    private InputAction SouthActionRef;
    private InputAction EastActionRef;
    private InputAction WestActionRef;
    private InputAction NorthActionRef;

    private InputAction DownActionRef;
    private InputAction RightActionRef;
    private InputAction LeftActionRef;
    private InputAction UpActionRef;

    private InputAction Options1ActionRef;
    private InputAction Options2ActionRef;


    public Vector2 RightStick { get { return m_rightStick; } }


    private void Awake()
    {
        // Hole die Action Map 'Player' und die spezifischen Actions
        var playerActionMap = inputActions.FindActionMap("MainPlayer");

        LeftStickActionRef      = playerActionMap.FindAction("LeftStick");
        RightStickActionRef     = playerActionMap.FindAction("RightStick");
        L3ActionRef             = playerActionMap.FindAction("L3");
        R3ActionRef             = playerActionMap.FindAction("R3");
        L1ActionRef             = playerActionMap.FindAction("L1");
        R1ActionRef             = playerActionMap.FindAction("R1");
        L2ActionRef             = playerActionMap.FindAction("L2");
        R2ActionRef             = playerActionMap.FindAction("R2");
        SouthActionRef          = playerActionMap.FindAction("South");
        EastActionRef           = playerActionMap.FindAction("East");
        WestActionRef           = playerActionMap.FindAction("West");
        NorthActionRef          = playerActionMap.FindAction("North");
        DownActionRef           = playerActionMap.FindAction("Down");
        RightActionRef          = playerActionMap.FindAction("Right");
        LeftActionRef           = playerActionMap.FindAction("Left");
        UpActionRef             = playerActionMap.FindAction("Up");
        Options1ActionRef        = playerActionMap.FindAction("Options1");
        Options2ActionRef        = playerActionMap.FindAction("Options2");


        ClearBufferAction = () => { m_lastInputIsUnread = false; c_inputBufferCoroutine = null; Debug.Log($"EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE  is {m_lastBuffedInput.action.name}"); };

    }


    private void OnEnable()
    {

        LeftStickActionRef.performed    += OnLeftStick;
        LeftStickActionRef.canceled     += OnLeftStick;
        RightStickActionRef.performed   += OnRightStick;
        RightStickActionRef.canceled    += OnRightStick;

        L3ActionRef.performed           += OnL3;
        R3ActionRef.performed           += OnR3;
        L1ActionRef.performed           += OnL1;
        R1ActionRef.performed           += OnR1;
        L2ActionRef.performed           += OnL2;
        R2ActionRef.performed           += OnR2;
        SouthActionRef.performed        += OnSouth;
        EastActionRef.performed         += OnEast;
        WestActionRef.performed         += OnWest;
        NorthActionRef.performed        += OnNorth;
        DownActionRef.performed         += OnDown;
        RightActionRef.performed        += OnRight;
        LeftActionRef.performed         += OnLeft;
        UpActionRef.performed           += OnUp;
        Options1ActionRef.performed     += OnOption1;
        Options2ActionRef.performed     += OnOption2;


        LeftStickActionRef.Enable();
        RightStickActionRef.Enable();
        L3ActionRef.Enable();
        R3ActionRef.Enable();
        L1ActionRef.Enable();
        R1ActionRef.Enable();
        L2ActionRef.Enable();
        R2ActionRef.Enable();
        SouthActionRef.Enable();
        EastActionRef.Enable();
        WestActionRef.Enable();
        NorthActionRef.Enable();
        DownActionRef.Enable();
        RightActionRef.Enable();
        LeftActionRef.Enable();
        UpActionRef.Enable();
        Options1ActionRef.Enable();
        Options2ActionRef.Enable();


    }

    private void OnDisable()
    {
        LeftStickActionRef.performed    -= OnLeftStick;
        RightStickActionRef.performed   -= OnRightStick;
        L3ActionRef.performed           -= OnL3;
        R3ActionRef.performed           -= OnR3;
        L1ActionRef.performed           -= OnL1;
        R1ActionRef.performed           -= OnR1;
        L2ActionRef.performed           -= OnL2;
        R2ActionRef.performed           -= OnR2;
        SouthActionRef.performed        -= OnSouth;
        EastActionRef.performed         -= OnEast;
        WestActionRef.performed         -= OnWest;
        NorthActionRef.performed        -= OnNorth;
        DownActionRef.performed         -= OnDown;
        RightActionRef.performed        -= OnRight;
        LeftActionRef.performed         -= OnLeft;
        UpActionRef.performed           -= OnUp;
        Options1ActionRef.performed     -= OnOption1;
        Options2ActionRef.performed     -= OnOption2;


        LeftStickActionRef.Disable();
        RightStickActionRef.Disable();
        L3ActionRef.Disable();
        R3ActionRef.Disable();
        L1ActionRef.Disable();
        R1ActionRef.Disable();
        L2ActionRef.Disable();
        R2ActionRef.Disable();
        SouthActionRef.Disable();
        EastActionRef.Disable();
        WestActionRef.Disable();
        NorthActionRef. Disable();
        DownActionRef.Disable();
        RightActionRef. Disable();
        LeftActionRef.Disable();
        UpActionRef.Disable();
        Options1ActionRef.Disable();
        Options2ActionRef.Disable();
    }



    public void RecallLatestInput()
    {
        if (!m_lastInputIsUnread)
            return;

        switch (m_lastBuffedInput.action.name)
        {
            case "L3":
                OnL3(m_lastBuffedInput);
                break;
            case "R1":
                OnR1(m_lastBuffedInput);
                break;
            case "L2":
                OnL2(m_lastBuffedInput);
                break;
            case "R2":
                OnR2(m_lastBuffedInput);
                break;
            case "South":
                OnSouth(m_lastBuffedInput);
                break;
            case "East":
                OnEast(m_lastBuffedInput);
                break;
            case "West":
                OnWest(m_lastBuffedInput);
                break;
            case "Right":
                OnRight(m_lastBuffedInput);
                break;
            case "Left":
                OnLeft(m_lastBuffedInput);
                break;
            default:
                Debug.Log("Last Input Check must be wrong?");
                break;
        }
    }

    private bool SetBuffer(InputAction.CallbackContext context)
    {
        if (1 != 1) ///////////check if Animation is currently not interuptable
        {
            m_lastInputIsUnread = true;
            m_lastBuffedInput = context;

            //the last input only stays readable for an amount of time;
            c_inputBufferCoroutine = StartCoroutine(UtilityFunctions.Wait(m_inputBufferTime, ClearBufferAction));

            return true;
        }
        else // check with something with priority like dodge here
        {
            m_lastInputIsUnread = false;            //not sure if needed
            if (c_inputBufferCoroutine != null)
            {
                StopCoroutine(c_inputBufferCoroutine);
                c_inputBufferCoroutine = null;
            }
        }
        
        return false;
    }


    //Sticks
    private void OnLeftStick(InputAction.CallbackContext context)
    {
        m_leftStick = context.ReadValue<Vector2>();
        //Debug.Log($"AAAAAAAAAAAAAAAAAAAAA {m_leftStick}");
    }

    private void OnRightStick(InputAction.CallbackContext context)
    {
        m_rightStick = context.ReadValue<Vector2>();
        //Debug.Log($"AAAAAAAAAAAAAAAAAAAAA {m_rightStick}");
    }


    //StickButtons
    private void OnL3(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if(context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA L3");
    }

    private void OnR3(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA R3");
    }


    //ShoulderButtons
    private void OnL1(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA L1");
    }

    private void OnR1(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA R1");
    }

    private void OnL2(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA L2");
    }

    private void OnR2(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA R2");
    }

    
    
    //ActionButtons
    private void OnSouth(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA South");
    }

    private void OnEast(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA East");
    }

    private void OnWest(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA West");
    }

    private void OnNorth(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA North");
    }


    //DPad
    private void OnDown(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA Down");
    }

    private void OnRight(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA Right");
    }

    private void OnLeft(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA Left");
    }

    private void OnUp(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA Up");
    }

    
    //Options
    private void OnOption1(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA Option1");
    }

    private void OnOption2(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA Option2");
    }



}

    /*

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        m_Instance = this;

        if (m_playerInput == null)
            m_playerInput = gameObject.GetComponent<PlayerInput>();

        if (m_Instance != null)
        {
            PlayerInputAction += (InputAction.CallbackContext context) => OnInputActionTriggered(context);
            m_playerInput.onActionTriggered += PlayerInputAction;
        }

        ClearBufferAction = () => { m_lastInputIsUnread = false; c_inputBufferCoroutine = null; Debug.Log($"EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE  is {m_lastBuffedInput.action.name}"); };

    }

    private void OnDestroy()
    {
        if (m_playerInput != null && PlayerInputAction != null)
            m_playerInput.onActionTriggered -= PlayerInputAction;
    }

    private void OnInputActionTriggered(InputAction.CallbackContext context)
    {
        //if(context.action.name != m_validInputs)
        if (context.control.device is Gamepad)
            HandleInput(context);
    }

    public void RecallLatestInput()
    {
        if (!m_lastInputIsUnread)
            return;

        HandleInput(m_lastBuffedInput);
    }


    private void HandleInput(InputAction.CallbackContext context)
    {

        //non buffer Inputs
        switch (context.action.name)
        {
            case "LeftStick":
                m_leftStick = context.ReadValue<Vector2>();
                Debug.Log(m_leftStick);
                break;
            case "RightStick":
                m_rightStick = context.ReadValue<Vector2>();
                Debug.Log(m_rightStick);
                break;

            case "R3":
                Debug.Log($"R3 {context.started}");
                break;

            case "L1":
                Debug.Log($"L1 {context.started}");
                break;

            case "South":
                Debug.Log($"South {context.started}");
                break;
            case "North":
                Debug.Log($"North {context.started}");
                break;

            case "Option1":
                Debug.Log($"Opt 1 {context.started}");
                break;
            case "option2":
                Debug.Log($"Opt 2 {context.started}");
                break;



            //Buffer Inputs
            default:

                if (1 == 1) ///////////check later if animations are played which cant be interrupted
                {
                    m_lastInputIsUnread = true;
                    
                    m_lastBuffedInput = context;

                    //the last input only stays readable for an amount of time;
                    c_inputBufferCoroutine = StartCoroutine(UtilityFunctions.Wait(m_inputBufferTime, ClearBufferAction));
                    
                    break;
                }
                else
                {
                    m_lastInputIsUnread = false;
                    if (c_inputBufferCoroutine != null)
                    {
                        StopCoroutine(c_inputBufferCoroutine);
                        c_inputBufferCoroutine = null;

                    }
                }

                switch (context.action.name)
                {
                    case "L3":
                        Debug.Log($"L3 {context.started}");
                        break;


                    case "R1":
                        Debug.Log($"R1 {context.started}");
                        break;


                    case "R2":
                        Debug.Log(context.action.WasReleasedThisFrame() + " ee");
                        Debug.Log($"R2 {context.started}");
                        break;
                    case "L2":
                        Debug.Log($"L2 {context.started}");
                        break;


                    case "East":
                        Debug.Log($"East {context.started}");
                        break;
                    case "West":
                        Debug.Log($"West {context.started}");
                        break;


                    case "Down":
                        Debug.Log($"Down {context.started}");
                        break;
                    case "Right":
                        Debug.Log($"Right {context.started}");
                        break;
                    case "Left":
                        Debug.Log($"Left {context.started}");
                        break;
                    case "Up":
                        Debug.Log($"Up {context.started}");
                        break;

                    default:

                        break;
                }



                break;
        }

    }


}

    */

/*
namespace Inputs 
{
    public class InputManager : MonoBehaviour
    {
        //Sticks
        [NonSerialized] public static Vector2 leftStick = new Vector2();
        [NonSerialized] public static Vector2 rightStick = new Vector2();

        //StickButtons
        [NonSerialized] public static bool R3 = false;
        [NonSerialized] public static bool R3Up = false;
        [NonSerialized] public static bool R3Down = false;

        //ShoulderButtons
        [NonSerialized] public static bool R1 = false;
        [NonSerialized] public static bool R1Up = false;
        [NonSerialized] public static bool R1Down = false;

        [NonSerialized] public static float R2Prev = 0;
        [NonSerialized] public static bool R2 = false;
        [NonSerialized] public static bool R2Up = false;
        [NonSerialized] public static bool R2Down = false;

        [NonSerialized] public static bool L1 = false;
        [NonSerialized] public static bool L1Up = false;
        [NonSerialized] public static bool L1Down = false;

        [NonSerialized] public static float L2Prev = 0;
        [NonSerialized] public static bool L2 = false;
        [NonSerialized] public static bool L2Up = false;
        [NonSerialized] public static bool L2Down = false;

        //ActionButtons
        [NonSerialized] public static bool AorCross = false;
        [NonSerialized] public static bool AorCrossUp = false;
        [NonSerialized] public static bool AorCrossDown = false;

        [NonSerialized] public static bool BorCircle = false;
        [NonSerialized] public static bool BorCircleUp = false;
        [NonSerialized] public static bool BorCircleDown = false;

        [NonSerialized] public static bool XorSquare = false;
        [NonSerialized] public static bool XorSquareUp = false;
        [NonSerialized] public static bool XorSquareDown = false;

        [NonSerialized] public static bool YorTriangle = false;
        [NonSerialized] public static bool YorTriangleUp = false;
        [NonSerialized] public static bool YorTriangleDown = false;

        //DPad
        [NonSerialized] public static float DPadYPrev = 0;
        [NonSerialized] public static bool DPadUp = false;
        [NonSerialized] public static bool DPadUp_Up = false;
        [NonSerialized] public static bool DPadUp_Down = false;

        [NonSerialized] public static bool DPadDown = false;
        [NonSerialized] public static bool DPadDown_Up = false;
        [NonSerialized] public static bool DPadDown_Down = false;

        [NonSerialized] public static bool DPadRight = false;
        [NonSerialized] public static bool DPadRight_Up = false;
        [NonSerialized] public static bool DPadRight_Down = false;

        [NonSerialized] public static float DPadXPrev = 0;
        [NonSerialized] public static bool DPadLeft = false;
        [NonSerialized] public static bool DPadLeft_Up = false;
        [NonSerialized] public static bool DPadLeft_Down = false;





        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            leftStick = new Vector2(Input.GetAxis("LeftJoyStickX"), Input.GetAxis("LeftJoyStickY"));
            rightStick = new Vector2(Input.GetAxis("RightJoyStickX"), Input.GetAxis("RightJoyStickY"));

            //if (R1Down == true) R1Down = false;
            //if (R1Up == true) R1Up = false;
            if (Input.GetButtonDown("R1")) { R1 = true; R1Down = true; }
            if (Input.GetButtonUp("R1")) { R1 = false; R1Up = true; }

            //if (R2Down == true) R2Down = false;
            //if (R2Up == true) R2Up = false;
            if (Input.GetAxis("R2") == 1 && R2Prev != 1) { R2 = true; R2Down = true; }
            if (Input.GetAxis("R2") != 1 && R2Prev == 1) { R2 = false; R2Up = true; }
            if (R2Prev != Input.GetAxis("R2")) R2Prev = Input.GetAxis("R2");


            //if (R3Down == true) R3Down = false;
            //if (R3Up == true) R3Up = false;
            if (Input.GetButtonDown("R3")) { R3 = true; R3Down = true; }
            if (Input.GetButtonUp("R3")) { R3 = false; R3Up = true; }


            //if (L1Down == true) L1Down = false;
            //if (L1Up == true) L1Up = false;
            if (Input.GetButtonDown("L1")) { L1 = true; L1Down = true; }
            if (Input.GetButtonUp("L1")) { L1 = false; L1Up = true; }

            //if (L2Down == true) L2Down = false;
            //if (L2Up == true) L2Up = false;
            if (Input.GetAxis("L2") == 1 && L2Prev != 1) { L2 = true; L2Down = true; }
            if (Input.GetAxis("L2") != 1 && L2Prev == 1) { L2 = false; L2Up = true; }
            if (L2Prev != Input.GetAxis("L2")) L2Prev = Input.GetAxis("L2");




            //if (AorCrossDown == true) AorCrossDown = false;
            //if (AorCrossUp == true) AorCrossUp = false;
            if (Input.GetButtonDown("AorCross")) { AorCross = true; AorCrossDown = true; }
            if (Input.GetButtonUp("AorCross")) { AorCross = false; AorCrossUp = true; }

            //if (BorCircleDown == true) BorCircleDown = false;
            //if (BorCircleUp == true) BorCircleUp = false;
            if (Input.GetButtonDown("BorCircle")) { BorCircle = true; BorCircleDown = true; }
            if (Input.GetButtonUp("BorCircle")) { BorCircle = false; BorCircleUp = true; }

            //if (XorSquareDown == true) XorSquareDown = false;
            //if (XorSquareUp == true) XorSquareUp = false;
            if (Input.GetButtonDown("XorSquare")) { XorSquare = true; XorSquareDown = true; }
            if (Input.GetButtonUp("XorSquare")) { XorSquare = false; XorSquareUp = true; }

            //if (YorTriangleDown == true) YorTriangleDown = false;
            //if (YorTriangleUp == true) YorTriangleUp = false;
            if (Input.GetButtonDown("YorTriangle")) { YorTriangle = true; YorTriangleDown = true; }
            if (Input.GetButtonUp("YorTriangle")) { YorTriangle = false; YorTriangleUp = true; }



            if (Input.GetAxis("DPadY") == 1 && DPadYPrev != 1) { DPadUp = true; DPadUp_Down = true; }
            if (Input.GetAxis("DPadY") != 1 && DPadYPrev == 1) { DPadUp = false; DPadUp_Up = true; }

            if (Input.GetAxis("DPadY") == -1 && DPadYPrev != -1) { DPadDown = true; DPadDown_Down = true; }
            if (Input.GetAxis("DPadY") != -1 && DPadYPrev == -1) { DPadDown = false; DPadDown_Up = true; }

            if (Input.GetAxis("DPadX") == 1 && DPadXPrev != 1) { DPadRight = true; DPadRight_Down = true; }
            if (Input.GetAxis("DPadX") != 1 && DPadXPrev == 1) { DPadRight = false; DPadRight_Up = true; }

            if (Input.GetAxis("DPadX") == -1 && DPadXPrev != -1) { DPadLeft = true; DPadLeft_Down = true; }
            if (Input.GetAxis("DPadX") != -1 && DPadXPrev == -1) { DPadLeft = false; DPadLeft_Up = true; }

            if (DPadYPrev != Input.GetAxis("DPadY")) DPadYPrev = Input.GetAxis("DPadY");
            if (DPadXPrev != Input.GetAxis("DPadX")) DPadXPrev = Input.GetAxis("DPadX");

            //Debug.Log(DPadDown_Down);

        }




        public static void SetR1DownFalse() { R1Down = false; }
        public static void SetR1UpFalse() { R1Up = false; }

        public static void SetR2DownFalse() { R2Down = false; }
        public static void SetR2UpFalse() { R2Up = false; }

        public static void SetR3DownFalse() { R3Down = false; }
        public static void SetR3UpFalse() { R3Up = false; }

        public static void SetL1DownFalse() { L1Down = false; }
        public static void SetL1UpFalse() { L1Up = false; }

        public static void SetL2DownFalse() { L2Down = false; }
        public static void SetL2UpFalse() { L2Up = false; }

        public static void SetAorCrossDownFalse() { AorCrossDown = false; }
        public static void SetAorCrossUpFalse() { AorCrossUp = false; }

        public static void SetBorCircleDownFalse() { BorCircleDown = false; }
        public static void SetBorCircleUpFalse() { BorCircleUp = false; }

        public static void SetXorSquareDownFalse() { XorSquareDown = false; }
        public static void SetXorSquareUpFalse() { XorSquareUp = false; }

        public static void SetYorTriangleDownFalse() { YorTriangleDown = false; }
        public static void SetYorTriangleUpFalse() { YorTriangleUp = false; }


        public static void DPadUp_DownFalse() { DPadUp_Down = false; }
        public static void DPadUp_UpFalse() { DPadUp_Up = false; }

        public static void DPadDown_DownFalse() { DPadDown_Down = false; }
        public static void DPadDown_UpFalse() { DPadDown_Up = false; }

        public static void DPadRight_DownFalse() { DPadRight_Down = false; }
        public static void DPadRight_UpFalse() { DPadRight_Up = false; }

        public static void DPadLeft_DownFalse() { DPadLeft_Down = false; }
        public static void DPadLeft_UpFalse() { DPadLeft_Up = false; }





    }


}
*/