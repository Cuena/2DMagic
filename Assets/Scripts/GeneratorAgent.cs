using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System;

public class GeneratorAgent : Agent
{

    private int[] rv;
    public GridManager gridManager;
    public MarioAgent marioAgent;

    private DecisionRequester dr;
    // Start is called before the first frame update
    public override void Initialize()
    {
        dr = marioAgent.GetComponent<DecisionRequester>();
        print("INICIALIZANDO GENERATOR AGENT");


    }

    public override void OnEpisodeBegin()
    {
        print("GENERATOR EPISODE BEGIN");
        dr.enabled = false;
        Reset();
        RequestDecision();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        print("COLLECTEANDO OBSERVATIONS");
        System.Random random = new System.Random();
        float[] values = new float[50];

        for (int i = 0; i < 50; ++i)
            values[i] = (float)random.Next();

        print(values);
        sensor.AddObservation(values);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        var discreteActions = actionBuffers.DiscreteActions;

        int[] values = new int[50];
        for (int i = 0; i < 50; ++i)
        {
            values[i] = discreteActions[i];
        }

        
        gridManager.generateBaseMap(50, values);

        dr.enabled = true;

    }


    public void Reset()
    {

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        print("HEURISTICA ====================");
        //int[] values = new int[50]; { 0, 0, 0, 0, 0 };

        var discreteActionsOut = actionsOut.DiscreteActions;

        for (int i = 0; i < 50; ++i)
        {
            discreteActionsOut[i] = 0;
        }

        discreteActionsOut[7] = 1;
        //gridManager.generateBaseMap(50, values);

        //dr.enabled = true;
    }




    // Update is called once per frame
    void Update()
    {

    }
}
