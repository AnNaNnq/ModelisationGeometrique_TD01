using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System;

public class MeshLoader : MonoBehaviour
{
    public string filePath = "./bunny.off";
    private Mesh mesh;

    private Vector3[] vertices;
    private List<int> triangles;

    void Start()
    {
        LoadMeshFromFile(filePath);
        TraceMaillage();
    }

    void LoadMeshFromFile(string path)
    {
        string[] lines = File.ReadAllLines(path);
        if (lines[0] != "OFF")
        {
            Debug.LogError("Le fichier n'est pas un fichier OFF valide.");
            return;
        }

        // R�cup�ration des sommets et des facettes
        string[] counts = lines[1].Split(' ');
        int numVertices = int.Parse(counts[0]);
        int numFaces = int.Parse(counts[1]);

        // Initialisation des listes des sommets et des facettes
        vertices = new Vector3[numVertices];
        triangles = new List<int>();

        // Parsing des sommets
        for (int i = 0; i < numVertices; i++)
        {
            string[] vertexData = lines[i + 2].Split(' ');
            // CultureInfo.InvariantCulture assure que le point est utilis� comme s�parateur d�cimal
            float x = float.Parse(vertexData[0], CultureInfo.InvariantCulture);
            float y = float.Parse(vertexData[1], CultureInfo.InvariantCulture);
            float z = float.Parse(vertexData[2], CultureInfo.InvariantCulture);
            vertices[i] = new Vector3(x, y, z);
        }

        // Calcul du centre de gravit� du maillage
        Vector3 centerOfGravity = Vector3.zero;
        for (int i = 0; i < vertices.Length; i++)
        {
            centerOfGravity += vertices[i];
        }
        centerOfGravity /= vertices.Length;

        // Recentrage des sommets par rapport au centre de gravit�
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= centerOfGravity;
        }

        // Normalisation de la taille du maillage
        float maxCoord = 0f;
        for (int i = 0; i < vertices.Length; i++)
        {
            maxCoord = Mathf.Max(maxCoord, Mathf.Abs(vertices[i].x), Mathf.Abs(vertices[i].y), Mathf.Abs(vertices[i].z));
        }

        // Division de chaque coordonn�e par la valeur maximale
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] /= maxCoord;
        }

        // Parsing des facettes
        for (int i = 0; i < numFaces; i++)
        {
            string[] faceData = lines[i + 2 + numVertices].Split(' ');

            if (faceData[0] != "3")
            {
                Debug.LogError("Le fichier contient des facettes non triangulaires.");
                return;
            }

            int v0 = int.Parse(faceData[1]);
            int v1 = int.Parse(faceData[2]);
            int v2 = int.Parse(faceData[3]);

            triangles.Add(v0);
            triangles.Add(v1);
            triangles.Add(v2);
        }

        // Mesh
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.mesh = mesh;
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }
    }

    void TraceMaillage()
    {
        Debug.Log("=== Trac� du maillage ===");

        Debug.Log("Sommets :");
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Debug.Log($"Sommet {i} : ({vertex.x}, {vertex.y}, {vertex.z})");
        }

        Debug.Log("Facettes :");
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];
            Debug.Log($"Facette {i / 3} : Sommets ({v0}, {v1}, {v2})");
        }

        Debug.Log("=== Fin du trac� du maillage ===");
    }

    void SaveMeshToFile(string path)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("OFF");
                writer.WriteLine($"{mesh.vertexCount} {mesh.triangles.Length / 3} 0");
                foreach (Vector3 vertex in mesh.vertices)
                {
                    writer.WriteLine($"{vertex.x} {vertex.y} {vertex.z}");
                }

                for (int i = 0; i < mesh.triangles.Length; i += 3)
                {
                    writer.WriteLine($"3 {mesh.triangles[i]} {mesh.triangles[i + 1]} {mesh.triangles[i + 2]}");
                }
            }
        }
        catch
        {
            Debug.LogError("Erreur");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            string absolutePath = Path.Combine(Application.dataPath, "saved_mesh.off");
            SaveMeshToFile(absolutePath);
        }
    }
}
