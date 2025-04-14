using System.ComponentModel;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject m_inputManagerObject;

    public static PlayerInputManager InputManager { get; private set; }


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if (InputManager == null)
        {
            GameObject impMan = Instantiate(m_inputManagerObject);
            InputManager = impMan.GetComponent<PlayerInputManager>();
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
