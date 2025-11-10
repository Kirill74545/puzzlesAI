using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundList", menuName = "Custom/Background List")]
public class BackgroundListSO : ScriptableObject
{
    [Header("Список фонов")]
    public BackgroundDataSO[] backgrounds;
}