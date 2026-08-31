using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Chess Puzzle/Level Data")]
public class LevelDataSO : ScriptableObject
{
    public string levelName;
    public Color levelThemeColor = Color.white;
    public bool isTutorial;
    public List<PieceSpawnData> piecesToSpawn = new List<PieceSpawnData>();
}