using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Random = System.Random;
using System.Linq;
using UnityEngine.XR;
using UnityEngine.Tilemaps;
using Unity.MLAgents;

public class GridManager : MonoBehaviour
{

    public GameObject terrain;
    public GameObject back;
    public GameObject spike;
    public GameObject flag;
    public GameObject bsky;
    public GameObject enemy;




    public Sprite[] sprites;
    public int[,] Grid;
    int Vertical, Horizontal;



    // Start is called before the first frame update
    void Start()
    {

        print("GridManager start");
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




    public void insertVector(int[] rv)
    {

        for (int i = 0; i < 50; i++)
        {
            Grid[9, i] = 0;

        }

        print("+++  SE HA LLAMADO");
        for (int j = 0; j < rv.Length; j++)
        {
            //if (rv[j] + 7 < 50)
            Grid[9, j + 7] = rv[j];
        }

        Grid[9, 0] = 0;
        Grid[9, 1] = 0;
        Grid[9, 2] = 0;
        Grid[9, 3] = 0;
        Grid[9, 4] = 0;
        Grid[9, 5] = 0;
        Grid[9, 6] = 0;


        //for (int i = 0; i < 7; i++)
        //{
        //    grid[9, i] = 0;
        //}

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

    public int[] generateBaseMap(int maxLength, int[] rv)
    {
        print("desde el generateBaseMap: "+String.Join(";;", rv));
        var rnd = new Random();
        print(String.Join(',', rv));

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
        insertVector(rv);
        // var p = rnd.Next(8, 48);
        Grid[8, w-2] = 5;
        Grid[9, w - 2] = 0;
        //Grid[9, w - 3] = 0;
        var c = 0;
        //for (int i = 0; i < w; i++)
        //{
            //print(Grid[8, i]);

         //   if (Grid[8, i] == 3)
         //   {
         //       c++;
         //   }
        //}
        print("JOder: "+c);

        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                    SpawnTile(j, -1*i, Grid[i, j]);
            }


        }

        var ret = new int[50];
        for (int i = 0; i < 50; i++)
        {
            ret[i] = Grid[9, i];
        }
        Debug.Log("+++ = " + String.Join("",
             new List<int>(ret)
             .ConvertAll(i => i.ToString())
             .ToArray()));
        return ret;
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
        } else if (value == 5)
        {
            SpriteRenderer sr = Instantiate(enemy, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();
            SpriteRenderer srb = Instantiate(back, GridToWorldPosition(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();

            sr.name = "Enemy X: " + x + "Y:" + y;
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
