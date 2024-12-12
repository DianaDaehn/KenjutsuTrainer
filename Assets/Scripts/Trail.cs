using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Trail : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public MeshFilter meshFilter;
    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int i = 1; i < lines.Count; i++)
        {
            Vector3 a = lines[i].lineStart;
            Vector3 b = lines[i].lineEnd;
            Vector3 c = lines[i-1].lineStart;
            Vector3 d = lines[i-1].lineEnd;

            int n = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            indices.Add(n+2);
            indices.Add(n+3);
            indices.Add(n+1);
            indices.Add(n+2);
            indices.Add(n+1);
            indices.Add(n);

            indices.Add(n);
            indices.Add(n+1);
            indices.Add(n+2);
            indices.Add(n+1);
            indices.Add(n+3);
            indices.Add(n+2);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
    }

    void FixedUpdate()
    {
        lines.Add(new Line
        {
            lineStart = start.position,
            lineEnd = end.position,
        });

        if (lines.Count > 0.5 / Time.fixedDeltaTime)
        {
            lines.RemoveAt(0);
        }
    }

    struct Line
    {
        public Vector3 lineStart;
        public Vector3 lineEnd;
    }
    private List<Line> lines = new List<Line>();
}
