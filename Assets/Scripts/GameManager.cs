using System.ComponentModel;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject m_inputManagerObject;

    private static PlayerInputManager m_inputManager;
    public static PlayerInputManager InputManager { get => m_inputManager; }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (InputManager == null)
        {
            GameObject impMan = Instantiate(m_inputManagerObject);
            m_inputManager = impMan.GetComponent<PlayerInputManager>();
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
