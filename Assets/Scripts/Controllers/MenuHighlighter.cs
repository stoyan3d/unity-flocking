using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuHighlighter : MonoBehaviour {

    public Button boidButton;
    public Button obstacleButton;
    public GameObject glow;

    private Animator glowAnimator;

	// Use this for initialization
	void Start () {
        glow.SetActive(false);

        glowAnimator = glow.GetComponent<Animator>();

        boidButton.onClick.AddListener(() => Fade());
        obstacleButton.onClick.AddListener(() => Fade());
    }
	
	// Update is called once per frame
	void Update () {
        switch (SimpleController.instance.buildMode)
        {
            case SimpleController.BuildMode.Boid:
                //glow.SetActive(false);
                glow.transform.position = boidButton.transform.position;
                glow.SetActive(true);
                break;
            case SimpleController.BuildMode.Obstacle:
                glow.transform.position = obstacleButton.transform.position;
                glow.SetActive(true);
                break;
        }
    }

    void Fade()
    {
        glowAnimator.SetTrigger("Fade");
    }
}
