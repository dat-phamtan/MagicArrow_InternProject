using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class SimpleProceduralMesh : MonoBehaviour
{
    public void OnEnable()
    {
        
    }

    void Start()
    {
        var mesh = new Mesh
        {
            name = "Procedural Mesh"
        };

        mesh.vertices = new Vector3[]
        {
            Vector3.one,
            Vector3.right,
            Vector3.up
        };
        Debug.Log(Vector3.right);

        mesh.triangles = new int[]
        {
            0, 1, 2
        };

        mesh.normals = new Vector3[]
        {
            Vector3.back,
            Vector3.back,
            Vector3.back
        };

        GetComponent<MeshFilter>().mesh = mesh;


    }

    void Update()
    {
        
    }
}
