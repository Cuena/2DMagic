using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridManager : MonoBehaviour
{

    public GameObject terrain;
    public GameObject back;
    public GameObject spike;
    public GameObject flag;
    public GameObject bsky;



    public Sprite[] sprites;
    public float[,] Grid;
    int Vertical, Horizontal;



    // Start is called before the first frame update
    void Start()
    {

        print("grid");
        Grid = new float[10, 25]   {{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 0 },
                                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
                                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0 }};



        Vertical = Grid.GetLength(0); // (int) Camera.main.orthographicSize;
        Horizontal = Grid.GetLength(1); //Vertical * (int) Camera.main.aspect *2;
        //Cols = Horizontal*4;
        //Rows = Vertical*4;
        //Grid = new float[Cols, Rows];

        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
  


                    SpawnTile(j, -1*i, Grid[i, j]);
                

            }


        }
    }


    private Vector3 GridToWorldPosition(int x, int y )
    {
        return new Vector3(x - (Horizontal - 0.5f), y - (Vertical - 0.5f));
    }

    private void SpawnTile(int x, int y, float value){

        if (value < 1) { 
            SpriteRenderer sr = Instantiate(terrain, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();
            sr.name = "Terrain X: " + x + "Y:" + y;
            sr.sprite = sprites[(int)value];
        } else if (value == 1)
        {
            SpriteRenderer sr = Instantiate(back, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();
            sr.name = "Background X: " + x + "Y:" + y;
            sr.sprite = sprites[(int)value];

        }else if(value == 2)
        {
            SpriteRenderer sr = Instantiate(spike, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();
            SpriteRenderer srb = Instantiate(back, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();

            sr.name = "Spikes X: " + x + "Y:" + y;
            sr.sprite = sprites[(int)value];
            srb.sprite = sprites[1];

        } else if(value == 3)
        {
            SpriteRenderer sr = Instantiate(flag, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();
            SpriteRenderer srb = Instantiate(back, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();

            sr.name = "Spikes X: " + x + "Y:" + y;
            sr.sprite = sprites[(int)value];
            srb.sprite = sprites[1];
        } else
        {
            SpriteRenderer sr = Instantiate(bsky, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();
            sr.name = "Terrain X: " + x + "Y:" + y;
            sr.sprite = sprites[(int)value];
        }



    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
