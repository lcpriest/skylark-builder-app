using System.Collections;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;

public class STLExporter
{
    public static void ExportGroupAsSTLFile(GroupBehaviour group, string fileName)
    {
        string modelName = fileName;

        // Get a list of all mesh filters in children.
        List<MeshFilter> groupMeshes = new List<MeshFilter>(group.GetComponentsInChildren<MeshFilter>());
        List<MeshFilter> removeMeshes = new List<MeshFilter>();
        foreach (MeshFilter Piece in groupMeshes)
        {
            if (Piece.GetComponent<MeshRenderer>() == null)
                removeMeshes.Add(Piece);
        }

        // Remove ones where mesh renderer is null.
        removeMeshes.ForEach(meshFilter =>
        {
            groupMeshes.Remove(meshFilter);
        });

        // Convert all mesh data into string data in STL Ascii format.
        StringBuilder meshStringBuilder = new StringBuilder();

        foreach (MeshFilter meshFilter in groupMeshes)
        {
            Mesh mesh = meshFilter.sharedMesh;

            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;

            meshStringBuilder.Append("solid " + mesh.name + "\n");
            for (int i = 0; i + 2 < triangles.Count(); i++)
            {
                Vector3 wv = meshFilter.transform.TransformDirection(normals[triangles[i]]);
                meshStringBuilder.Append(" facet normal " + wv.x.ToString("F6") + " "
                    + wv.z.ToString("F6") + " "
                    + wv.y.ToString("F6") + "\n");

                meshStringBuilder.Append("  outer loop" + "\n");

                wv = meshFilter.transform.TransformPoint(vertices[triangles[i]]);
                meshStringBuilder.Append("    vertex " + wv.x.ToString("F6") + " "
                    + wv.z.ToString("F6") + " "
                    + wv.y.ToString("F6") + "\n");

                wv = meshFilter.transform.TransformPoint(vertices[triangles[i + 1]]);
                meshStringBuilder.Append("    vertex " + wv.x.ToString("F6") + " "
                    + wv.z.ToString("F6") + " "
                    + wv.y.ToString("F6") + "\n");

                wv = meshFilter.transform.TransformPoint(vertices[triangles[i + 2]]);
                meshStringBuilder.Append("    vertex " + wv.x.ToString("F6") + " "
                    + wv.z.ToString("F6") + " "
                    + wv.y.ToString("F6") + "\n");

                meshStringBuilder.Append("  endloop" + "\n");
                meshStringBuilder.Append(" endfacet" + "\n");
                i += 2;
            }
            meshStringBuilder.Append("endsolid" + "\n" + "\n");
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadTextFile($"{modelName}.STL", meshStringBuilder.ToString());
#elif UNITY_EDITOR
        // Export as file for testing.
        File.WriteAllText($"{Application.dataPath}/{modelName}.STL", meshStringBuilder.ToString());
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadTextFile(string fileName, string data);
    
    [DllImport("__Internal")]
    private static extern void DownloadBinaryFile(string fileName, byte[] dataPtr, int dataSize);
#endif
}
