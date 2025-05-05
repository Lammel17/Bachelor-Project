using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;
    private PlayerMovement m_thePlayerMovement = null;
    private PlayerCameraHolder m_thePlayerCameraHolder = null;

    [SerializeField] private InputActionAsset inputActions;
    //private Action<InputAction.CallbackContext> PlayerInputAction = null; //??
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
    private InputAction EastHoldActionRef;
    private InputAction WestActionRef;
    private InputAction NorthActionRef;

    private InputAction DownActionRef;
    private InputAction RightActionRef;
    private InputAction LeftActionRef;
    private InputAction UpActionRef;

    private InputAction Options1ActionRef;
    private InputAction Options2ActionRef;


    public Vector2 RightStick { get => m_rightStick;  }
    public Vector2 LeftStick { get => m_leftStick;  }
    public float LeftStickSnappedMag { get => Snapping.Snap(m_leftStick.magnitude + 0.2f, 0.5f); }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

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
        EastHoldActionRef           = playerActionMap.FindAction("EastHold");
        WestActionRef           = playerActionMap.FindAction("West");
        NorthActionRef          = playerActionMap.FindAction("North");
        DownActionRef           = playerActionMap.FindAction("Down");
        RightActionRef          = playerActionMap.FindAction("Right");
        LeftActionRef           = playerActionMap.FindAction("Left");
        UpActionRef             = playerActionMap.FindAction("Up");
        Options1ActionRef       = playerActionMap.FindAction("Options1");
        Options2ActionRef       = playerActionMap.FindAction("Options2");


        ClearBufferAction = () => { m_lastInputIsUnread = false; c_inputBufferCoroutine = null; Debug.Log($"EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE  is {m_lastBuffedInput.action.name}"); };
       

    }

    
    public void SetPlayerAndCamera(PlayerMovement player, PlayerCameraHolder camera)
    {
        m_thePlayerMovement = player;
        m_thePlayerCameraHolder = camera;

        EnableOrDisableInputs(true);
    }

    private void EnableOrDisableInputs(bool enable)
    {
        if (enable)
        {
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
        else
        {
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
            NorthActionRef.Disable();
            DownActionRef.Disable();
            RightActionRef.Disable();
            LeftActionRef.Disable();
            UpActionRef.Disable();
            Options1ActionRef.Disable();
            Options2ActionRef.Disable();
        }

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
        EastHoldActionRef.performed       += OnEastHold;
        EastHoldActionRef.canceled        += OnEastHold;
        WestActionRef.performed         += OnWest;
        NorthActionRef.performed        += OnNorth;
        DownActionRef.performed         += OnDown;
        RightActionRef.performed        += OnRight;
        LeftActionRef.performed         += OnLeft;
        UpActionRef.performed           += OnUp;
        Options1ActionRef.performed     += OnOption1;
        Options2ActionRef.performed     += OnOption2;


        if (m_thePlayerCameraHolder != null && m_thePlayerMovement != null) 
            EnableOrDisableInputs(true);
        else
            EnableOrDisableInputs(false);

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
        EastHoldActionRef.performed        -= OnEastHold;
        EastHoldActionRef.canceled         -= OnEastHold;
        WestActionRef.performed         -= OnWest;
        NorthActionRef.performed        -= OnNorth;
        DownActionRef.performed         -= OnDown;
        RightActionRef.performed        -= OnRight;
        LeftActionRef.performed         -= OnLeft;
        UpActionRef.performed           -= OnUp;
        Options1ActionRef.performed     -= OnOption1;
        Options2ActionRef.performed     -= OnOption2;


        EnableOrDisableInputs(false);

    }




    public void RecallLatestBufferedInput()
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

    private bool SetBuffer(InputAction.CallbackContext context/*, int priority*/)
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



    Vector2 m_lastExteremeInput = Vector2.zero;
    Vector2 m_lastInput = Vector2.zero;
    Vector2 m_veryLastInput = Vector2.zero;
    float m_extremeInputMagnitude = 0;
    float m_lastInputMagnitude = 0;
    //Sticks
    private void OnLeftStick(InputAction.CallbackContext context)
    {
        float deadZone = 0.2f;

        m_leftStick = context.ReadValue<Vector2>();
        float inputMagnitude = m_leftStick.magnitude;

        SetLastExtremeInput();
        void SetLastExtremeInput()
        {
            //Wenn zuletzt unter deadzone war, dann ignoiren
            if (m_lastInputMagnitude < deadZone)
            {
                m_extremeInputMagnitude = 0;
                return;
            }

            //Wenn es inputMag kleiner als deadzone ist, dann wird es immer einletztes mal gesetzt gesetzt, dirakt auf 0
            if (inputMagnitude < deadZone) 
            {
                m_lastExteremeInput = m_leftStick;
                m_extremeInputMagnitude = 0;
                return;
            }
            // wenn stick fast 1 ist, dann wird es immer gesetzt gesetzt
            if (inputMagnitude >= 0.9f)
            {
                m_lastExteremeInput = m_leftStick;
                m_extremeInputMagnitude = inputMagnitude;
                return;
            }
            //wenn lastInput gleich lastExtremeInput ist, dann ignoiren
            if (m_lastInput == m_lastExteremeInput)
                return;


            //Wenn umschwung, dann ignoiren
            if ((m_leftStick.sqrMagnitude > m_lastInput.sqrMagnitude && m_lastInput.sqrMagnitude < m_veryLastInput.sqrMagnitude) || (m_leftStick.sqrMagnitude < m_lastInput.sqrMagnitude && m_lastInput.sqrMagnitude > m_lastExteremeInput.sqrMagnitude))
            {
                return;
            }
            //beim verlassen des Center muss man schneller werden
            if (((m_leftStick - m_lastInput).sqrMagnitude > (m_lastInput - m_veryLastInput).sqrMagnitude) && (m_leftStick.sqrMagnitude > m_lastInput.sqrMagnitude && m_lastInput.sqrMagnitude > m_lastExteremeInput.sqrMagnitude))
            {
                m_lastExteremeInput = m_leftStick;
                m_extremeInputMagnitude = inputMagnitude;
                return;
            }
            //beim nähern des Center muss man langsamer werden
            if (((m_leftStick - m_lastInput).sqrMagnitude < (m_lastInput - m_veryLastInput).sqrMagnitude) && (m_leftStick.sqrMagnitude < m_lastInput.sqrMagnitude && m_lastInput.sqrMagnitude < m_lastExteremeInput.sqrMagnitude))
            {
                m_lastExteremeInput = m_leftStick;
                m_extremeInputMagnitude = inputMagnitude;
                return;
            }

            return;

        }


        //this makes it easier to walk in a straight line, because the x value of 0 stays 0 when the x is under 0.1, but only if the magnitude is 1, thsi is Lerped
        Vector2 input = new Vector2(    Mathf.InverseLerp(0.1f * m_extremeInputMagnitude,    1,     Mathf.Abs(m_lastExteremeInput.x)) * Mathf.Sign(m_lastExteremeInput.x), m_lastExteremeInput.y);

        //Still! Stick value bounces when letting it go, thats sucks, problem for later?
        float magnitude = Snapping.Snap(Mathf.InverseLerp(0.2f, 1, m_extremeInputMagnitude) + 0.1f, 0.5f);
        if (magnitude != m_thePlayerMovement.MoveStrenght)
            m_thePlayerMovement.MoveStrenght = magnitude; //only gets set, when it differns from current magnitude
        if (magnitude > 0)
            m_thePlayerMovement.InputDirection = new Vector3(input.x, 0, input.y);

        m_veryLastInput = m_lastInput;
        m_lastInput = m_leftStick;
        m_lastInputMagnitude = inputMagnitude;

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
        if (context.performed)
            m_thePlayerCameraHolder.IsLockOn = !m_thePlayerCameraHolder.IsLockOn;
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

        if (!context.performed)
            return;
        Debug.Log($"ON EAST");
        m_thePlayerMovement.TriggerEvading();
    }

    private void OnEastHold(InputAction.CallbackContext context)
    {
        if (SetBuffer(context))
            return;

        if (context.performed)
        {
            //Debug.Log($"AAAAAAAAAAAAAAAAAAAAA East perf hold down");
            m_thePlayerMovement.IsRunning = true;
        }
        if (context.canceled)
        {
            //Debug.Log($"AAAAAAAAAAAAAAAAAAAAA East hold up");
            m_thePlayerMovement.IsRunning = false;
            //Beware, this canceled gets called even if hold was not performed
        }
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
