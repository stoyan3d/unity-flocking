using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SimpleController : MonoBehaviour
{
    public StartingObstacle[] startingObstacles;
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

    public AudioClip clickSound;
    public AudioClip placeBoidSound;
    public AudioClip placeObstacleSound;
    private AudioSource source;

    public GameObject placeVFX;
    public GameObject placeObstacleVFX;

    public enum BuildMode { Boid, Obstacle }
    public BuildMode buildMode { get; protected set; }

    public static SimpleController instance;

    public float width = 160;
    public float height = 90;

    public List<Boid> boids { get; protected set; } // This is where we hold all the boids
    public List<Obstacle> obstacles { get; protected set; } // This is where we hold all the obstacles

    Dictionary<Boid, GameObject> boidGameObjectMap;
    Dictionary<Obstacle, GameObject> obstacleGameObjectMap;

    GameObject boidsGo;
    GameObject obstalcesGo;
    GameObject vfxGo;

    bool initialized = false;

    // Use this for initialization
    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;

        // Initialize our variables
        boids = new List<Boid>();
        obstacles = new List<Obstacle>();
        boidGameObjectMap = new Dictionary<Boid, GameObject>();
        obstacleGameObjectMap = new Dictionary<Obstacle, GameObject>();
        source = GetComponent<AudioSource>();

        // Create parent objects for the boids and obstalces
        boidsGo = new GameObject("Boids");
        boidsGo.transform.SetParent(transform);
        obstalcesGo = new GameObject("Obstacles");
        obstalcesGo.transform.SetParent(transform);
        vfxGo = new GameObject("VFX");
        vfxGo.transform.SetParent(transform);

        // Add boids
        for (int i = 0; i < flockSize; i++)
        {
            AddBoid(new Boid(Vector3.zero));
        }

        // Populate the map with some initial obstacles
        if (startingObstacles != null)
        {
            foreach (StartingObstacle obs in startingObstacles)
            {
                AddObstacle(new Obstacle(new Vector3(obs.x, 0, obs.y)));
            }
        }

        initialized = true;
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

        // Set build mode
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
    }

    public void AddBoid(Boid b)
    {
        if (initialized)
        {
            source.clip = placeBoidSound;
            source.Play();

            GameObject go;
            go = SimplePool.Spawn(placeVFX, b.position, placeVFX.transform.rotation);
            go.transform.SetParent(vfxGo.transform);
        }

        boidGameObjectMap[b] = Instantiate(boidPrefab);
        boidGameObjectMap[b].transform.SetParent(boidsGo.transform);
        boids.Add(b);
    }

    public void AddObstacle(Obstacle o)
    {
        if (initialized)
        {
            source.clip = placeObstacleSound;
            source.Play();

            GameObject go;
            go = SimplePool.Spawn(placeObstacleVFX, o.position, placeObstacleVFX.transform.rotation);
            go.transform.SetParent(vfxGo.transform);
        }

        obstacleGameObjectMap[o] = Instantiate(obstaclePrefab);
        obstacleGameObjectMap[o].transform.SetParent(obstalcesGo.transform);
        obstacles.Add(o);
    }

    public void SetBoidMode()
    {
        source.clip = clickSound;
        source.Play();

        buildMode = BuildMode.Boid;
    }

    public void SetObstacleMode()
    {
        source.clip = clickSound;
        source.Play();

        buildMode = BuildMode.Obstacle;
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }

    private Vector3 GetGroundPosFromScreen(Vector3 screenPos)
    {
        var ray = Camera.main.ScreenPointToRay(screenPos);
        float d;
        Plane groundPlane = new Plane(Vector3.up, 0);
        groundPlane.Raycast(ray, out d);
        return ray.GetPoint(d);
    }
}

[System.Serializable]
public struct StartingObstacle
{
    public float x;
    public float y;
}
