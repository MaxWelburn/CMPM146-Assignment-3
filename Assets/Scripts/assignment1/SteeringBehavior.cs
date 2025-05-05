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

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;
    }

    // Update is called once per frame
    void Update()
    {
        // Assignment 1: If a single target was set, move to that target
        //                If a path was set, follow that path ("tightly"
        
        Vector3 subTarget = path != null ? path[0] : target;
        if ((subTarget - transform.position).magnitude < 0.1 && path != null) return;
        Vector3 dir = subTarget - transform.position;
        float dist = (subTarget - transform.position).magnitude;
        float arrival_distance = 25.0f;
        float stop_car = path != null ? 20.0f : 1.0f;
        float slow_down = 1;
        if (dist < stop_car) {
            if (path != null) {
                if (dist > 10) {
                    slow_down = Mathf.Clamp((dist - 5) / 10, 0, 1);
                } else {
                    path.RemoveAt(0);
                    if (path.Count == 0) {
                        path = null;
                    }
                }
            } else {
                kinematic.SetDesiredSpeed(0);
                return;
            }
        }
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        if (path == null) {
            slow_down = Mathf.Clamp(dist / arrival_distance, 0, 1);
        }
        UnityEngine.Debug.Log(slow_down);
        kinematic.SetDesiredSpeed(kinematic.GetMaxSpeed() * slow_down);
        kinematic.SetDesiredRotationalVelocity(angle * angle * Mathf.Sign(angle));
    }
    //}
    


    //label.text = dist.ToString() + " " + angle.ToString();
        //SECTION2:  kinematic.SetDesiredSpeed(kinematic.GetMaxSpeed());
        //kinematic.SetDesiredRotationalVelocity(angle * angle * Mathf.Sign(angle));


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