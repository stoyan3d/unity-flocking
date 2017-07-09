using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController
{

    Obstacle playerObstacle;
    GameObject go;
    string controllerNum;

    // Use this for initialization
    public PlayerController(string controllerNum)
    {

        // Spawn a player in the middle of the map
        Vector3 spawnPosition = new Vector3(SimpleController.instance.width / 2, 0, SimpleController.instance.height / 2);
        playerObstacle = new Obstacle(spawnPosition);
        go = SimpleController.instance.AddObstacle(playerObstacle);
        go.name = "Character";

        this.controllerNum = controllerNum;
    }

    // Update is called once per frame
    public void Run()
    {
        float x = Input.GetAxis("Horizontal" + controllerNum);
        float y = Input.GetAxis("Vertical" + controllerNum);

        if (x != 0 || y != 0)
        {
            //Debug.Log("Controller " + controllerNum + " is being pressed");
            Vector3 translation = new Vector3(x, 0, y);
            go.transform.Translate(translation);
            playerObstacle.position = go.transform.position;
        }
    }
}
