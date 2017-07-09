using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle {
    public Vector3 position;
    public float distance; // Distance at which we can start avoiding
    public float weight; // How strong the avoidance is

    public Obstacle(Vector3 position, float distance = 10f, float weight = 0.3f)
    {
        this.position = position;
        this.distance = distance;
        this.weight = weight;
    }
}
