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
        performedActionsInEpisode = 0;
        freezeMario();
        Reset();

        resetBuilding();

        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float[] observations = new float[NUM_ROWS * ROW_SIZE];

        for (int row = 0; row < NUM_ROWS; row++)
        {
            for (int i = 0; i < ROW_SIZE; ++i)
            {
                observations[row * ROW_SIZE + i] = building[row, i] * 1f;
            }
        }

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
                building[row, i] = building[row, i] * 1;
            }
        }

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

        // chequear las constraints 
        float penalty = CheckConstraints(ret, enemy_ret);

        AddReward(penalty);

        if (!current_ep_constraints_passed)
        {
            marioAgent.Reset();
            return;
        }

        marioAgent.floor = ret;
        unfreezeMario();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        var discreteActions = actionBuffers.DiscreteActions;

        int currentActionFloorBottom= discreteActions[0];  // suelo
        int currentActionFloorMedium = discreteActions[1];  // suelo
        int currentActionFloorTop = discreteActions[2];  // suelo
        int currentActionEnemy = discreteActions[3];
        int currentColumn = performedActionsInEpisode; //GetCurrentColumn(performedActionsInEpisode - 1);

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

        if (performedActionsInEpisode >= ROW_SIZE)
        {
            BuildLevel();
        }
        else
        {
            RequestDecision();
        }
    }

    private float CheckConstraints(int[] values, int[] enemies)
    {
        // version 1D
        float penalty;

        // C1: 1-4 & -1 deben ser suelo
        //penalty += CheckConstraint1(values);

        // C2: que no haya x huecos consecutivos
        current_ep_constraints_passed = true;
        var penalty2 = CheckConstraint2(values, 3);
        if (penalty2 > 0.0f) current_ep_constraints_passed = false;
        
        // C3: maximo numero de enemigos
        var penalty3 = CheckConstraints3(enemies, 5);
        if (penalty3 > 0.0f) current_ep_constraints_passed = false;

        penalty = penalty2 + penalty3;


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
        return 1.0f;
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

    public void Reset() { }

    public override void Heuristic(in ActionBuffers actionsOut)
    {

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


        Random random = new Random();

        int numAddedHoles = 0;

        float per_block_prob = maxNumHoles / 43f;


        int consecutiveHoles = 0;

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
    void Update() {}

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
