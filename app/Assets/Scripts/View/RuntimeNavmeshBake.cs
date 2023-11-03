using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class RuntimeNavmeshBake : MonoBehaviour
{
    void Start()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
