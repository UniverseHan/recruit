using System.Collections.Generic;

[System.Serializable]
public class RoomTypeMeta {
  public int 룸타입id;
}

[System.Serializable]
public class RoomType {
  public List<string> coordinatesBase64s;
  public RoomTypeMeta meta;

  public string ToString() {
    string coordinateStrings = "";
    foreach (string coordinatesBase64 in coordinatesBase64s)
    {
      coordinateStrings += "    coordinateStrings: " + coordinatesBase64 + "\n";
    }
    return meta.ToString() + "\n" + coordinateStrings;
  }
}