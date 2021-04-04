using System.Collections.Generic;

[System.Serializable]
public class DongModelMeta
{
  public int bd_id;
  public string 동;
  public string 지면높이;

  public string ToString() {
    return string.Format(
      "[\n" +
      "    bd_id: {0}],\n" +
      "    동: {1},\n" +
      "    지면높이: {2}\n" +
      "]",
      bd_id, 동, 지면높이);
  }
}

[System.Serializable]
public class DongModel 
{
  public List<RoomType> roomtypes;
  public DongModelMeta meta;

  public string ToString() {
    string roomTypeStrings = "";
    foreach (RoomType roomtype in roomtypes) 
    {
      roomTypeStrings += roomtype.ToString() + "\n";
    }
    return meta.ToString() + "\n" + roomTypeStrings;
  }
}