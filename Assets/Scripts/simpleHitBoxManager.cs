using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(HitAndHurtBoxManagerOfEquipment))]
public class simpleHitBoxManager : MonoBehaviour
{

    [SerializeField] private HitAndHurtBoxManagerOfEquipment m_hitBoxManager;
    [SerializeField] private int m_hitBoxCollectionRef = 0;
    private DamageData m_damageData = new DamageData(0,0,0, new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), 0, 0, Vector3.zero);
    [Space]
    [Space]
    [SerializeField] private int m_physicalSliceDamage = 0;
    [SerializeField] private int m_physicalBluntDamage = 0;
    [SerializeField] private int m_physicalPierceDamage = 0;
    [SerializeField] private Vector2Int m_thermicDamage = new Vector2Int(0,0);
    [SerializeField] private Vector2Int m_electricDamage = new Vector2Int(0,0);
    [SerializeField] private Vector2Int m_metaphysicDamage = new Vector2Int(0,0);
    [SerializeField] private int m_contaminationBuildUp = 0;
    [SerializeField] private int m_poiseDamage = 0;
    [Space]
    [Space]
    [SerializeField] private float m_timeIntervall = 2;
    [SerializeField] private bool m_refreshDamageData = false;
    [Space]
    [SerializeField] private MeshRenderer m_objMeshRenderer;
    [SerializeField] private Material m_deactiveMaterial;
    [SerializeField] private Material m_activeMaterial;

    void Start()
    {
        m_damageData = new DamageData(m_physicalSliceDamage, m_physicalBluntDamage, m_physicalPierceDamage, m_thermicDamage, m_electricDamage, m_metaphysicDamage, m_contaminationBuildUp, m_poiseDamage, transform.forward);

        Repeat(true);
    }

    private void Update()
    {
        if (m_refreshDamageData)
        {
            m_damageData = new DamageData(m_physicalSliceDamage, 0, 0, m_thermicDamage, new Vector2Int(0, 0), new Vector2Int(0, 0), m_contaminationBuildUp, m_poiseDamage, transform.forward);
            m_refreshDamageData = false;
        }
    }

    private void Repeat( bool activate)
    {
        if (m_hitBoxManager != null)
        {
            if (activate) 
            { 
                m_hitBoxManager.ActivateHitboxCollection(m_hitBoxCollectionRef, m_damageData); 
                if (m_objMeshRenderer != null && m_activeMaterial != null && m_deactiveMaterial != null)
                    m_objMeshRenderer.material = m_activeMaterial;
            }
            else
            {
                m_hitBoxManager.DeactivateAllHitboxCollections();
                if (m_objMeshRenderer != null && m_activeMaterial != null && m_deactiveMaterial != null)
                    m_objMeshRenderer.material = m_deactiveMaterial;
            }
        }

        StartCoroutine(UtilityFunctions.Wait(m_timeIntervall, () => { Repeat(!activate); }));
    }
}
