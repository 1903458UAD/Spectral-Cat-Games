using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class RemoveAllScripts : MonoBehaviour
{
    [MenuItem("Tools/Remove All Script Components")]
    private static void RemoveAllScriptComponents()
    {
        // Get all GameObjects in the scene, including inactive ones
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        int removedScriptCount = 0;

        // Define a list of built-in Unity component types we don't want to remove
        System.Type[] unityBuiltInTypes = new System.Type[]
        {
            typeof(Transform),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            typeof(MeshFilter),
            typeof(Collider),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider),
            typeof(Rigidbody),
            typeof(Camera),
            typeof(Light),
            typeof(Animator),
            typeof(AudioSource),
            typeof(Canvas),
            typeof(RectTransform),
            typeof(TextMesh),
            typeof(SpriteRenderer)
            // Add more Unity built-in types if needed
        };

        foreach (GameObject obj in allObjects)
        {
            // Skip prefabs and non-scene objects
            if (EditorUtility.IsPersistent(obj) || obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                continue;

            // Get all components
            UnityEngine.Component[] components = obj.GetComponents<UnityEngine.Component>();

            foreach (UnityEngine.Component comp in components)
            {
                if (comp == null)
                {
                    // Remove missing scripts
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                    removedScriptCount++;
                    continue;
                }

                System.Type compType = comp.GetType();

                // Remove if it's not a built-in Unity component
                bool isBuiltIn = false;
                foreach (var builtInType in unityBuiltInTypes)
                {
                    if (compType == builtInType || compType.IsSubclassOf(builtInType))
                    {
                        isBuiltIn = true;
                        break;
                    }
                }

                if (!isBuiltIn)
                {
                    DestroyImmediate(comp, true);
                   // Debug.Log($"Removed script/component {compType.Name} from {obj.name}", obj);
                    removedScriptCount++;
                }
            }
        }

        //Debug.Log($"Total script components removed: {removedScriptCount}");
    }
}
