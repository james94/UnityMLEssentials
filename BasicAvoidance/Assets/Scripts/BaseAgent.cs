using System.Collections;
using Unity.MLAgents;
using UnityEngine;

public class BaseAgent : Agent
{
    [SerializeField] // so it knows how to change the color of the mesh
    protected MeshRenderer groundMeshRenderer;

    [SerializeField]
    protected Material successMaterial;

    [SerializeField]
    protected Material failureMaterial;

    [SerializeField]
    protected Material defaultMaterial;

    // method based on Unity's one
    protected IEnumerator SwapGroundMaterial(Material mat, float time)
    {
        groundMeshRenderer.material = mat; // change ground material, show red
        yield return new WaitForSeconds(time); // wait this amount of secs (about 1/2 sec)
        groundMeshRenderer.material = defaultMaterial; // brings material back to default
    }
}
