using UnityEngine;

[CreateAssetMenu(fileName = "FormationPattern", menuName = "Formation/Pattern")]
public class FormationPattern : ScriptableObject
{
    public string patternName;
    public int minWaveToAppear = 0;
    public bool[,] grid;
}
