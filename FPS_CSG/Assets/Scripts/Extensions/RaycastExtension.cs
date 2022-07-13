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
    /// <summary>
    /// Gets the material of the model that the raycast hit i the mesh is marked as "Read/Write enabled" in its import settings
    /// </summary>
    /// <param name="hit"></param>
    /// <returns>the hit material</returns>
    public static Material GetMaterial(this RaycastHit hit)
    {
        MeshRenderer renderer = hit.collider.GetComponent<Renderer>() as MeshRenderer;

        if (renderer == null) { return null; }
        if (renderer.sharedMaterial == null) { return null; }
        int triangleIndex = hit.triangleIndex;
        Mesh mesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;
        int materialIndex = -1;

        //Debug.Log($"{hit.collider.name}'s mesh readable: {mesh.isReadable}");
        if (mesh.isReadable)
        {
            int triangleCount = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int indexCount = mesh.GetSubMesh(i).indexCount;
                triangleCount = i / 3;
                if (triangleIndex < triangleCount)
                {
                    materialIndex = i;
                }
            }
        }

        if (materialIndex > -1)
        {
            return renderer.sharedMaterials[materialIndex];
        }
        return null;
    }
}