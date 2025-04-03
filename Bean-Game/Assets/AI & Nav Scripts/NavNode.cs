using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class NavNode : MonoBehaviour
{


    private void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
