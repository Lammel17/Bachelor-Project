using UnityEngine;
using UnityEngine.UIElements;

public class simpleHitBoxManager : MonoBehaviour
{

    [SerializeField] private HitBox m_hitBox;
    private DamageData m_damageData = new DamageData(0,0,0, new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), 0, 0);

    [SerializeField] private int m_physicalSliceDamage = 0;
    [SerializeField] private Vector2Int m_thermicDamage = new Vector2Int(0,0);
    [SerializeField] private int m_contaminationBuildUp = 0;
    [SerializeField] private int m_poiseDamage = 0;

    [SerializeField] private bool m_refresh = false;

    void Start()
    {
        m_damageData = new DamageData(m_physicalSliceDamage, 0, 0, m_thermicDamage, new Vector2Int(0, 0), new Vector2Int(0, 0), m_contaminationBuildUp, m_poiseDamage);

        Repeat(true);
    }

    private void Update()
    {
        if (m_refresh)
        {
            m_damageData = new DamageData(m_physicalSliceDamage, 0, 0, m_thermicDamage, new Vector2Int(0, 0), new Vector2Int(0, 0), m_contaminationBuildUp, m_poiseDamage);
            m_refresh = false;
        }
    }

    private void Repeat( bool activate)
    {
        if (m_hitBox != null)
        {
            if (activate) m_hitBox.ActivateHitBox(m_damageData);
            else m_hitBox.DeactivateHitBox();
        }

        StartCoroutine(UtilityFunctions.Wait(2, () => { Repeat(!activate); }));
    }
}
