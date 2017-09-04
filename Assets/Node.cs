using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node{
    public bool _canWalk;//是否可行走
    public Vector3 _worldPos;//节点世界坐标
    public int _gridX, _gridY;//在网格中的位置

    public Node _parent;//父节点

    //代价
    public int gCost;
    public int hCost;
    public int fCost { get { return gCost + hCost; } }

    public Node(bool canWalk,Vector3 worldPos,int gridX,int gridY) {
        _canWalk = canWalk;
        _worldPos = worldPos;
        _gridX = gridX;
        _gridY = gridY;
    }

}
