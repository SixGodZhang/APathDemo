using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class FindPath : MonoBehaviour
{

    private Grid _grid;//grid脚本

    LinkedList<Node> openSet = new LinkedList<Node>();//定义开放列表
    List<Node> closeSet = new List<Node>();//定义封闭列表


    void Awake()
    {
        _grid = GetComponent<Grid>();
    }

    void Start()
    {

    }

    /// <summary>
    /// 从网格中获取路径
    /// </summary>
    /// <param name="startPos">玩家位置</param>
    /// <param name="endPos">目标点</param>
    public List<Node> GetPathFromGrid(Vector3 startPos, Vector3 endPos)
    {

        Node startNode = _grid.GetPlayerPositionNode(startPos);
        Node endNode = _grid.GetPlayerPositionNode(endPos);
        List<Node> finalPath = new List<Node>();//最后路径点集合

        //openSet.Add(startNode);//添加开始节点到开放列表
        openSet.AddFirst(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.First.Value;
            openSet.RemoveFirst();
            closeSet.Add(currentNode);


            //如果当前节点为结束节点，则结束寻路
            if (currentNode == endNode)
            {
                finalPath = GetFinalPath(startNode, endNode);//获取最后的路径
                break;
            }

            //获取当前节点附近的节点,并把他们加入到开放列表中,开放列表是一个有序的列表
            List<Node> nearNodeList = GetNearNodesByCurrentNode(currentNode, startNode, endNode);
            for (int i = 0; i < nearNodeList.Count; i++)
            {
                if (openSet.Count > 0)
                {//如果开放列表中有值的话
                    LinkedListNode<Node> tempNode = openSet.First;//需要遍历的值
                    while (tempNode != null)//如果tempNode不为NULL
                    {
                        if (nearNodeList[i].fCost <= tempNode.Value.fCost)
                        {
                            openSet.AddBefore(tempNode, nearNodeList[i]);//增加节点
                            break;
                        }

                        tempNode = tempNode.Next;//下一个节点
                    }
                }
                else if (openSet.Count == 0)
                {
                    openSet.AddFirst(nearNodeList[i]);
                }


            }

            //openSet.AddRange(GetNearNodesByCurrentNode(currentNode, startNode, endNode));
        }

        return finalPath;
    }

    //最后的路径
    List<Node> GetFinalPath(Node startNode, Node endNode)
    {
        List<Node> finalPathNodes = new List<Node>();
        Node currentNode = endNode;

        while (currentNode._parent != null)
        {
            currentNode._worldPos.y = 0.5f;
            finalPathNodes.Add(currentNode);
            currentNode = currentNode._parent;
        }

        if (currentNode == startNode)
        {
            finalPathNodes.Add(currentNode);
        }

        Profiler.BeginSample("BeginSample");
        ReleaseResource();
        Profiler.EndSample();
        return finalPathNodes;
    }

    void ReleaseResource()
    {
        if (openSet.Count > 0)
        {
            openSet.Clear();
        }

        if (closeSet.Count > 0)
        {
            closeSet.Clear();
        }
    }

    /// <summary>
    /// 获取中心节点周围的所有相邻节点
    /// </summary>
    /// <param name="centerNode">中心节点</param>
    List<Node> GetNearNodesByCurrentNode(Node centerNode, Node startNode, Node endNode)
    {
        List<Node> nearNodeList = new List<Node>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;//忽略中心节点
                Node tempNode = _grid.grid[centerNode._gridX + i, centerNode._gridY + j];
                //附近的节点必须是可行走的且不被包含在封闭列表中
                if (tempNode._canWalk && !closeSet.Contains(tempNode))
                {
                    //计算节点的gCost、fCost、hCost
                    //gCost=开始点到centerNode的gCost+centerNode到tempNode的gCost

                    if (!openSet.Contains(tempNode))
                    {//如果当前节点不在开放列表中
                        tempNode.gCost = centerNode.gCost + CalculateNodeCost(centerNode, tempNode);
                        tempNode.hCost = CalculateNodeCost(tempNode, endNode);
                        tempNode._parent = centerNode;
                        nearNodeList.Add(tempNode);
                    }
                    else
                    {//如果当前节点在开放列表中
                        int tempGCost = centerNode.gCost + CalculateNodeCost(centerNode, tempNode);
                        //然后做比较，如过tempGCost小于tempNode的原有的gCost，则修改tempNode的gCost
                        if ((tempNode.gCost == 0) || (tempNode.gCost != 0 && tempGCost < tempNode.gCost))
                        {
                            tempNode._parent = centerNode;
                            tempNode.gCost = centerNode.gCost + CalculateNodeCost(centerNode, tempNode);
                        }
                    }

                }
            }
        }

        return nearNodeList;
    }

    /// <summary>
    /// 计算节点间的路径代价
    /// </summary>
    /// <param name="nodeA">节点A</param>
    /// <param name="nodeB">节点B</param>
    /// <returns></returns>
    int CalculateNodeCost(Node nodeA, Node nodeB)
    {
        int xDistance = Mathf.Abs(nodeA._gridX - nodeB._gridX);//节点A、B的x轴偏移量
        int yDistance = Mathf.Abs(nodeA._gridY - nodeB._gridY);//节点A、B的y轴偏移量

        if (xDistance < yDistance)
        {
            return 14 * xDistance + (yDistance - xDistance) * 10;
        }
        else
        {
            return 14 * yDistance + (xDistance - yDistance) * 10;
        }
    }
}
