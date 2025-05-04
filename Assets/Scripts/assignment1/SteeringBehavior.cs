// using UnityEngine;
// using System.Collections.Generic;
// using TMPro;

// public class SteeringBehavior : MonoBehaviour
// {
//     public Vector3 target;
//     public KinematicBehavior kinematic;
//     public List<Vector3> path;
//     // you can use this label to show debug information,
//     // like the distance to the (next) target
//     public TextMeshProUGUI label;
//     public float arriv_dist = 10;

    
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         kinematic = GetComponent<KinematicBehavior>();
//         target = transform.position;
//         path = null;
//         EventBus.OnSetMap += SetMap;
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // Assignment 1: If a single target was set, move to that target
//         //                If a path was set, follow that path ("tightly"
        
//         Vector3 subTarget = path != null ? path[0] : target;
//         if ((subTarget - transform.position).magnitude < 0.1 && path != null) return;
//         Vector3 dir = subTarget - transform.position;
//         float dist = (subTarget - transform.position).magnitude;
//         float arrival_distance = 25.0f;
//         float stop_car = path != null ? 20.0f : 1.0f;
//         float slow_down = 1;
//         if (dist < stop_car) {
//             if (path != null) {
//                 if (dist > 10) {
//                     slow_down = Mathf.Clamp((dist - 5) / 10, 0, 1);
//                 } else {
//                     path.RemoveAt(0);
//                     if (path.Count == 0) {
//                         path = null;
//                     }
//                 }
//             } else {
//                 kinematic.SetDesiredSpeed(0);
//                 return;
//             }
//         }
//         float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
//         if (path == null) {
//             slow_down = Mathf.Clamp(dist / arrival_distance, 0, 1);
//         }
//         UnityEngine.Debug.Log(slow_down);
//         kinematic.SetDesiredSpeed(kinematic.GetMaxSpeed() * slow_down);
//         kinematic.SetDesiredRotationalVelocity(angle * angle * Mathf.Sign(angle));
//     }
//     //}
    


//     //label.text = dist.ToString() + " " + angle.ToString();
//         //SECTION2:  kinematic.SetDesiredSpeed(kinematic.GetMaxSpeed());
//         //kinematic.SetDesiredRotationalVelocity(angle * angle * Mathf.Sign(angle));


//     public void SetTarget(Vector3 target)
//     {
//         this.target = target;
//         EventBus.ShowTarget(target);
//     }

//     public void SetPath(List<Vector3> path)
//     {
//         this.path = path;
//         if (path != null) this.target = path[path.Count - 1];
//     }

//     public void SetMap(List<Wall> outline)
//     {
//         this.path = null;
//         this.target = transform.position;
//     }
// }







using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    // you can use this label to show debug information,
    // like the distance to the (next) target
    public TextMeshProUGUI label;
    public float arriv_dist = 10;

    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;
    }

    void Update()
    {
        Vector3 subTarget = path != null ? path[0] : target;
        if ((subTarget - transform.position).magnitude < 0.1 && path != null) return;

        Vector3 dir = subTarget - transform.position;
        float dist = dir.magnitude;

        float stop_car = path != null ? 20f : 1f;
        float slow_down = 1f;

        if (dist < stop_car)
        {
            if (path != null)
            {
                if (dist > 10f)
                    slow_down = Mathf.Clamp((dist - 5f) / 10f, 0f, 1f);
                else
                {
                    path.RemoveAt(0);
                    if (path.Count == 0) path = null;
                }
            }
            else
            {
                kinematic.SetDesiredSpeed(0);
                return;
            }
        }

        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);

        if (path == null)
            slow_down = Mathf.Clamp(dist / arriv_dist, 0f, 1f);

        // —— Modified Speed & Rotation Logic —— //

        float angleAbs = Mathf.Abs(angle);
        // full speed until 30°, then linearly go to 0 at 90°
        float turnDropStart = 30f;
        float turnDropEnd   = 90f;
        float angleFactor = angleAbs <= turnDropStart
            ? 1f
            : Mathf.Clamp01(1f - ((angleAbs - turnDropStart) / (turnDropEnd - turnDropStart)));

        float desiredSpeed = kinematic.GetMaxSpeed() * slow_down * angleFactor;
        kinematic.SetDesiredSpeed(desiredSpeed);

        // 2. Rotation: simple proportional on degrees, clamped to a max turn rate
        float rotationGain = 2f;        // how hard we turn per degree of error
        float maxTurnRate  = 120f;      // max degrees per second
        float rawTurn      = angle * rotationGain;
        float desiredTurn  = Mathf.Clamp(rawTurn, -maxTurnRate, maxTurnRate);
        kinematic.SetDesiredRotationalVelocity(desiredTurn);

        // debug so you can tune these:
        // Debug.Log($"spd={desiredSpeed:F1}  turn={desiredTurn:F1}  slow_down={slow_down:F2}  angleAbs={angleAbs:F1}");
            
    }

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        EventBus.ShowTarget(target);
    }

    public void SetPath(List<Vector3> path)
    {
        this.path = path;
        if (path != null) this.target = path[path.Count - 1];
    }

    public void SetMap(List<Wall> outline)
    {
        this.path = null;
        this.target = transform.position;
    }
}
