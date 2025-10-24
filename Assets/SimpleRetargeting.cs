using NUnit.Framework;
using UnityEngine;

public class SimpleRetargeting : MonoBehaviour
{
    [SerializeField] private RetargetPair[] m_retargetPairList;
    [SerializeField] private Vector3 m_offset;

    [System.Serializable]
    private class RetargetPair
    {
        [SerializeField] public Transform m_original;
        [SerializeField] public Transform m_retarget;

        private RetargetPair(Transform orig, Transform retar)
        {
            m_original = orig;
            m_retarget = retar;
        }
    }




    public void DoTheRetargeting()
    {

        foreach (RetargetPair pair in m_retargetPairList)
        {
            if (pair.m_retarget != null && pair.m_original != null)
            {
                pair.m_retarget.position = pair.m_original.position + m_offset;
                pair.m_retarget.rotation = pair.m_original.rotation;
            }

        }
    }
}
