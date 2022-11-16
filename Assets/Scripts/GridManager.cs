using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Random = System.Random;
using System.Linq;
using UnityEngine.XR;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{

    public GameObject terrain;
    public GameObject back;
    public GameObject spike;
    public GameObject flag;
    public GameObject bsky;



    public Sprite[] sprites;
    public int[,] Grid;
    int Vertical, Horizontal;



    // Start is called before the first frame update
    void Start()
    {

        print("grid");
        Grid = new int[10, 25]   {{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                                    { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 0 },
                                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
                                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0 }};



        //Vertical = Grid.GetLength(0); // (int) Camera.main.orthographicSize;
        //Horizontal = Grid.GetLength(1); //Vertical * (int) Camera.main.aspect *2;
        //Cols = Horizontal*4;
        //Rows = Vertical*4;
        //Grid = new float[Cols, Rows];

        //Grid = generateBaseMap(50);
        
        
        //for (int i = 0; i < Grid.GetLength(0); i++)
        //{
        //    for (int j = 0; j < Grid.GetLength(1); j++)
        //    {
        //            SpawnTile(j, -1*i, Grid[i, j]);
        //    }


        //}

    }




    public void insertVector(int[,] grid, int[] rv)
    {
        for (int i = 0; i < grid.GetLength(1); i++)
        {
            grid[9, i] =  rv[i];
        }
    }





    public void insertRandomHoles(int[,] grid, int numHoles, int holeSize)
    {
        var rnd = new Random();


        var w = grid.GetLength(1);

        List<int> possible = Enumerable.Range(5, w - 10).ToList();

        List<int> listNumbers = new List<int>();
        for (int i = 0; i < numHoles; i++)
        {
            int index = rnd.Next(0, possible.Count);
            listNumbers.Add(possible[index]);
            possible.RemoveAt(index);
        }

        //print("Random holes in ");
        //print(string.Join(";", listNumbers));

        foreach (int j in listNumbers)
        {
            for (int x = 0; x < holeSize; x++)
            {
                var t = j;
                Grid[8, t] = 1;
                Grid[8, t + 1] = 1;
                Grid[9, t] = 1;
                Grid[9, t + 1] = 1;
                t = t+1;
            }

           
        }
    }

    public int[,] generateBaseMap(int maxLength, int[] rv)
    {
        var rnd = new Random();

        int w = rnd.Next(20, maxLength);

        w = 50;

        //w = Grid.GetLength(0);
        //p = Grid.GetLength(1);

        Grid = new int[10, w];

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < w; j++)
            {
                if (j == 0 || j == w - 1)
                {
                    Grid[i, j] = 0;
                }
                else if (i >= 10 - 2)
                {
                    Grid[i, j] = 1;
                }
                else
                {
                    Grid[i, j] = 1;
                }
            }
        }

        //insertRandomHoles(Grid, 3, 2);
        insertVector(Grid, rv);

        var p = rnd.Next(8, 48);
        Grid[8, w-2] = 3;
        var c = 0;
        for (int i = 0; i < w; i++)
        {
            //print(Grid[8, i]);

            if (Grid[8, i] == 3)
            {
                c++;
            }
        }
        print("JOder: "+c);

        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                    SpawnTile(j, -1*i, Grid[i, j]);
            }


        }

        return Grid;
    }
  


    public Vector2 GridToWorldPosition(int x, int y )
    {
        return new Vector2(x - (Horizontal - 0.5f), y - (Vertical - 0.5f));
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

            sr.name = "Win X: " + x + "Y:" + y;
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
