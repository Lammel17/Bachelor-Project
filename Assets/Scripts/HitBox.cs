using UnityEngine;
using System;
using System.Collections.Generic;
using EditorAttributes;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{
    [SerializeField] [Required] private HitBoxManager m_hitBoxManager;

    public HitBoxManager OwnHitBoxManager { get => m_hitBoxManager; set => m_hitBoxManager = value; }

    public DamageData HurtBoxWasHit(HurtBoxManager hurtBoxManager)
    {
        return m_hitBoxManager.HurtBoxWasHit(hurtBoxManager);
    }

    
}
