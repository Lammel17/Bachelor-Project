using System.ComponentModel;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject m_playerInputManagerObject;

    [SerializeField] private CharacterActionAndMovementHandler m_characterActionAndMovement;
    [SerializeField] private PlayerCameraHolder m_playerCameraHolder;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //init InputManager
        if (PlayerInputManager.Instance == null)
        {
            GameObject inputManObj = Instantiate(m_playerInputManagerObject);
            PlayerInputManager inputMan = inputManObj.GetComponent<PlayerInputManager>();
            inputMan.SetPlayerAndCamera(m_characterActionAndMovement, m_playerCameraHolder);
        }


    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
