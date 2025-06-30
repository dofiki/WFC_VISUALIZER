using System;
using System.Collections.Generic;
using UnityEngine;

public enum SocketType
{
    None,
    RoadHorizontal,
    RoadVertical,
    Grass,
    Connectors,
    Crossing
}

[Serializable]
public class TileSockets
{
    public List<SocketType> top = new List<SocketType>();
    public List<SocketType> bottom = new List<SocketType>();
    public List<SocketType> left = new List<SocketType>();
    public List<SocketType> right = new List<SocketType>();
}

public class BaseTile : MonoBehaviour
{
    public TileSockets sockets;
    public Material mat;

    void Start()
    {
        // Apply material if assigned
        if (mat != null)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = mat;
            }
        }
    }


}
