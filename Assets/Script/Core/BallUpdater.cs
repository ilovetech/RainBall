using System.Collections.Generic;
using UnityEngine;

public enum MoveDirection
{
    Forward,
    Backward
}

public class BallUpdater : MonoBehaviour
{
    public GameObject NextBall;

    public GameObject LastBall;

    public float IntersectFactor { get; set; }

    public float DistanceFactor { get; set; }

    public float DiaMeter { get; set; }

    public float Theta { get; set; }

    public int Index { get; set; }

    public int Color { get; set; }

    public string Name { get { return string.Format("ball_{0}_color_{1}", Index + 1, Color); } }

    public MoveDirection MoveDirection { get; set; }

    public LinkedList<Vector3> TrackingTail { get; set; }

    public Vector3 Direction { get; set; }

    public bool Running { get; set; }

    public void UpdateBrotherBall()
    {
        var brotherBall = (MoveDirection == MoveDirection.Forward) ? NextBall : LastBall;

        if(TrackingTail.Count == 0)
        {
            TrackingTail.AddLast(transform.position);
            return;
        }

        var begin = TrackingTail.First.Value;
        var end = transform.position;
        var distance = Vector3.Distance(begin, end);

        if(distance <= Theta)
        {
            //Debug.Log("name: " + name + "Too small update, pass by this vector.");
            return;
        }

        TrackingTail.AddLast(end);

        Direction = end - begin;
        Direction.Normalize();
        Direction.Scale(new Vector3(DiaMeter, 1, DiaMeter));

        DrawDirection();

        //Debug.Log("name: " + name + ", distance: " + distance + ", begin: " + begin + ", end: " + end + ", tracking list count: " + TrackingTail.Count);

        var farAway = (distance >= DiaMeter / 2);
        var lessBegin = TrackingTail.First.Value;
        while (distance >= DiaMeter)
        {
            farAway = true;

            begin = TrackingTail.First.Value;

            TrackingTail.RemoveFirst();
            
            //Debug.Log("name: " + name + ", dequeue point: " + begin);

            lessBegin = TrackingTail.First.Value;

            distance = Vector3.Distance(lessBegin, end);
        }

        //Debug.Log("name: " + name + ", distance adjust: " + distance + ", first: " + TrackingTail.First.Value + ", last: " +
        //          TrackingTail.Last.Value + ", tracking list count: " + TrackingTail.Count);

        // [NOTE] Update brother ball only if we are FAR AWAY enough, (head to end distance greater than half diameter).
        // Case study: Only left one position in hands.
        // All positions are far away to current position.
        // This happens when shooter hit the ball list, the header balls are running fast.
        // begin position: the one that neareast to one diameter of current ball, but greater than.
        // lessBegin position: the one that neareast to one diameter of current ball, but smaller than.
        if (farAway)
        {
            begin = new Vector3((lessBegin.x * IntersectFactor + begin.x * (1 - IntersectFactor)), (lessBegin.y * IntersectFactor + begin.y * (1 - IntersectFactor)),
                                (lessBegin.z * IntersectFactor + begin.z * (1 - IntersectFactor)));
            //begin = Vector3.Slerp(lessBegin, begin, IntersectFactor);

            //Debug.Log("begin position: " + begin);

            var final = (begin - end);
            final.Normalize();
            final.Scale(new Vector3(DiaMeter * DistanceFactor, 1, DiaMeter * DistanceFactor));
            
            if(brotherBall != null)
            {
                brotherBall.transform.position = final + end;
            }

            //Debug.Log("Name: " + name + "Current ball position: " + transform.position + ", Brother ball position: " + brotherBall.transform.position + ", distance: " + Vector3.Distance(brotherBall.transform.position, end) + ", distance vector: " + final.magnitude);
        }
    }

    private void DrawDirection()
    {
        var beginP = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z);
        var endP = beginP + Direction;
        Debug.DrawLine(beginP, endP, UnityEngine.Color.yellow);
    }

    public void Set(BallUpdater other)
    {
        Set(other.DiaMeter, other.DistanceFactor, other.IntersectFactor, other.Theta);
    }

    public void Set(float diaMeter, float distanceFactor, float intersectFactor, float theta)
    {
        DiaMeter = diaMeter;
        DistanceFactor = distanceFactor;
        IntersectFactor = intersectFactor;
        Theta = theta;
    }

    public bool IsInSegment(Vector3 begin, Vector3 end)
    {
        return Utils.IsInSegement(begin, end, transform.position) && Utils.IsParallel(begin, end, Direction);
    }

    void OnMouseDown()
    {
        Debug.Log(string.Format("Dragon ball {0}, I am touching you.", name));
    }

    void Awake()
    {
        TrackingTail = new LinkedList<Vector3>();
    }
}
