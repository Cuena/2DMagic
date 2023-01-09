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

    }

    public void insertVector(int row, int[] rv, int[] enemyLine)
    {

        for (int i = 0; i < 50; i++)
        {
            Grid[row, i] = 0;

        }

        for (int j = 0; j < rv.Length; j++)
        {
            int tile = rv[j];
            if (row == 9 && tile == 1)
            {
                // sustituir aire por fuego
                tile = 2;
            }
            Grid[row, j + 7] = tile;
        }

        if (enemyLine != null)
        {
            for (int j = 0; j < rv.Length; j++)
            {
                if (enemyLine[j] == 0)
                {
                    Grid[2, j + 7] = 1;
                }
                else
                {
                    Grid[2, j + 7] = 5;
                }

            }
        }

        Grid[9, 0] = 0;
        Grid[9, 1] = 0;
        Grid[9, 2] = 0;
        Grid[9, 3] = 0;
        Grid[9, 4] = 0;
        Grid[9, 5] = 0;
        Grid[9, 6] = 0;

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

    public int[,] generateBaseMap(int maxLength, int[] rv, int[] enemyLine)
    {
        var rnd = new Random();
        int w = 50;


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

        insertVector(9, rv, enemyLine);  // suelo
        insertVector(8, rv, null);  // base/bottom floor
        insertVector(7, rv, null);  // base/bottom floor
        Grid[8, w-2] = 3;
        Grid[9, w - 2] = 0;
        var c = 0;
        

        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                    SpawnTile(j, -1*i, Grid[i, j]);
            }
        }

        var full_ret = new int[4, 50];
        for (int r = 0; r < 4; r++)
        {
            for (int i = 0; i < 50; i++)
            {
                var k = 0;
                if (r == 0) k = 9;
                if (r == 1) k = 8;
                if (r == 2) k = 7;
                if (r == 3) k = 2;
                full_ret[r, i] = Grid[k, i];
            }
        }
        
        return full_ret;
    }


    public int[,] generateBaseMapMultiRow(int maxLength, int[,] generatedRows, int[] enemyLine)
    {
        var rnd = new Random();

        int w = 50;

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

        insertVector(9, ArrayUtils.GetRow(generatedRows, 0), enemyLine);  // base/bottom floor
        insertVector(8, ArrayUtils.GetRow(generatedRows, 1), null);
        insertVector(7, ArrayUtils.GetRow(generatedRows, 2), null);

        // var p = rnd.Next(8, 48);
        for (int i = 0; i < 7; i++)
        {
            Grid[8, i] = 1;
            Grid[9, i] = 1;
            if (i == 6)
            {
                Grid[9, i] = 0;
                Grid[8, i] = 0;
            }
        }

        Grid[6, w - 2] = 3;
        Grid[7, w - 2] = 0;
        Grid[8, w - 2] = 0;
        Grid[9, w - 2] = 0;

        Grid[7, w - 1] = 0;
        Grid[8, w - 1] = 0;
        Grid[9, w - 1] = 0;
        Grid[2, w - 1] = 0;
        var c = 0;
        for (int i = 0; i < w; i++)
        {
            if (Grid[8, i] == 3)
            {
                c++;
            }
        }

        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                SpawnTile(j, -1 * i, Grid[i, j]);
            }


        }

        var full_ret = new int[4, 50];
        for (int r = 0; r < 4; r++)
        {
            for (int i = 0; i < 50; i++)
            {
                var k = 0;
                if (r == 0) k = 9;
                if (r == 1) k = 8;
                if (r == 2) k = 7;
                if (r == 3) k = 2;
                full_ret[r, i] = Grid[k, i];
            }
        }

        return full_ret;
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
    void Update() { }
}
