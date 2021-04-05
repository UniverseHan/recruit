using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZigbangResponse
{
  public int code;
  public bool success;

  public List<DongModel> data;

  public void Print() {
    Debug.Log("code: " + code);
    Debug.Log("success: " + success);
    string stringForDongs = "";
    foreach (DongModel dongModel in data) {
      stringForDongs += dongModel.ToString() + "\n";
    }
    Debug.Log(stringForDongs);
  }
}