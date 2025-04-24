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
    private InputAction WestActionRef;
    private InputAction NorthActionRef;

    private InputAction DownActionRef;
    private InputAction RightActionRef;
    private InputAction LeftActionRef;
    private InputAction UpActionRef;

    private InputAction Options1ActionRef;
    private InputAction Options2ActionRef;


    public Vector2 RightStick { get { return m_rightStick; } }
    public Vector2 LeftStick { get { return m_leftStick; } }


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
        WestActionRef.performed         += OnWest;
        NorthActionRef.performed        += OnNorth;
        DownActionRef.performed         += OnDown;
        RightActionRef.performed        += OnRight;
        LeftActionRef.performed         += OnLeft;
        UpActionRef.performed           += OnUp;
        Options1ActionRef.performed     += OnOption1;
        Options2ActionRef.performed     += OnOption2;

        if(m_thePlayerCameraHolder != null && m_thePlayerMovement != null) 
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

        Vector2 input = m_leftStick;

        //m_input = m_input.normalized * UtilityFunctions.RefitRange(m_input.magnitude, 0.08f, 1, 0, 1); //maybe better in input script, bc in the 0.08, the camera still reacts
        input = new Vector2(UtilityFunctions.RefitRange(Mathf.Abs(input.x), 0.1f * input.magnitude, 1, 0, 1) * Mathf.Sign(input.x), input.y);

        m_thePlayerMovement.MoveStrenght = input.magnitude;

        m_thePlayerMovement.InputDirection = new Vector3(input.x, 0, input.y);

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
