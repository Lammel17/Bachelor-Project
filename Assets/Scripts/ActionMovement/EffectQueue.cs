using System.Transactions;
using System;

public class EffectQueue
{
    public Action effect;
    public float relativeEffectTime;
    public bool wasApplied = false;

    public EffectQueue(Action e, float t)
    {
        effect = e;
        relativeEffectTime = t;
    }
}
