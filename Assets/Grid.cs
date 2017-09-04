using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

    public Node[,] grid;//地图网格
    public Vector2 gridSize;//地图大小
    public float nodeRadius;//节点半径
    private float nodeDiameter;//节点直径

    public LayerMask whatLayer;

    private int gridCountX;//网格数组X轴方向长度
    private int gridCountY;//网格数组Y轴方向长度

    public Transform playerPos;//玩家所处在地图中的节点

    private FindPath path;//路径
    List<Node> finalPath = new List<Node>();//最后路径点集合

    private int index = 0;//计数

    private float time=0.0f;//计时

    void Start () {
        nodeDiameter = nodeRadius * 2;
        gridCountX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
        gridCountY = Mathf.RoundToInt(gridSize.y / nodeDiameter);
        grid = new Node[gridCountX, gridCountY];
        CreateGrid();
        path = GetComponent<FindPath>();//初始化
    }

    /// <summary>
    /// 创建地图网格
    /// </summary>
    public void CreateGrid()
    {
        Vector3 startPoint = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;
        for (int i = 0; i < gridCountX; i++)
        {
            for (int j = 0; j < gridCountY; j++)
            {
                Vector3 worldPos = startPoint + Vector3.right * (i * nodeDiameter + nodeRadius) + Vector3.forward * (j * nodeDiameter + nodeRadius);
                bool canWalk = !Physics.CheckSphere(worldPos, nodeRadius, whatLayer);//检测此处位置是否可行走
                grid[i, j] = new Node(canWalk, worldPos, i, j);
            }
        }
    }

    /// <summary>
    /// 世界坐标系中的点转换为在地图中所处的节点
    /// </summary>
    /// <param name="playerPos">世界坐标系中的点</param>
    public Node GetPlayerPositionNode(Vector3 playerPos) {
        float percentX = (playerPos.x + gridSize.x / 2) / gridSize.x;
        float percentZ = (playerPos.z + gridSize.y / 2) / gridSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridCountX - 1) * percentX);
        int y = Mathf.RoundToInt((gridCountY - 1) * percentZ);

        return grid[x, y];

    }

    void Update () {

        time += Time.deltaTime;
        if (time>0.5f) {
            //如果点击鼠标右键，获取路径
            if (Input.GetMouseButtonDown(0))
            {
                finalPath = new List<Node>();
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    if (hitInfo.transform.CompareTag("floor"))
                    {
                        finalPath = path.GetPathFromGrid(playerPos.position, hitInfo.point);
                        index = finalPath.Count - 1;
                    }
                }
            }

            time = 0.0f;//计时器重置
        }



        //按照预定义路径行走
        WalkByPreDefinedPath(finalPath);
		
	}

    /// <summary>
    /// 按照预定义的路径行走
    /// </summary>
    /// <param name="pathList">预定义路径</param>
    void WalkByPreDefinedPath(List<Node> pathList)
    {
        if (pathList.Count == 0 || pathList == null ) {
            return;
        }
        
        if (Vector3.Distance(playerPos.position, pathList[index]._worldPos)<0.2f) {
            index--;
        }
        if (index < 0)
        {
            index = 0;
            return;
        }
        playerPos.localPosition= Vector3.MoveTowards(playerPos.position, pathList[index]._worldPos, Time.deltaTime * 1.5f);

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 5, gridSize.y));
        if (grid == null) return;
        Node playerNode = GetPlayerPositionNode(playerPos.position);//获取玩家所处地图中的节点
        foreach (Node node in grid) {
            Gizmos.color = node._canWalk ? Color.white : Color.red;
            Gizmos.DrawCube(node._worldPos, Vector3.one*(nodeDiameter-.1f));
        }

        if (playerNode != null && playerNode._canWalk)
        {
            Gizmos.color = Color.cyan;//蓝色
            Gizmos.DrawCube(playerNode._worldPos, Vector3.one * (nodeDiameter - .1f));
        }
        else {
            Debug.Log("playerNode==null");
        }


        if (finalPath!=null) {
            Gizmos.color = Color.black;//黑色
            foreach (Node item in finalPath)
            {
                Gizmos.DrawCube(item._worldPos, Vector3.one * (nodeDiameter - .1f));
            }
            
        }
    }
}
