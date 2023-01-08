using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System;
using Random = System.Random;

using System.Threading;
using Unity.VisualScripting;

public class GeneratorAgent : Agent
{

    private static readonly int NUM_ROWS = 3;
    private static readonly int ROW_SIZE = 50 - 7;

    private int[] rv;
    public GridManager gridManager;
    public MarioAgent marioAgent;

    private DecisionRequester dr;

    private int marioDecisionRequesterPeriod;
    private bool marioDecisionRequesterActionsBetweenDecisions;

    private int performedActionsInEpisode = 0;
    private int[,] building;

    private int[] enemyLine;
    private int tot_enemy;
    private int tot_fire;
    private int tot_fails;

    private bool current_ep_constraints_passed;

    // Start is called before the first frame update
    public override void Initialize()
    {
        dr = marioAgent.GetComponent<DecisionRequester>();
        print("INICIALIZANDO GENERATOR AGENT");
        marioDecisionRequesterPeriod = dr.DecisionPeriod;
        marioDecisionRequesterActionsBetweenDecisions = dr.TakeActionsBetweenDecisions;
        building = new int[NUM_ROWS, ROW_SIZE];  // generamos tres filas de bloques
        enemyLine = new int[50 - 7];

        resetBuilding();
    }

    private void resetBuilding()
    {
        performedActionsInEpisode = 0;
        for (int row = 0; row < NUM_ROWS; row++)
        {
            for (int i = 0; i < ROW_SIZE; i++)
            {
                building[row, i] = -1;
                enemyLine[i] = -1;
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        print("GENERATOR EPISODE BEGIN");
        performedActionsInEpisode = 0;
        freezeMario();
        Reset();

        resetBuilding();

        //for (int i = 7; i < 50; i++)
        //{
        //    print("estamos en el bucle " + i);
        //    RequestDecision();
        //    //Academy.Instance.EnvironmentStep();
        //}

        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        print("COLLECTEANDO OBSERVATIONS");
        System.Random random = new System.Random();
        //float[] values = new float[];

        //float[] observations = new float[building.Length];
        float[] observations = new float[NUM_ROWS * ROW_SIZE];

        for (int row = 0; row < NUM_ROWS; row++)
        {
            for (int i = 0; i < ROW_SIZE; ++i)
            {
                observations[row * ROW_SIZE + i] = building[row, i] * 1f;
            }
        }

        //print(values);
        sensor.AddObservation(observations);
        sensor.AddObservation(GetCurrentRow());
        sensor.AddObservation(GetCurrentColumn());
    }

    private void BuildLevel()
    {
        for (int row = 0; row < building.GetLength(0); row++)
        {
            for (int i = 0; i < building.GetLength(1); i++)
            {
                //building[row, i] = building[row, i] * 2;  // fix porque el agujero es fuego, no aire
                building[row, i] = building[row, i] * 1;
            }
        }

        print("amiguito row 2\t" + string.Join("\t", ArrayUtils.GetRow(building, 2)));
        print("amiguito row 1\t" + string.Join("\t", ArrayUtils.GetRow(building, 1)));
        print("amiguito row 0\t" + string.Join("\t", ArrayUtils.GetRow(building, 0)));

        var full_ret = gridManager.generateBaseMapMultiRow(50, building, enemyLine);

        var ret = new int[50];
        var enemy_ret = new int[50];

        for (int i = 0; i < full_ret.GetLength(0); i++)
        {
            for (int j = 0; j < full_ret.GetLength(1); j++)
            {
                if (i == 0)
                {
                    ret[j] = full_ret[i, j];
                }
                else
                {
                    enemy_ret[j] = full_ret[i, j];
                }
            }
        }

        Debug.Log("+++*** = " + String.Join("",
         new List<int>(ret)
         .ConvertAll(i => i.ToString())
         .ToArray()));

        Debug.Log("+++*** = " + String.Join("",
         new List<int>(enemy_ret)
         .ConvertAll(i => i.ToString())
         .ToArray()));

        // chequear las constraints 
        float penalty = CheckConstraints(ret, enemy_ret);

        AddReward(penalty);

        print("(((" +penalty);

        if (!current_ep_constraints_passed)
        {
            print("=== NO PASA LAS CONSTRAINTS");
            marioAgent.Reset();
            return;
        }

        print("=== SI QUE PASA LAS CONSTRAINTS");

        marioAgent.floor = ret;
        unfreezeMario();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        var discreteActions = actionBuffers.DiscreteActions;

        //int numHoleIdx = discreteActions.Length;
        //int[] values = new int[numHoleIdx];
        //for (int i = 0; i < numHoleIdx; ++i)
        //{
        //    values[i] = discreteActions[i];
        //}

        int currentActionFloorBottom= discreteActions[0];  // suelo
        int currentActionFloorMedium = discreteActions[1];  // suelo
        int currentActionFloorTop = discreteActions[2];  // suelo
        int currentActionEnemy = discreteActions[3];
        //int currentRow = GetCurrentRow(performedActionsInEpisode - 1);
        int currentColumn = performedActionsInEpisode; //GetCurrentColumn(performedActionsInEpisode - 1);

        //print("$$current row is: " + currentRow);
        //print("$$current column is: " + currentColumn + "; action is " + currentActionFloorBottom);

        building[0, currentColumn] = currentActionFloorBottom;
        building[1, currentColumn] = currentActionFloorMedium;
        building[2, currentColumn] = currentActionFloorTop;
        
        if (building[0, currentColumn] == 1)
        {
            building[1, currentColumn] = 1;
            building[2, currentColumn] = 1;
        } 
        else if (building[1, currentColumn] == 1)
        {
            building[2, currentColumn] = 1;
        }

        if (building[2, currentColumn] == 0)
        {
            building[0, currentColumn] = 1;
            building[1, currentColumn] = 1;
        }

        enemyLine[performedActionsInEpisode] = currentActionEnemy;

        performedActionsInEpisode++;

        //print(performedActionsInEpisode + " vamos construyendo " + currentAction);
        if (performedActionsInEpisode >= ROW_SIZE)
        {
            print("se va a construir");
            BuildLevel();
        }
        else
        {
            print("se esta buildeando " + performedActionsInEpisode);
            RequestDecision();
        }
    }

    private float CheckConstraints(int[] values, int[] enemies)
    {
        // version 1D
        float penalty = 0.0f;

        // C1: 1-4 & -1 deben ser suelo
        //penalty += CheckConstraint1(values);

        // C2: que no haya x huecos consecutivos
        current_ep_constraints_passed = true;
        var penalty2 = CheckConstraint2(values, 3);
        if (penalty2 > 0.0f) current_ep_constraints_passed = false;
        
        var penalty3 = CheckConstraints3(enemies, 5);
        if (penalty3 > 0.0f) current_ep_constraints_passed = false;

        //var penalty4 = CheckConstraint4();
        //if (penalty4 > 0.0f) current_ep_constraints_passed = false;

        print("===" + penalty);
        penalty = penalty2 + penalty3;

        print(tot_enemy + " mierdas");

        return -penalty * 100;
    }

    private float CheckConstraint1(int[] values)
    {
        int[] n = new int[] { 0, 1, 2, 3, 50 - 1 };
        var pass = true;
        for (int i = 0; i < n.Length; ++i)
        {
            if (values[n[i]] != 0)  // si esta tile no es suelo, esta mal
            {
                pass = false;
                break;
            }
        }

        if (pass)
        {
            return 0.0f;
        }
        return 1.0f;  // TODO este numero igual hay que fine tunear
    }


    private float CheckConstraints3(int[] enemies, int max_enemies)
    {
        int c = 0;
        bool pass = true;
        for (int i = 0; i < enemies.GetLength(0); ++i)
        {
            if (enemies[i] == 5)
            {
                c += 1;

            }
        }
        if (c > max_enemies)
        {
            pass = false;
            
        }
        tot_enemy = c;
        if (pass)
        {
            

            return -1.0f * (tot_enemy + 1);
        } else
        {
            print("=== A");
            return 1.0f * (tot_enemy + 1);
        }
    }

    private float CheckConstraint2(int[] values, int max_consecutive_holes=3)
    {
        var pass = true;
        tot_fails = 0;

        int holecount = 0;

        int current_consecutive_holes = 0;
        var w = 0;
        for (int i = 0; i < values.Length; ++i)
        {
            if (values[i] == 2)  // 2 porque es fuego
            {
                holecount++;
                // si tenemos un hueco
                current_consecutive_holes += 1;
                if (current_consecutive_holes > max_consecutive_holes)
                {
                    if (pass) pass = false;
                    tot_fails++;
                }
            }
            else
            {
                // no hay agujero, asumimos que es suelo
                current_consecutive_holes = 0;
            }
        }


        for (int i = 0; i < values.GetLength(0); ++i)
        {
            if (values[i] == 2)
            {
                w += 1;
            }
        }

        tot_fire = w;

        if (pass)
        {
            return -1.0f * (1+holecount);
        }
        return 1.0f * tot_fails;
    }

    private float CheckConstraint4()
    {
        bool pass = true;

        int mistakes = 0;

        for (int r = building.GetLength(0) - 1; r > 0; r--)
        {
            for (int c = 0; c < building.GetLength(1); c++)
            {
                if (building[r, c] == 0)
                {
                    // hay suelo
                    if (building[r - 1, c] != 0)
                    {
                        // si abajo hay algo que no es suelo (aire, fuego)
                        pass = false;
                        mistakes += 1;
                    }
                }
            }
        }

        pass = mistakes == 0;
        Academy.Instance.StatsRecorder.Add("C4_mistakes", mistakes);
        if (pass)
        {
            return -1.0f;
        }
        return 1.0f * mistakes;
    }

    public void Reset() { }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        print("HEURISTICA ====================");
        //int[] values = new int[50]; { 0, 0, 0, 0, 0 };

        var discreteActionsOut = actionsOut.DiscreteActions;

        for (int i = 0; i < discreteActionsOut.Length; ++i)
        {
            discreteActionsOut[i] = 0;
        }

        int curriculum_stage = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("stage", 4f);


        int maxNumHoles = 0;
        int maxHoleSize = 0;

        if (curriculum_stage == 0)
        {
            maxNumHoles = 0;
            maxHoleSize = 0;
        }

        if (curriculum_stage == 1)
        {
            maxNumHoles = 2;
            maxHoleSize = 1;
        }

        if (curriculum_stage == 2)
        {
            maxNumHoles = 5;
            maxHoleSize = 2;
        }

        if (curriculum_stage == 3)
        {
            maxNumHoles = 10;
            maxHoleSize = 2;
        }

        if (curriculum_stage >= 4)
        {
            maxNumHoles = 20;
            maxHoleSize = 3;
        }

        //int maxHoleSize = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("max_hole_sizes", 3.0f);


        Random random = new Random();

        int numAddedHoles = 0;

        float per_block_prob = maxNumHoles / 43f;
    
        for (int i = 0; i < performedActionsInEpisode; i++)
        {
            //if (building[i] == 0) numAddedHoles++;
        }

        //int[] actions = new int[discreteActionsOut.Length];

        int consecutiveHoles = 0;
        for (int i = 0; i < maxHoleSize; i++)
        {
            //if (performedActionsInEpisode - i - 1 >= 0 && building[performedActionsInEpisode - i - 1] == 1)
            //{
            //    consecutiveHoles++;
            //}
            //else
            //{
            //    break;
            //}
        }

        if (numAddedHoles >= maxNumHoles || maxHoleSize == 0 || consecutiveHoles >= maxHoleSize)
        {
            discreteActionsOut[0] = 0;  // place background
            return;
        }

        bool addHole = random.NextDouble() < per_block_prob;
    
        if (addHole)
        {
            discreteActionsOut[0] = 1;  // place hole
            return;
        }

        int[] actions = new int[discreteActionsOut.Length];
        for (int i = 0; i < discreteActionsOut.Length; i++)
        {
            actions[i] = discreteActionsOut[i];
        }
        print(string.Join(";", actions));

        //discreteActionsOut[7] = 1;
        //gridManager.generateBaseMap(50, values);

        //dr.enabled = true;
    }

    private void freezeMario()
    {
        // stop mario from requesting decisions
        dr = marioAgent.GetComponent<DecisionRequester>();
        Destroy(dr);
    }

    private void unfreezeMario()
    {
        // make mario go back to request decisions
        dr = marioAgent.AddComponent<DecisionRequester>();
        dr.DecisionPeriod = marioDecisionRequesterPeriod;
        dr.TakeActionsBetweenDecisions = marioDecisionRequesterActionsBetweenDecisions;
    }


    // Update is called once per frame
    void Update()
    {

    }

    private int GetCurrentRow()
    {
        return (int)((performedActionsInEpisode + 1) / ROW_SIZE);
    }

    private int GetCurrentRow(int perfActions)
    {
        return (int)((perfActions + 1) / ROW_SIZE);
    }

    private int GetCurrentColumn()
    {
        return (int)((performedActionsInEpisode + 1) % ROW_SIZE);
    }

    private int GetCurrentColumn(int perfActions)
    {
        return (int)((perfActions + 1) % ROW_SIZE);
    }
}
