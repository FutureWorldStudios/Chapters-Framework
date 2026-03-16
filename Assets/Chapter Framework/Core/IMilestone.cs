using System;
using UnityEngine;

public interface IMilestone 
{
    public void Begin();
    public void Cancel();
    public void Complete();
}
