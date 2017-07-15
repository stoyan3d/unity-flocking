using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid {

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
    float r;
    float maxForce;
    float maxSpeed;


    public Boid(Vector3 position)
    {
        acceleration = Vector3.zero;

        velocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        this.position = position;
        position.y = 0;
        r = 2.0f;
        maxSpeed = 2;
        maxForce = 0.03f;
    }

    public void Run(List<Boid> boids, List<Obstacle> obstacles)
    {
        Flock(boids, obstacles);
        Update();
        Borders();
    }

    void ApplyForce(Vector3 force)
    {
        acceleration += force;
    }

    /// <summary>
    /// Accumulate the accelaration based on three rules
    /// </summary>
    /// <param name="boids"></param>
    void Flock(List<Boid> boids, List<Obstacle> obstacles)
    {
        /*
        // Separation
        Vector3 sep = Separate(boids); 
        sep *= SimpleController.instance.separationWeight;
        ApplyForce(sep);
        //Debug.DrawLine(position, position + sep * 100, Color.red);

        // Obstacle avoidance
        sep = AvoidObstacles(obstacles);
        ApplyForce(sep);

        // Alignment
        Vector3 ali = Align(boids);
        ali *= SimpleController.instance.alignmentWeight;
        ApplyForce(ali);
        //Debug.DrawLine(position, position + ali * 100, Color.blue);

        // Cohesion
        Vector3 coh = Cohesion(boids);
        coh *= SimpleController.instance.cohesionWeight;
        ApplyForce(coh);
        //Debug.DrawLine(position, position + coh * 100, Color.yellow);
        */

        // All forces
        Vector3 allForces = AllForces(boids);
        ApplyForce(allForces);

        // Obstacle avoidance
        Vector3 obs = AvoidObstacles(obstacles);
        ApplyForce(obs);
    }

    void Update()
    {
        velocity += acceleration;
        // Limit our speed
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        position += velocity * Time.deltaTime * SimpleController.instance.boidSpeed;

        // Reset the accelaration each update loop
        acceleration = Vector3.zero;
    }

    /// <summary>
    /// Change position for a wraparound effect
    /// </summary>
    void Borders()
    {
        if (position.x < -r)
            position.x = SimpleController.instance.width + r;
        if (position.z < -r)
            position.z = SimpleController.instance.height + r;
        if (position.x > SimpleController.instance.width + r)
            position.x = -r;
        if (position.z > SimpleController.instance.height + r)
            position.z = -r;
    }

    Vector3 Seek(Vector3 target)
    {
        // Apply a steering force towards target
        Vector3 desired = target - position; // Vector pointing from the position to the target

        // Scale to maximum speed
        desired.Normalize();
        desired *= maxSpeed;

        // Steering = Desired - Velocity
        Vector3 steer = desired - velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);
        return steer;
    }

    Vector3 Separate(List<Boid> boids)
    {
        // Steer away if too close to nearby boids
        // How far should boids separate
        Vector3 sum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < boids.Count; i++)
        {
            float dist = Vector3.Distance(position, boids[i].position);

            // Check if we are too close
            if (dist > 0 && dist < SimpleController.instance.separationDistance)
            {
                // Calculate vector pointing away from neighbour
                Vector3 diff = position - boids[i].position;
                diff.Normalize();
                diff = diff / dist; // Weight by distance
                sum += diff;
                count++;
            }
        }

        // Average
        if (count > 0)
        {
            sum = sum / count;
        }

        // As long as the vector is greater than 0
        if (sum.magnitude > 0)
        {
            sum.Normalize();
            sum *= maxSpeed;
            sum -= velocity;
            sum = Vector3.ClampMagnitude(sum, maxForce);
        }

        return sum;
    }

    Vector3 Align(List<Boid> boids)
    {
        // FIXME: There is a lot of duplicated code here so we can optimize this
        // Calculate the average velocity of nearby boids
        Vector3 sum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < boids.Count; i++)
        {
            float dist = Vector3.Distance(position, boids[i].position);

            if (dist > 0 && dist < SimpleController.instance.alignmentDistance)
            {
                sum += boids[i].velocity;
                count++;
            }
        }

        if (count > 0)
        {
            sum = sum / count;

            // Implement Reynolds: Steering = Desired - Velocity
            sum.Normalize();
            sum *= maxSpeed;
            sum -= velocity;
            sum = Vector3.ClampMagnitude(sum, maxForce);
            return sum;
        }
        else
        {
            return Vector3.zero;
        }
    }

    Vector3 Cohesion(List<Boid> boids)
    {
        // FIXME: There is a lot of duplicated code here so we can optimize this
        // Steer boids towards the average center
        Vector3 sum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < boids.Count; i++)
        {
            float dist = Vector3.Distance(position, boids[i].position);

            if (dist > 0 && dist < SimpleController.instance.cohesionDistance)
            {
                sum += boids[i].position;
                count++;
            }
        }

        if (count > 0)
        {
            sum = sum / count;

            return Seek(sum);
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Iterate through our obstacle list and avoid each one. Each obstacle has individual weights.
    /// </summary>
    /// <param name="obstacles"></param>
    /// <returns></returns>
    Vector3 AvoidObstacles(List<Obstacle> obstacles)
    {
        if (obstacles == null)
            return Vector3.zero;

        // Steer away if too close to nearby obstacles

        //float desiredSeparation = SimpleController.instance.obstacleDistance;
        Vector3 sum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < obstacles.Count; i++)
        {
            float dist = Vector3.Distance(position, obstacles[i].position);

            // Check if we are too close
            if (dist > 0 && dist < obstacles[i].distance)
            {
                // Calculate vector pointing away from neighbour
                Vector3 diff = position - obstacles[i].position;
                diff.Normalize();
                diff = diff / dist; // Weight by distance
                sum += diff * obstacles[i].weight;
                count++;
            }
        }

        // Average
        if (count > 0)
        {
            sum = sum / count;
        }

        return sum;
    }

    Vector3 AllForces(List<Boid> boids)
    {
        Vector3 sum = Vector3.zero;

        // Separation
        Vector3 sep = Vector3.zero;
        int sepCount = 0;
        Vector3 ali = Vector3.zero;
        int aliCount = 0;
        Vector3 cohSum = Vector3.zero;
        int cohCount = 0;

        for (int i = 0; i < boids.Count; i++)
        {
            float dist = Vector3.Distance(position, boids[i].position);

            // Check if we are too close
            if (dist > 0 && dist < SimpleController.instance.separationDistance)
            {
                // Calculate vector pointing away from neighbour
                Vector3 diff = position - boids[i].position;
                diff.Normalize();
                diff = diff / dist; // Weight by distance
                sep += diff;
                sepCount++;
            }

            if (dist > 0 && dist < SimpleController.instance.alignmentDistance)
            {
                ali += boids[i].velocity;
                aliCount++;
            }

            if (dist > 0 && dist < SimpleController.instance.cohesionDistance)
            {
                cohSum += boids[i].position;
                cohCount++;
            }
        }

        // Separation
        if (sepCount > 0)
        {
            sep = sep / sepCount;
        }

        if (sep.magnitude > 0)
        {
            sep.Normalize();
            sep *= maxSpeed;
            sep -= velocity;
            sep = Vector3.ClampMagnitude(sep, maxForce);
        }

        sum += sep * SimpleController.instance.separationWeight; ;

        // Alignment
        if (aliCount > 0)
        {
            ali = ali / aliCount;

            // Implement Reynolds: Steering = Desired - Velocity
            ali.Normalize();
            ali *= maxSpeed;
            ali -= velocity;
            ali = Vector3.ClampMagnitude(ali, maxForce);

            sum += ali * SimpleController.instance.alignmentWeight; ;
        }


        // Cohesion
        if (cohCount > 0)
        {
            cohSum = cohSum / cohCount;

            sum += Seek(cohSum) * SimpleController.instance.cohesionWeight;
        }

        return sum;
    }
}
