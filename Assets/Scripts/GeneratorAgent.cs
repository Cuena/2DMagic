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

    private int[] rv;
    public GridManager gridManager;
    public MarioAgent marioAgent;

    private DecisionRequester dr;

    private int marioDecisionRequesterPeriod;
    private bool marioDecisionRequesterActionsBetweenDecisions;

    private int performedActionsInEpisode = 0;
    private int[] building;

    // Start is called before the first frame update
    public override void Initialize()
    {
        dr = marioAgent.GetComponent<DecisionRequester>();
        print("INICIALIZANDO GENERATOR AGENT");
        marioDecisionRequesterPeriod = dr.DecisionPeriod;
        marioDecisionRequesterActionsBetweenDecisions = dr.TakeActionsBetweenDecisions;
        building = new int[50-7];
        resetBuilding();
    }

    private void resetBuilding()
    {
        performedActionsInEpisode = 0;
        for (int i = 0; i < building.Length; i++)
        {
            building[i] = -1;
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

        float[] observations = new float[building.Length];

        for (int i = 0; i < building.Length; ++i) 
        {
            observations[i] = building[i] * 1f;
            //values[i] = (float)random.Next();
            //sensor.AddObservation(values[i]);
        }

        //print(values);
        sensor.AddObservation(observations);
    }

    private void BuildLevel()
    {
        for (int i = 0; i < building.Length; i++)
        {
            building[i] = building[i] * 2;
        }

        var ret = gridManager.generateBaseMap(50, building);


        Debug.Log("+++*** = " + String.Join("",
         new List<int>(ret)
         .ConvertAll(i => i.ToString())
         .ToArray()));

        // chequear las constraints 
        float penalty = CheckConstraints(ret);

        AddReward(penalty);

        if (penalty < 0.0f)
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

        int currentAction = discreteActions[0];

        building[performedActionsInEpisode] = currentAction;
        performedActionsInEpisode++;

        print(performedActionsInEpisode + " vamos construyendo "+ currentAction);
        if (performedActionsInEpisode >= 50-7)
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

    private float CheckConstraints(int[] values) 
    {
        // version 1D
        float penalty = 0.0f;

        // C1: 1-4 & -1 deben ser suelo
        //penalty += CheckConstraint1(values);

        // C2: que no haya x huecos consecutivos
        penalty += CheckConstraint2(values, 3);

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


    private float CheckConstraint2(int[] values, int max_consecutive_holes=3)
    {
        var pass = true;

        int current_consecutive_holes = 0;

        for (int i = 0; i < values.Length; ++i)
        {
            if (values[i] == 2)  // 2 porque es fuego
            {
                // si tenemos un hueco
                current_consecutive_holes += 1;
                if (current_consecutive_holes > max_consecutive_holes)
                {
                    pass = false;
                    break;
                }
            }
            else
            {
                // no hay agujero, asumimos que es suelo
                current_consecutive_holes = 0;
            }
        }

        if (pass)
        {
            return -1.0f;
        }
        return 1.0f;
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
            if (building[i] == 0) numAddedHoles++;
        }

        //int[] actions = new int[discreteActionsOut.Length];

        int consecutiveHoles = 0;
        for (int i = 0; i < maxHoleSize; i++)
        {
            if (performedActionsInEpisode - i - 1 >= 0 && building[performedActionsInEpisode - i - 1] == 1)
            {
                consecutiveHoles++;
            }
            else
            {
                break;
            }
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
}
