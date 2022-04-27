﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {

    private static int BoardWidth = 48;
    private static int BoardHeight = 36;

    private bool didStartDeath = false;

    public int totalPellets = 0;
    public int score = 0;
    public int pacmanLives = 3;

    public AudioClip BackgroundAudioNormal;
    public AudioClip BackgroundAudioFrightened;
    public AudioClip BackgroundAudioPacManDeath;

    public GameObject[,] board = new GameObject[BoardWidth, BoardHeight];
	// Use this for initialization
	void Start () {
        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject o in objects)
        {
            Vector2 pos = o.transform.position;

            if(o.tag != "Ghost" && o.name !="Non Node" && o.name!="Pac-man" && o.name!= "Node" && o.name!="Pellets" && o.name !="Maze" && o.tag!="Maze" && o.tag!="Ghosthome")
            {
                if (o.GetComponent<Tile>() != null)
               {
                    if (o.GetComponent<Tile>().isPellet || o.GetComponent<Tile>().isSuperPellet)
                    {
                       totalPellets++;
                    }
                }
                board[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)] = o;
            }
        }
	}
	
    public void StartDeath()
    {
        if(!didStartDeath)
        {
            didStartDeath = true;
            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }
            GameObject pacMan = GameObject.Find("Pac-man");
            pacMan.transform.GetComponent<Pacman>().canMove = false;

            pacMan.transform.GetComponent<Animator>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            StartCoroutine(ProcessDeathAfter(2));
        }
    }

    IEnumerator ProcessDeathAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }
        StartCoroutine(ProcessDeathAnimation(1.55f));
    }

    IEnumerator ProcessDeathAnimation(float delay)
    {
        GameObject pacMan = GameObject.Find("Pac-man");
        pacMan.transform.localScale = new Vector3(1, 1, 1);
        pacMan.transform.localRotation = Quaternion.Euler(0, 0, 0);
        pacMan.transform.GetComponent<Animator>().runtimeAnimatorController = pacMan.transform.GetComponent<Pacman>().deathAnimation;
        pacMan.transform.GetComponent<Animator>().enabled = true;
        transform.GetComponent<AudioSource>().clip = BackgroundAudioPacManDeath;
        transform.GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(delay);

        StartCoroutine(ProcessRestart(2));
    }
    IEnumerator ProcessRestart(float delay)
    {
        GameObject pacMan = GameObject.Find("Pac-man");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        transform.GetComponent<AudioSource>().Stop();
        yield return new WaitForSeconds(delay);
        Restart();
    }

    public void Restart()
    {
        pacmanLives -= 1;
        GameObject pacMan = GameObject.Find("Pac-man");
        pacMan.transform.GetComponent<Pacman>().Restart();
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        
        foreach(GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().Restart();
        }
        transform.GetComponent<AudioSource>().clip = BackgroundAudioNormal;
        transform.GetComponent<AudioSource>().Play();
        didStartDeath = false;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
