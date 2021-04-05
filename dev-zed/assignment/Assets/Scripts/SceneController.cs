using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Text;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
    private Texture2D texture;
    
    List<GameObject> sceneObjects = new List<GameObject>();
    void Start()
    {
        texture = loadApartmentTexture();
        ZigbangResponse response = fetchApartmentsData();
        createApartments(response.data);
    }

    private ZigbangResponse fetchApartmentsData()
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}", Application.dataPath + "/Samples/json", "dong.json"), FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        string jsonData = System.Text.Encoding.UTF8.GetString(data);

        return JsonUtility.FromJson<ZigbangResponse>(jsonData);
    }

    private Texture2D loadApartmentTexture()
    {
        byte[] textureBuffer = System.IO.File.ReadAllBytes(string.Format("{0}/{1}", Application.dataPath + "/Samples/texture", "buildingTester_d.png"));
        if (textureBuffer.Length <= 0)
        {
            Debug.Log("Texture loading failed.");
            return null;
        }

        texture = new Texture2D(0, 0); 
        texture.LoadImage(textureBuffer); 
        return texture;
    }

    private void createApartments(List<DongModel> apartments) 
    {
        foreach (DongModel dong in apartments)
        {
            RoomType modelData = dong.roomtypes[0];

            // surfaces
            List<Vector3> vertexList = new List<Vector3>();
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            foreach(string encodedVertices in modelData.coordinatesBase64s)
            {
                Vector3[] partials = paseVertexies(encodedVertices);
                foreach(Vector3 v in partials) {
                    vertexList.Add(v);
                    minY = Math.Min(minY, v.y);
                    maxY = Math.Max(maxY, v.y);
                }
            }

            float height = Math.Abs(maxY - minY);
            
            Vector3[] vertexArray = vertexList.ToArray();
            Mesh mesh = createMesh(vertexArray);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject obj = new GameObject("Apart" + sceneObjects.Count);
            obj.AddComponent<MeshFilter>();
            obj.GetComponent<MeshFilter>().mesh = mesh;
            Material material = new Material(Shader.Find("Unlit/Texture"));
            material.mainTexture = texture as Texture;
            material.SetTextureScale("_MainTex", new Vector2(1.0f,(float)Math.Floor(height/3)));
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
        mesh.triangles = generateIndices(vertices);
        mesh.uv = generateUVs(vertices);
        return mesh;
    }

    /**
     * @brief 
     * @desc 정점들은 폴리곤 단위로 구성되어 있다. 그렇기 때문에 중복된 정점들도 존재한다. 인덱스는 1:1로 매칭된다.
     **/
    private int[] generateIndices(Vector3[] vertices)
    {
        List<int> indices = new List<int>();
        for(int i = 0; i < vertices.Length; ++i)
        {
            indices.Add(i);
        }
        return indices.ToArray();
    }

    private Vector2[] generateUVs(Vector3[] vertices)
    {
        List<Vector2> uvs = new List<Vector2>();
        for(int i = 0; i < vertices.Length; i += 6) {
            Vector3 normal = getNormal(vertices[i + 0], vertices[i + 1], vertices[i + 2]);
            Vector3 range = getUVByDirection(normal);
            float leftSide = range.x;
            float rightSide = range.y;
            uvs.Add(new Vector2(leftSide, 0.5f));
            uvs.Add(new Vector2(rightSide, 0.5f));
            uvs.Add(new Vector2(rightSide, 1.0f));
            
            uvs.Add(new Vector2(rightSide, 1.0f));
            uvs.Add(new Vector2(leftSide, 1.0f));
            uvs.Add(new Vector2(leftSide, 0.5f));
        }
        return uvs.ToArray();
    }

    private Vector3 getNormal(Vector3 a,Vector3 b,Vector3 c )
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        Vector3 perp = Vector3.Cross(side1, side2);
        perp /= perp.magnitude;
        return perp;
    }

    private Vector2 getUVByDirection(Vector3 normal) 
    {
        Vector3 projectedOnXZ = new Vector3(normal.x, 0.0f, normal.z);
        float aDotB = Vector3.Dot(projectedOnXZ, Vector3.forward);
        float theta = (float)Math.Acos(aDotB / (projectedOnXZ.magnitude * Vector3.forward.magnitude));
        float degree = (float)(theta * (180.0f / Math.PI));

        Debug.Log("degree is " + degree + ", nor * forwad = " + aDotB);

        // case 3
        if (Math.Abs(Math.Abs(Vector3.Dot(normal, Vector3.up)) - 1) < 0.001f) 
        {
            return new Vector2(18.0f/24.0f, 1.0f);
        } 
        // case 1
        else if (aDotB <= 0 && degree >= 140.0f) 
        {
            return new Vector2(0.0f, 12.0f/24.0f);
        } 
        else // case 2
        {
            return new Vector2(12.0f/24.0f, 18.0f/24.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
