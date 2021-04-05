using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Text;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject itsObject;

    private Texture2D texture;
    
    List<GameObject> sceneObjects = new List<GameObject>();
    void Start()
    {
        Debug.Log("Hello console");
        Debug.Log(Application.dataPath);
        FileStream fileStream = new FileStream(string.Format("{0}/{1}", Application.dataPath + "/Samples/json", "dong.json"), FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        string jsonData = System.Text.Encoding.UTF8.GetString(data);

        Debug.Log(jsonData);
        ZigbangResponse response = JsonUtility.FromJson<ZigbangResponse>(jsonData);
        response.Print();


        
        byte[] textureBuffer = System.IO.File.ReadAllBytes(string.Format("{0}/{1}", Application.dataPath + "/Samples/texture", "buildingTester_d.png"));
        if (textureBuffer.Length > 0) {
            texture = new Texture2D(0, 0); 
            texture.LoadImage(textureBuffer); 
            Debug.Log("Texture");
        }
        

        createApartments(response.data);
        itsObject = new GameObject("test object");
    }

    private void createApartments(List<DongModel> apartments) 
    {
        foreach (DongModel dong in apartments)
        {
            RoomType modelData = dong.roomtypes[0];

            // surfaces
            List<Vector3> vertexList = new List<Vector3>();
            foreach(string encodedVertices in modelData.coordinatesBase64s)
            {
                Vector3[] partials = paseVertexies(encodedVertices);
                foreach(Vector3 v in partials) {
                    vertexList.Add(v);
                }
            }
            
            Vector3[] vertexArray = vertexList.ToArray();
            Mesh mesh = createMesh(vertexArray);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject obj = new GameObject("Apart" + sceneObjects.Count);
            obj.AddComponent<MeshFilter>();
            obj.GetComponent<MeshFilter>().mesh = mesh;
            Material material = new Material(Shader.Find("Unlit/Texture"));
            material.mainTexture = texture as Texture;
            material.SetTextureScale("_MainTex", new Vector2(1.0f,3));
            // material.SetTexture("mainTexture", texture);
            obj.AddComponent<MeshRenderer>();
            obj.GetComponent<MeshRenderer>().material = material;

            sceneObjects.Add(obj);
        }
    }

    private Vector3[] paseVertexies(string encodedString)
    {
        List<Vector3> vertices = new List<Vector3>();

        byte[] byteBuffer = Convert.FromBase64String(encodedString);
        Debug.Log("Sizeof Buffer is " + byteBuffer.Length);
        Debug.Assert(byteBuffer.Length%12 == 0, "byte buffer's size is not muplied by 3");
        int read = 0;
        while(read < byteBuffer.Length) 
        {
            float[] vs = new float[3];
            Buffer.BlockCopy(byteBuffer, read, vs, 0, sizeof(float) * 3);
            read += sizeof(float) * 3;
            Vector3 vertex = new Vector3(vs[0], vs[2], vs[1]);
            vertices.Add(vertex);
        }

        string vString = vertices.Count + " vertices\n";
        foreach(Vector3 v in vertices)
        {
            vString += v.ToString("F4") + "\n";
        }
        Debug.Log(vString);
        
        return vertices.ToArray();
    }

    private Mesh createMesh(Vector3[] vertices)
    {
        Debug.Assert(vertices.Length % 3 == 0, "Surface may not be composed by 4 vertices.");

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        List<int> indices = new List<int>();
        for(int i = 0; i < vertices.Length; ++i)
        {
            indices.Add(i);
        }
        Debug.Log(string.Join(", ", indices));
        mesh.triangles = indices.ToArray();

        List<Vector2> uvs = new List<Vector2>();
        for(int i = 0; i < vertices.Length; i += 6) {
            
            Vector3 normal = getNormal(vertices[i + 0], vertices[i + 1], vertices[i + 2]);
            float leftSide = 0.0f;
            float rightSide = 12.0f / 24.0f;
            uvs.Add(new Vector2(leftSide, 0.0f));
            uvs.Add(new Vector2(rightSide, 0.0f));
            uvs.Add(new Vector2(rightSide, 1.0f));
            
            uvs.Add(new Vector2(rightSide, 1.0f));
            uvs.Add(new Vector2(leftSide, 1.0f));
            uvs.Add(new Vector2(leftSide, 0.0f));
            
        }
        mesh.uv = uvs.ToArray();
        return mesh;
    }

    private Vector3 getNormal(Vector3 a,Vector3 b,Vector3 c )
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        Vector3 perp = Vector3.Cross(side1, side2);
        perp /= perp.magnitude;
        return perp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
