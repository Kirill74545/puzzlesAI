using UnityEngine;

[System.Serializable]
public class OpenverseResponse
{
    public OpenverseImage[] results;
}

[System.Serializable]
public class OpenverseImage
{
    public string url;       
    public string title;      
    public string license;    
}