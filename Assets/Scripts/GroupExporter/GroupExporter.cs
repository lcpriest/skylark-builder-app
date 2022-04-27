using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using System.Text;
using System.IO;
using System;
using System.Runtime.InteropServices;

public class GroupExporter
{
    struct ObjMaterial
    {
        public string name;
        public string textureName;
        public Texture2D mainTexture;
    }

    public static void ExportGroupAsFile(GroupBehaviour group)
    {
        ExportGroupAsOBJFile(group);
    }

    static void ExportGroupAsOBJFile(GroupBehaviour group)
    {
        string modelName = "EXPORTED";

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

        // Data for writing OBJ data.
        int vertexOffset = 0;
        int normalOffset = 0;
        int uvOffset = 0;

        Dictionary<string, ObjMaterial> materialList = new Dictionary<string, ObjMaterial>();

        // Convert all mesh data into string data in OBJ format.
        string meshData_OBJ = $"mtllib {modelName}.mtl \n";

        foreach (MeshFilter meshFilter in groupMeshes)
        {
            StringBuilder meshStringBuilder = new StringBuilder();
            Mesh mesh = meshFilter.sharedMesh;
            Material[] mats = meshFilter.GetComponent<Renderer>().sharedMaterials;
            
            meshStringBuilder.Append("g ").Append(meshFilter.name).Append("\n");

            if (!mesh.isReadable)
            {
                Debug.Assert(false, $"Mesh {meshFilter.name} is not readable. Please enable read in import settings.");
                continue;
            }

            // Vertex data.
            foreach (Vector3 lv in mesh.vertices)
            {
                Vector3 wv = meshFilter.transform.TransformPoint(lv);

                // This is sort of ugly - inverting x-component since we're in a different coordinate system than "everyone" is "used to".
                meshStringBuilder.Append(string.Format("v {0} {1} {2}\n", wv.x, wv.y, wv.z));
            }
            meshStringBuilder.Append("\n");

            // Normal data.
            foreach (Vector3 lv in mesh.normals)
            {
                Vector3 wv = meshFilter.transform.TransformDirection(lv);
                meshStringBuilder.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
            }
            meshStringBuilder.Append("\n");

            // UV data.
            foreach (Vector3 v in mesh.uv)
            {
                meshStringBuilder.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }

            // Material data.
            for (int matIndex = 0; matIndex < mesh.subMeshCount; matIndex++)
            {
                meshStringBuilder.Append("\n");
                meshStringBuilder.Append("usemtl ").Append(mats[matIndex].name).Append("\n");
                meshStringBuilder.Append("usemap ").Append(mats[matIndex].name).Append("\n");

                // See if this material is already in the materiallist.
                try
                {
                    ObjMaterial objMaterial = new ObjMaterial();

                    objMaterial.name = mats[matIndex].name;

                    if (mats[matIndex].mainTexture)
                        objMaterial.mainTexture = mats[matIndex].mainTexture as Texture2D;
                    else
                        objMaterial.mainTexture = null;

                    materialList.Add(objMaterial.name, objMaterial);
                }
                catch (ArgumentException)
                {
                    //Already in the dictionary
                }

                int[] triangles = mesh.GetTriangles(matIndex);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    // Because we inverted the x-component, we also needed to alter the triangle winding.
                    meshStringBuilder.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                           triangles[i] + 1 + normalOffset, triangles[i + 1] + 1 + vertexOffset, triangles[i + 2] + 1 + uvOffset));
                }
            }

            vertexOffset += mesh.vertices.Length;
            normalOffset += mesh.normals.Length;
            uvOffset += mesh.uv.Length;

            meshData_OBJ += meshStringBuilder.ToString();
        }

        // Generate the .MTL file for materials.
        string mtlData = "";

        Dictionary<string, ObjMaterial>.KeyCollection keys = materialList.Keys;
        foreach (string key in keys)
        {
            if (materialList[key].mainTexture)
            {
                // Write the textures as PNG.
                Texture2D decompressed = DeCompress(materialList[key].mainTexture);
                byte[] imgData = decompressed.EncodeToPNG();
#if UNITY_WEBGL && !UNITY_EDITOR
            //DownloadBinaryFile($"{modelName}_{materialList[key].name}.png", imgData, imgData.Length);
#elif UNITY_EDITOR
                File.WriteAllBytes($"{Application.dataPath}/{modelName}_{materialList[key].name}.png", imgData);
#endif

                // Add to .MTL file.
                mtlData += $"newmtl {materialList[key].name}\n" +
                            "Ns 225.000000\n" +
                            "Ka 1.000000 1.000000 1.000000\n" +
                            "Kd 0.800000 0.800000 0.800000\n" +
                            "Ks 0.500000 0.500000 0.500000\n" +
                            "Ke 0.000000 0.000000 0.000000\n" +
                            "Ni 1.450000\n" +
                            "d 1.000000\n" +
                            "illum 2\n" +
                            $"map_Kd {modelName}_{materialList[key].name}.png\n";
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR        
        //DownloadTextFile($"{modelName}.OBJ", meshData_OBJ);
        //DownloadTextFile($"{modelName}.MTL", mtlData);
#elif UNITY_EDITOR
        // Export as file for testing.
        File.WriteAllText($"{Application.dataPath}/{modelName}.OBJ", meshData_OBJ);        
        // Export .MTL file.
        File.WriteAllText($"{Application.dataPath}/{modelName}.MTL", mtlData);          
#endif
    }

    static Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
#if UNITY_WEBGL && !UNITY_EDITOR
    //[DllImport("__Internal")]
    //private static extern void DownloadTextFile(string fileName, string data);
    
    //[DllImport("__Internal")]
    //private static extern void DownloadBinaryFile(string fileName, byte[] dataPtr, int dataSize);
#endif
}
