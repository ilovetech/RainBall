using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Utilities
/// </summary>
public static class Utils
{
    public static Vector3 InvalidPoint = Vector3.zero;

    private static DraganBallManager ballManager;
    private static PathAnimationController pathController;
    private static Settings settings;
    private static Shooter shooter;
    private static SelectionDetector selectionDetector;
    private static PathSampler pathSampler;
    private static LevelManager levelManager;

    #region Instance Helper

    public static DraganBallManager BallManager
    {
        get
        {
            if (ballManager == null)
            {
                ballManager = GameObject.FindGameObjectWithTag("BallManager").GetComponent<DraganBallManager>();
            }
            return ballManager;
        }
    }

    public static PathAnimationController PathController
    {
        get
        {
            if (pathController == null)
            {
                pathController = GameObject.FindGameObjectWithTag("PathController").GetComponent<PathAnimationController>();
            }
            return pathController;
        }
    }

    public static Settings Settings
    {
        get
        {
            if (settings == null)
            {
                settings = GameObject.FindGameObjectWithTag("Settings").GetComponent<Settings>();
            }
            return settings;
        }
    }

    public static Shooter Shooter
    {
        get
        {
            if (shooter == null)
            {
                shooter = GameObject.FindGameObjectWithTag("Shooter").GetComponent<Shooter>();
            }
            return shooter;
        }
    }

    public static SelectionDetector SelectionDetector
    {
        get
        {
            if (selectionDetector == null)
            {
                selectionDetector = GameObject.FindGameObjectWithTag("SelectionDetector").GetComponent<SelectionDetector>();
            }
            return selectionDetector;
        }
    }

    public static PathSampler PathSampler
    {
        get
        {
            if (pathSampler == null)
            {
                pathSampler = GameObject.FindGameObjectWithTag("PathSampler").GetComponent<PathSampler>();
            }
            return pathSampler;
        }
    }

    public static LevelManager LevelManager
    {
        get
        {
            if (levelManager == null)
            {
                levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
            }
            return levelManager;
        }
    }
    #endregion

    #region Intersection

    /// <summary>
    /// Get intersection list between p1 and p2
    /// </summary>
    /// <param name="p1">Starting point</param>
    /// <param name="p2">Ending point</param>
    /// <param name="intersectionNum">Intersection number</param>
    /// <returns>Position list</returns>
    public static Vector3[] GetIntersectionList(Vector3 p1, Vector3 p2, int intersectionNum)
    {
        var intersectionList = new Vector3[intersectionNum + 1];
        for (var i = 0; i <= intersectionNum; ++i)
        {
            var intersectionPoint =
                new Vector3((p1.x * (intersectionNum - i) + p2.x * i) / intersectionNum, (p1.y * (intersectionNum - i) + p2.y * i) / intersectionNum,
                            (p1.z * (intersectionNum - i) + p2.z * i) / intersectionNum);
            intersectionList[i] = intersectionPoint;
        }
        return intersectionList;
    }

    /// <summary>
    /// Trim list node list to list with node count
    /// </summary>
    /// <param name="nodeList">Node list to trim</param>
    /// <param name="nodeCount">Node count to trim</param>
    /// <returns>Trimmed list</returns>
    public static List<Vector3> TrimList(List<Vector3> nodeList, int nodeCount)
    {
        if (nodeCount <= 0)
        {
            Debug.Log("Node count should be greater than 0.");
            return new List<Vector3>();
        }

        var trimList = new List<Vector3>();

        if (nodeList.Count <= nodeCount)
        {
            trimList.AddRange(nodeList);
            return trimList;
        }

        var byPassNode = (float)nodeList.Count / (nodeCount - 1);
        for (var i = 0; i < nodeCount - 1; ++i)
        {
            trimList.Add(nodeList[(int)(i * byPassNode)]);
        }
        trimList.Add(nodeList[nodeList.Count - 1]);
        return trimList;
    }

