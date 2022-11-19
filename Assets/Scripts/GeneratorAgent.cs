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

    private int curriculum_stage = 3;

    // Start is called before the first frame update
    public override void Initialize()
    {
        dr = marioAgent.GetComponent<DecisionRequester>();
        print("INICIALIZANDO GENERATOR AGENT");
        marioDecisionRequesterPeriod = dr.DecisionPeriod;
        marioDecisionRequesterActionsBetweenDecisions = dr.TakeActionsBetweenDecisions;
    }

    public override void OnEpisodeBegin()
    {
        print("GENERATOR EPISODE BEGIN");
        curriculum_stage = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("mario_learning", 3.0f);
        freezeMario();
        Reset();
        RequestDecision();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        print("COLLECTEANDO OBSERVATIONS");
        System.Random random = new System.Random();
        float[] values = new float[10];

        for (int i = 0; i < 10; ++i) 
        { 
            values[i] = (float)random.Next();
            sensor.AddObservation(values[i]);
        }

        //print(values);
        //sensor.AddObservation(values);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        var discreteActions = actionBuffers.DiscreteActions;

        int numHoleIdx = discreteActions.Length;
        int[] values = new int[numHoleIdx];
        for (int i = 0; i < numHoleIdx; ++i)
        {
            values[i] = discreteActions[i];
        }


        var ret = gridManager.generateBaseMap(50, values);


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
            marioAgent.EndEpisode();
            EndEpisode();
            return;
        }

        print("=== SI QUE PASA LAS CONSTRAINTS");


        unfreezeMario();
    }

    private float CheckConstraints(int[] values) 
    {
        // version 1D
        float penalty = 0.0f;

        // C1: 1-4 & -1 deben ser suelo
        //penalty += CheckConstraint1(values);

        // C2: que no haya x huecos consecutivos
        //penalty += CheckConstraint2(values);

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
            if (values[i] == 1)
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
            return 0.0f;
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

        int maxNumHoles = 5;

        int maxHoleSize = curriculum_stage;

        int lastHoleStartIdx = -1;
        bool inHole = false;
        Random random = new Random();

        int numAddedHoles = 0;

        print("generating the new holes");
        for (int i = 7; i < 50 - 3; ++i)
        {
            if (maxHoleSize == 0) break;
            bool addHole = random.Next(0, 2) == 1;
            if (addHole && (!inHole || i - lastHoleStartIdx < maxHoleSize))
            {
                //discreteActionsOut[i] = 1;  // insert hole
                discreteActionsOut[numAddedHoles] = i;
                numAddedHoles++;
                if (!inHole)
                {
                    inHole = true;
                    lastHoleStartIdx = i;
                }
                print("adding a hole");
                continue;
            }

            inHole = false;
            if (numAddedHoles >= actionsOut.DiscreteActions.Length)
            {
                break;
            }
        }

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
