using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using Unity.IO.LowLevel.Unsafe;


[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;
    private CharacterActionAndMovementHandler m_characterActionAndMovement = null;
    private PlayerCameraHolder m_thePlayerCameraHolder = null;

    [SerializeField] private InputActionAsset inputActions;
    //private Action<InputAction.CallbackContext> PlayerInputAction = null; //??
    private Action ClearBufferAction = null;

    private Vector2 m_leftStick = new Vector2();
    private Vector2 m_rightStick = new Vector2();
    private bool m_isShift = false;

    /*[SerializeField] */
    private float m_inputBufferTime = 0.5f;

    public class LastBufferedInputCombi
    {
        public InputAction.CallbackContext lastBuffedInput;
        public bool isNorth;

        public LastBufferedInputCombi(InputAction.CallbackContext lastBuffedInput, bool isNorth    )
        {
            this.lastBuffedInput = lastBuffedInput;
            this.isNorth = isNorth;
        }
    }
    private LastBufferedInputCombi m_lastBufferedInputCombi;

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
    private InputAction EastTapActionRef;
    private InputAction EastHoldActionRef;
    private InputAction WestActionRef;
    private InputAction NorthTapActionRef;
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
        EastTapActionRef           = playerActionMap.FindAction("EastTap");
        EastHoldActionRef           = playerActionMap.FindAction("EastHold");
        WestActionRef           = playerActionMap.FindAction("West");
        NorthTapActionRef          = playerActionMap.FindAction("NorthTap");
        NorthActionRef          = playerActionMap.FindAction("North");
        DownActionRef           = playerActionMap.FindAction("Down");
        RightActionRef          = playerActionMap.FindAction("Right");
        LeftActionRef           = playerActionMap.FindAction("Left");
        UpActionRef             = playerActionMap.FindAction("Up");
        Options1ActionRef       = playerActionMap.FindAction("Options1");
        Options2ActionRef       = playerActionMap.FindAction("Options2");


        ClearBufferAction = () => { m_lastInputIsUnread = false; c_inputBufferCoroutine = null; /*Debug.Log($"{m_lastBuffedInput.action.name} in buffer is getting cleared");*/ };
       

    }

    public void SetPlayerAndCamera(CharacterActionAndMovementHandler player, PlayerCameraHolder camera)
    {
        m_characterActionAndMovement = player;
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

            EastTapActionRef.Enable();
            EastHoldActionRef.Enable();

            WestActionRef.Enable();

            NorthTapActionRef.Enable();
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

            EastTapActionRef.Disable();
            EastHoldActionRef.Disable();

            WestActionRef.Disable();
               
            NorthTapActionRef.Disable();
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
        L1ActionRef.canceled            += OnL1;
        R1ActionRef.performed           += OnR1;

        L2ActionRef.performed           += OnL2;
        R2ActionRef.performed           += OnR2;

        SouthActionRef.performed        += OnSouth;

        EastTapActionRef.performed      += OnEastTap;
        EastHoldActionRef.performed     += OnEastHold;
        EastHoldActionRef.canceled      += OnEastHold;

        WestActionRef.performed         += OnWest;

        NorthTapActionRef.performed     += OnNorthTap;
        NorthActionRef.performed        += OnNorth;
        NorthActionRef.canceled         += OnNorth;

        DownActionRef.performed         += OnDown;
        RightActionRef.performed        += OnRight;
        LeftActionRef.performed         += OnLeft;
        UpActionRef.performed           += OnUp;

        Options1ActionRef.performed     += OnOption1;
        Options2ActionRef.performed     += OnOption2;


        if (m_thePlayerCameraHolder != null && m_characterActionAndMovement != null) 
            EnableOrDisableInputs(true);
        else
            EnableOrDisableInputs(false);

    }

    private void OnDisable()
    {
        LeftStickActionRef.performed    -= OnLeftStick;
        LeftStickActionRef.canceled     -= OnLeftStick;
        RightStickActionRef.performed   -= OnRightStick;
        RightStickActionRef.canceled    -= OnRightStick;

        L3ActionRef.performed           -= OnL3;
        R3ActionRef.performed           -= OnR3;

        L1ActionRef.performed           -= OnL1;
        L1ActionRef.canceled            -= OnL1;
        R1ActionRef.performed           -= OnR1;

        L2ActionRef.performed           -= OnL2;
        R2ActionRef.performed           -= OnR2;

        SouthActionRef.performed        -= OnSouth;

        EastTapActionRef.performed      -= OnEastTap;
        EastHoldActionRef.performed     -= OnEastHold;
        EastHoldActionRef.canceled      -= OnEastHold;

        WestActionRef.performed         -= OnWest;

        NorthTapActionRef.performed     -= OnNorthTap;
        NorthActionRef.performed        -= OnNorth;
        NorthActionRef.canceled         -= OnNorth;

        DownActionRef.performed         -= OnDown;
        RightActionRef.performed        -= OnRight;
        LeftActionRef.performed         -= OnLeft;
        UpActionRef.performed           -= OnUp;

        Options1ActionRef.performed     -= OnOption1;
        Options2ActionRef.performed     -= OnOption2;


        EnableOrDisableInputs(false);

    }


    public bool CheckRecallLatestBufferedInput()
    {
        return m_lastInputIsUnread;
    }

    public void RecallLatestBufferedInput()
    {
        if (!m_lastInputIsUnread)
            return;

        bool buffedIsShift = m_lastBufferedInputCombi.isNorth;

        switch (m_lastBufferedInputCombi.lastBuffedInput.action.name)
        {
            case "L3":
                OnL3(m_lastBufferedInputCombi.lastBuffedInput);
                break;
            case "R1":
                if (!buffedIsShift)     OnR1_only(m_lastBufferedInputCombi.lastBuffedInput, false);
                else                    OnR1_Shift(m_lastBufferedInputCombi.lastBuffedInput, true);
                break;
            case "L1":
                if (buffedIsShift)      OnL1_North(m_lastBufferedInputCombi.lastBuffedInput, true);
                break;
            case "R2":
                if (!buffedIsShift)     OnR2_only(m_lastBufferedInputCombi.lastBuffedInput, false);
                else                    OnR2_Shift(m_lastBufferedInputCombi.lastBuffedInput, true);
                break;
            case "L2":
                if (!buffedIsShift)     OnL2_only(m_lastBufferedInputCombi.lastBuffedInput, false);
                else                    OnL2_Shift(m_lastBufferedInputCombi.lastBuffedInput, true);
                break;
            case "South":
                OnSouth(m_lastBufferedInputCombi.lastBuffedInput);
                break;
            case "EastTap":
                OnEastTap(m_lastBufferedInputCombi.lastBuffedInput);
                break;
            case "West":
                OnWest(m_lastBufferedInputCombi.lastBuffedInput);
                break;
            case "Right":
                OnRight(m_lastBufferedInputCombi.lastBuffedInput);
                break;
            case "Left":
                OnLeft(m_lastBufferedInputCombi.lastBuffedInput);
                break;
            default:
                Debug.Log("Last Input Check must be wrong?");
                break;
        }
    }

    private bool SetBuffer(InputAction.CallbackContext context, bool isShift, int priority)
    {
        if (priority > (int)m_characterActionAndMovement.CurrentInteruptability) ///////////check if Animation is currently interuptable
        {
            m_lastInputIsUnread = false;            //not sure if needed
            if (c_inputBufferCoroutine != null)
            {
                StopCoroutine(c_inputBufferCoroutine);
                c_inputBufferCoroutine = null;
            }
            return false;
        }
        else // maybe check with something with priority like dodge here
        {
            m_lastInputIsUnread = true;
            m_lastBufferedInputCombi = new LastBufferedInputCombi(context, isShift);
            if (c_inputBufferCoroutine != null)
            {
                StopCoroutine(c_inputBufferCoroutine);
                c_inputBufferCoroutine = null;
            }
            //the last input only stays readable for an amount of time;
            c_inputBufferCoroutine = StartCoroutine(UtilityFunctions.Wait(m_inputBufferTime, ClearBufferAction));
            return true;
        }
        
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
        if (magnitude != m_characterActionAndMovement.InputStrenght)
            m_characterActionAndMovement.InputStrenght = magnitude; //only gets set, when it differns from current magnitude
        if (magnitude > 0)
            m_characterActionAndMovement.InputDirection = new Vector3(input.x, 0, input.y);

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
        int priority = 0;

        if (SetBuffer(context, m_isShift, priority))
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
        if (m_isShift && context.performed) OnL1_North(context, true);
        else if (!m_isShift)
        {
            if (context.performed) m_characterActionAndMovement.TriggerShielding(true);
            if (context.canceled)  m_characterActionAndMovement.TriggerShielding(false);
        }
    }
    private void OnL1_North(InputAction.CallbackContext context, bool isShift)
    {
        int priority = m_characterActionAndMovement.GetInterruptabilityShieldLightSpecial();
        if (SetBuffer(context, isShift, priority)) return;

        m_characterActionAndMovement.TriggerShieldSpecialLight();
    }



    private void OnR1(InputAction.CallbackContext context)
    {
        if (m_isShift)  OnR1_Shift(context, true);
        else            OnR1_only(context, false);
    }
    private void OnR1_only(InputAction.CallbackContext context, bool isShift)
    {
        int priority = m_characterActionAndMovement.GetInterruptabilityLight();
        if (SetBuffer(context, isShift, priority)) return;

        m_characterActionAndMovement.TriggerLightAttack();
    }
    private void OnR1_Shift(InputAction.CallbackContext context, bool isNorth)
    {
        int priority = m_characterActionAndMovement.GetInterruptabilityLightSpecial();
        if (SetBuffer(context, isNorth, priority)) return;

        m_characterActionAndMovement.TriggerSpecialLightAttack();
    }



    private void OnL2(InputAction.CallbackContext context)
    {
        if (m_isShift)  OnL2_Shift(context, true);
        else            OnL2_only(context, false);
    }
    private void OnL2_only(InputAction.CallbackContext context, bool isShift)
    {
        int priority = 2;
        if (SetBuffer(context, isShift, priority)) return;

        //Debug.Log($"AAAAAAAAAAAAAAAAAAAAA L2");
    }
    private void OnL2_Shift(InputAction.CallbackContext context, bool isShift)
    {
        int priority = m_characterActionAndMovement.GetInterruptabilityHeavySpecial(); 
        if (SetBuffer(context, isShift, priority)) return;

        m_characterActionAndMovement.TriggerShieldSpecialHeavy();

    }



    private void OnR2(InputAction.CallbackContext context)
    {
        if (m_isShift)  OnR2_Shift(context, true);
        else            OnR2_only(context, false);
    }
    private void OnR2_only(InputAction.CallbackContext context, bool isShift)
    {
        int priority = m_characterActionAndMovement.GetInterruptabilityHeavy();
        if (SetBuffer(context, isShift, priority))  return;

        m_characterActionAndMovement.TriggerHeavyAttack();
    }
    private void OnR2_Shift(InputAction.CallbackContext context, bool isShift)
    {
        int priority = m_characterActionAndMovement.GetInterruptabilityHeavy();
        if (SetBuffer(context, isShift, priority)) return;

        m_characterActionAndMovement.TriggerSpecialHeavyAttack();
    }


    //ActionButtons
    private void OnSouth(InputAction.CallbackContext context)
    {

        int priority = 0;

        if (SetBuffer(context, m_isShift, priority))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA South");
    }

    private void OnEastTap(InputAction.CallbackContext context)
    {
        int priority = 3;

        if (SetBuffer(context, m_isShift, priority))
            return;

        m_characterActionAndMovement.TriggerEvading();
    }

    private void OnEastHold(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            //Debug.Log($"AAAAAAAAAAAAAAAAAAAAA East perf hold down");
            m_characterActionAndMovement.IsHoldRunning = true;
        }
        if (context.canceled)
        {
            //Debug.Log($"AAAAAAAAAAAAAAAAAAAAA East hold up");
            m_characterActionAndMovement.IsHoldRunning = false;
            //Beware, this canceled gets called even if hold was not performed
        }
    }

    private void OnWest(InputAction.CallbackContext context)
    {
        int priority = 2;

        if (SetBuffer(context, m_isShift, priority))
            return;

        m_characterActionAndMovement.TriggerItemUse();


        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA West");
    }


    private void OnNorthTap(InputAction.CallbackContext context)
    {
            //Debug.Log($"AAAAAAAAAAAAAAAAAAAAA North TAP");

    }
    private void OnNorth(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            m_isShift = true; 
        }

        if (context.canceled)
        {
            m_isShift = false;
        }
    }


    //DPad
    private void OnDown(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA Down");
    }

    private void OnRight(InputAction.CallbackContext context)
    {
        int priority = 0;

        if (SetBuffer(context, m_isShift, priority))
            return;

        //if (context.performed)
        //    Debug.Log($"AAAAAAAAAAAAAAAAAAAAA Right");
    }

    private void OnLeft(InputAction.CallbackContext context)
    {
        int priority = 0;

        if (SetBuffer(context, m_isShift, priority))
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
