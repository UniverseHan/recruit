using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
