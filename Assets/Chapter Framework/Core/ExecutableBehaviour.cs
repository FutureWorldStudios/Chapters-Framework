using UnityEngine;

namespace VRG.ChapterFramework.Core
{

    public abstract class ExecutableBehaviour : MonoBehaviour, IExecutable
    {
        public virtual void Begin()
        {
            Debug.Log($"[exec order] Begin called on {gameObject.name}");
        }

        public virtual void Complete()
        {
            Debug.Log($"[exec order] Complete called on {gameObject.name}");
        }

        public virtual void ForceCompletion()
        {
             Debug.Log($"[exec order] ForceCompletion called on {gameObject.name}");
        }

        public virtual void ForceReset()
        {
            Debug.Log($"[exec order] ForceReset called on {gameObject.name}");
        }
    }
}