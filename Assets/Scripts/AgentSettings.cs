using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AgentSettings", order = 1)]
public class AgentSettings : ScriptableObject
{
    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    public float flagReward;
    public float stepReward;
    public float dieReward;
    public float deathByTimeoutReward;
    public float jumpReward;
    public int maxCurriculumMoveReward;
    public float moveRightReward;
    public float moveLeftReward;
    public float totalRightMultiReward;
    public float generatorMarioWinReward;
    public float generatorMarioLoseReward;
    public int timeRemaining;
    public float timeReward;
    public int curriculumStage;


    public Vector2 startingPos = new Vector2(0f, 0f);

  
    //private int maxX = 1;
    //private int maxY = 1;


}

