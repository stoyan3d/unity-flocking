using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleController : MonoBehaviour
{

    public int flockSize = 20;
    public float boidSpeed = 10f;
    public GameObject boidPrefab;
    public GameObject obstaclePrefab;
    public float separationWeight = 2f;
    public float separationDistance = 10f;
    public float alignmentWeight = 1f;
    public float alignmentDistance = 25f;
    public float cohesionWeight = 1f;
    public float cohesionDistance = 25f;
    public float turnSpeed = 3f;

    public enum BuildMode { Boid, Obstacle }
    BuildMode buildMode;

    public static SimpleController instance;

    public float width = 160;
    public float height = 90;

    public List<Boid> boids { get; protected set; } // This is where we hold all the boids
    public List<Obstacle> obstacles { get; protected set; } // This is where we hold all the obstacles

    Dictionary<Boid, GameObject> boidGameObjectMap;
    Dictionary<Obstacle, GameObject> obstacleGameObjectMap;

    GameObject boidsGo;
    GameObject obstalcesGo;

    PlayerController char1;
    PlayerController char2;

    // Use this for initialization
    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;

        boids = new List<Boid>();
        obstacles = new List<Obstacle>();
        boidGameObjectMap = new Dictionary<Boid, GameObject>();
        obstacleGameObjectMap = new Dictionary<Obstacle, GameObject>();

        // Create parent objects for the boids and obstalces
        boidsGo = new GameObject("Boids");
        boidsGo.transform.SetParent(transform);
        obstalcesGo = new GameObject("Obstacles");
        obstalcesGo.transform.SetParent(transform);

        // Add boids
        for (int i = 0; i < flockSize; i++)
        {
            AddBoid(new Boid(Vector3.zero));
        }

        // Add walls
        //SetupTestWorld();

        //Add cahracters
        //char1 = new PlayerController("1");
        //char2 = new PlayerController("2");
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Boid b in boids)
        {
            b.Run(boids, obstacles); // Pass the list of boids to each boid

            // Update their position and orientation
            boidGameObjectMap[b].transform.position = b.position;

            boidGameObjectMap[b].transform.rotation = Quaternion.Slerp(boidGameObjectMap[b].transform.rotation,
            Quaternion.LookRotation(b.velocity),
            Time.deltaTime * turnSpeed);
        }

        if (obstacles != null)
        {
            foreach (Obstacle o in obstacles)
            {
                obstacleGameObjectMap[o].transform.position = o.position;
            }
        }

        // Set build mode using keys


        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            switch (buildMode)
            {
                case BuildMode.Boid:
                    AddBoid(new Boid(GetGroundPosFromScreen(Input.mousePosition)));
                    break;
                case BuildMode.Obstacle:
                    AddObstacle(new Obstacle(GetGroundPosFromScreen(Input.mousePosition)));
                    break;
                default:
                    break;
            }
        }

        //char1.Run();
        //char2.Run();
    }

    public void AddBoid(Boid b)
    {
        boidGameObjectMap[b] = Instantiate(boidPrefab);
        boidGameObjectMap[b].transform.SetParent(boidsGo.transform);
        boids.Add(b);
    }

    public GameObject AddObstacle(Obstacle o)
    {
        obstacleGameObjectMap[o] = Instantiate(obstaclePrefab);
        obstacleGameObjectMap[o].transform.SetParent(obstalcesGo.transform);
        obstacles.Add(o);

        return obstacleGameObjectMap[o];
    }

    public void SetBoidMode()
    {
        buildMode = BuildMode.Boid;
    }

    public void SetObstacleMode()
    {
        buildMode = BuildMode.Obstacle;
    }

    private Vector3 GetGroundPosFromScreen(Vector3 screenPos)
    {
        var ray = Camera.main.ScreenPointToRay(screenPos);
        float d;
        Plane groundPlane = new Plane(Vector3.up, 0);
        groundPlane.Raycast(ray, out d);
        return ray.GetPoint(d);
    }

    public void SetupTestWorld()
    {
        
        int cageWidth = 30;
        int cageHeight = 30;
        int openingSize = 8;

        for (int x = -cageWidth / 2; x <= cageWidth / 2; x++)
        {
            // Horizontal wall
            AddWall(new Vector3(width / 2 + x, 0, height / 2 - cageHeight / 2));

            // Vertical walls
            if (x == -cageWidth / 2 || x == cageWidth / 2)
            {
                for (int y = 0; y < cageHeight; y++)
                {
                    AddWall(new Vector3(width / 2 + x, 0, height / 2 + y - cageHeight / 2));
                }
            }

            // Door
            if (x < -openingSize / 2 || x > openingSize / 2)
                AddWall(new Vector3(width / 2 + x, 0, height / 2 + cageHeight - cageHeight / 2));

        }
    }

    void AddWall(Vector3 pos)
    {
        Obstacle o = new Obstacle(pos, 2, 5);
        AddObstacle(o);
    }
}
