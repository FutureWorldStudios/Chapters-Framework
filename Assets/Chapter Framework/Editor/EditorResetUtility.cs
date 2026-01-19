using UnityEditor;
using UnityEngine;

public static class EditorResetUtility
{
    public static T ResetComponent<T>(GameObject go) where T : Component
    {
        if (go == null)
        {
            Debug.LogError("ResetComponent<T>: GameObject is null.");
            return null;
        }

        T existing = go.GetComponent<T>();
        if (existing == null)
        {
            Debug.LogWarning($"ResetComponent<{typeof(T).Name}>: Component not found on '{go.name}'. Adding new one.");
            Undo.RegisterCompleteObjectUndo(go, $"Add {typeof(T).Name}");
            return Undo.AddComponent<T>(go);
        }

        Undo.RegisterCompleteObjectUndo(go, $"Reset {typeof(T).Name}");
        Object.DestroyImmediate(existing);

        T added = Undo.AddComponent<T>(go);

        EditorUtility.SetDirty(go);
        return added;
    }
}
