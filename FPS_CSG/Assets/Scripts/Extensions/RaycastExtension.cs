using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RaycastExtension
{
    /// <summary>
    /// Gets The material from the generated csg mesh
    /// </summary>
    /// <param name="hit"></param>
    /// <returns>The material from the raycast hit</returns>
    public static Material GetBrushMaterial(this RaycastHit hit)
    {
        MeshCollider collider = hit.collider as MeshCollider;

        if (!collider)
            return null;

        Mesh mesh = collider.sharedMesh;

        // Finds the submesh that contains the hit triangle
        int limit = hit.triangleIndex * 3;
        int submesh;
        for (submesh = 0; submesh < mesh.subMeshCount; submesh++)
        {
            int numIndices = mesh.GetTriangles(submesh).Length;
            if (numIndices > limit)
                break;

            limit -= numIndices;
        }

        MeshRenderer renderer = collider.GetComponent<MeshRenderer>();

        if (!renderer)
            renderer = collider.GetComponentInParent<MeshRenderer>();

        if (!renderer)
            return null;

        return renderer.sharedMaterials[submesh];
    }

    public static Material GetMaterial(this RaycastHit hit)
    {
        return null;
    }
}