    /// <summary>
    /// Path length
    /// </summary>
    /// <param name="nodeList">Node list to calculate</param>
    /// <returns>The path length</returns>
    public static float PathLength(List<Vector3> nodeList)
    {
        var result = 0f;
        for (int i = 0; i < nodeList.Count - 1; i++)
        {
            result += Mathf.Abs(Vector3.Distance(nodeList[i], nodeList[i + 1]));
        }
        return result;
    }

    #endregion

    #region Collision Detection

    public static GameObject FindNearestByAngle(GameObject shootBall, Vector3 direction, List<BallUpdater> ballUpdaterList)
    {
        GameObject result = null;
        float smallestAngle = float.MaxValue;
        ballUpdaterList.ForEach(ballUpdater =>
        {
            var ball = ballUpdater.gameObject;
            var ballDirection = ball.transform.position - shootBall.transform.position;
            var angle = Math.Abs(Vector3.Angle(ballDirection, direction));
            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                result = ball;
            }
        });
        Debug.Log("Find Nearest Ball: " + result.transform.position);
        return result;
    }

    public static GameObject FindNearestByRay(Vector3 point, Vector3 direction)
    {
        GameObject result = null;
        RaycastHit hit;
        if (Physics.Raycast(point, direction, out hit))
        {
            result = hit.transform.gameObject;
        }
        if (result != null)
        {
            Debug.Log("Find Nearest Ball 2: " + result.transform.position);
        }
        return result;
    }

    public static GameObject FindNearestByRay(Vector3 point, Vector3 direction, float distance)
    {
        GameObject result = null;
        RaycastHit hit;
        if (Physics.Raycast(point, direction, out hit, distance))
        {
            result = hit.transform.gameObject;
        }
        if (result != null)
        {
            Debug.Log("Find Nearest Ball 2: " + result.transform.position);
        }
        return result;
    }

    public static GameObject FindNearestBySphear(Vector3 center, float radius, Vector3 direction)
    {
        GameObject result = null;
        RaycastHit hit;
        if (Physics.SphereCast(center, radius, direction, out hit, radius))
        {
            result = hit.transform.gameObject;
        }
        if (result != null)
        {
            Debug.Log("Find Nearest Ball by sphear: " + result.transform.position);
        }
        return result;
    }

    #endregion

    #region Path

    /// <summary>
    /// Configure tween path to game object
    /// </summary>
    /// <param name="gameObject">Game object to attach to</param>
    /// <param name="positionList">Position list</param>
    /// <param name="pathName">Path name</param>
    public static void ConfigureTweenPath(GameObject gameObject, List<Vector3> positionList, string pathName)
    {
        Object.DestroyImmediate(gameObject.GetComponent<iTweenPath>());
        var path = gameObject.AddComponent<iTweenPath>();
        path.enabled = false;
        path.Initialized = true;
        path.PathName = pathName;
        path.PathColor = Color.red;
        path.PathVisible = true;
        path.NodeCount = positionList.Count;
        path.Nodes.Clear();
        positionList.ForEach(position => path.Nodes.Add(position));
        path.enabled = true;
        Debug.Log("Path node count: " + path.NodeCount + ", right count: " + path.Nodes.Count);
    }

    private static int GetTrimNodeIndex(Vector3[] nodeList, Vector3 position)
    {
        var index = -1;
        var maxAngle = 0f;
        for (var i = 0; i < nodeList.Length - 2; ++i)
        {
            var left = nodeList[i];
            var right = nodeList[i + 1];
            var angle = Vector3.Angle(position - left, position - right);
            if (angle > maxAngle)
            {
                maxAngle = angle;
                index = i;
            }
        }
        return index;
    }

    public static List<Vector3> TrimPath(Vector3[] nodeList, Vector3 position, MoveDirection direction)
    {
        var index = GetTrimNodeIndex(nodeList, position);
        if (index == -1)
        {
            Debug.Log("Are you sure the position: " + position + " are part of path from node list?");
            return null;
        }

        var result = new List<Vector3>();
        if (direction == MoveDirection.Forward)
        {
            result.Add(position);
            for (var i = index + 1; i < nodeList.Length; ++i)
            {
                result.Add(nodeList[i]);
            }
        }
        else
        {
            for (var i = 0; i <= index; ++i)
            {
                result.Add(nodeList[i]);
            }
            result.Add(position);
        }
        return result;
    }

    public static List<Vector3> TrimPath(List<Vector3> nodeList, Vector3 position, MoveDirection direction)
    {
        return TrimPath(nodeList.ToArray(), position, direction);
    }

    public static List<Vector3> TrimPath(Vector3[] nodeList, Vector3 begin, Vector3 end)
    {
        var beginIndex = GetTrimNodeIndex(nodeList, begin);
        var endIndex = GetTrimNodeIndex(nodeList, end);
        if (beginIndex == -1 || endIndex == -1)
        {
            Debug.Log("Are you sure the position: " + begin + " and " + end + " are part of path from node list?");
            return null;
        }

        if (beginIndex == endIndex)
        {
            Debug.Log("Begin position and end position are too close, please check it out.");
            return null;
        }

        Debug.Log("Begin node index: " + beginIndex + ", end node index: " + endIndex);

        var result = new List<Vector3>();
        if (beginIndex < endIndex)
        {
            result.Add(begin);
            for (var i = beginIndex + 1; i <= endIndex; ++i)
            {
                result.Add(nodeList[i]);
            }
            result.Add(end);
        }
        else
        {
            result.Add(begin);
            for (var i = beginIndex; i > endIndex; --i)
            {
                result.Add(nodeList[i]);
            }
            result.Add(end);
        }
        return result;
    }

    public static List<Vector3> TrimPath(List<Vector3> nodeList, Vector3 begin, Vector3 end)
    {
        return TrimPath(nodeList.ToArray(), begin, end);
    }

    public static List<Vector3> TrimPath(Vector3[] nodeList, Vector3 position, MoveDirection direction, float distance)
    {
        var result = new List<Vector3>();

        var beginIndex = GetTrimNodeIndex(nodeList, position);
        beginIndex = (direction == MoveDirection.Forward) ? beginIndex + 1 : beginIndex;
        var startPosition = nodeList[beginIndex];
        var firstDistance = Mathf.Abs(Vector3.Distance(position, nodeList[beginIndex]));
        if (firstDistance < distance)
        {
            // edge case.
            if ((beginIndex == 0 && direction == MoveDirection.Backward) || (beginIndex == nodeList.Length - 1 && direction == MoveDirection.Forward))
            {
                result.Add(position);
                result.Add(startPosition);
            }
            else
            {
                var endIndex = (direction == MoveDirection.Forward) ? beginIndex + 1 : beginIndex - 1;
                var secondDistance = Math.Abs(Vector3.Distance(position, nodeList[endIndex]));
                if (secondDistance < distance)
                {
                    Debug.LogError(
                        "TrimPath fails. The node list distance between nodes are too tightly. Better to be done with greater or equal to ball's diameter.");
                    return null;
                }

                var finalPosition = (nodeList[beginIndex] + nodeList[endIndex]) / 2;
                var finalVecgtor = finalPosition - position;
                finalVecgtor.Normalize();
                finalVecgtor = finalVecgtor*distance;
                finalPosition = position + finalVecgtor;

                result.Add(position);
                result.Add(startPosition);
                result.Add(finalPosition);

                Debug.Log("Begin node index: " + beginIndex + ", end node index: " + endIndex);
            }
        }
        else
        {
            var finalPosition = (position + startPosition) / 2;
            var finalVecgtor = finalPosition - position;
            finalVecgtor.Normalize();
            finalVecgtor = finalVecgtor * distance;
            finalPosition = position + finalVecgtor;

            result.Add(position);
            result.Add(finalPosition);

            Debug.Log("The node index: " + beginIndex);
        }
        return result;
    }

    public static List<Vector3> TrimPath(List<Vector3> nodeList, Vector3 position, MoveDirection direction, float distance)
    {
        return TrimPath(nodeList.ToArray(), position, direction, distance);
    }

    #endregion
}
