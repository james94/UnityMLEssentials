using TMPro;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class AgentAvoidance : BaseAgent
{
    // Start is called before the first frame update
    [SerializeField]
    private float speed = 50.0f;

    [SerializeField]
    private Vector3 idlePosition = Vector3.zero;

    [SerializeField]
    private Vector3 leftPosition = Vector3.zero;

    [SerializeField]
    private Vector3 rightPosition = Vector3.zero;

    [SerializeField]
    private TextMeshProUGUI rewardValue = null;

    [SerializeField]
    private TextMeshProUGUI episodeValue = null;

    [SerializeField]
    private TextMeshProUGUI stepValue = null;

    private TargetMoving targetMoving = null; // ref to target since need to reset it

    private float overallReward = 0;

    private float overallSteps = 0;

    // also keep track of where our agent moves to and whats the prev position since our agents are very sneaky
    // if we dont capture this info, what happens is the agent moves to the left and never comes back
        // similar for the rightside; we'll penalize agent if prev position and current pos are same, so it'll move
        // to new pos
    private Vector3 moveTo = Vector3.zero;

    private Vector3 prevPosition = Vector3.zero;

    private int punishCounter;

    void Awake()
    {
        targetMoving = transform.parent.GetComponentInChildren<TargetMoving>();
    }

    // placing agent in the middle when they start
    public override void OnEpisodeBegin()
    {
        transform.localPosition = idlePosition;
        moveTo = prevPosition = idlePosition;
    }

    // collect 6 different observations to give to unity's python ml agents backend
    public override void CollectObservations(VectorSensor sensor)
    {
        // 3 observations from agent's local position - x, y, z
        sensor.AddObservation(transform.localPosition);

        // 3 observations from target's local position - x, y, z 
        sensor.AddObservation(targetMoving.transform.localPosition);
    }

    // is how the agent knows what direction to take and where to go
    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        prevPosition = moveTo;
        int direction = vectorAction.DiscreteActions[0]; // ActionSegment<int>
        moveTo = idlePosition;

        // use switch statement to tell our agent what position, direction to take
        switch(direction)
        {
            case 0:
                moveTo = idlePosition;
                break;
            case 1:
                moveTo = leftPosition;
                break;
            case 2:
                moveTo = rightPosition;
                break;
        }

        // have agent move a little slower with deltatime multiplied to speed, so they can lean from the collisions
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, moveTo, Time.fixedDeltaTime * speed);

        // if the prev position and current position are the same, punish the agent; keeping track how many times they are same
        if(prevPosition == moveTo)
        {
            punishCounter++;
        }

        // if these positions are the same two or more times in a row, punish agent
            // penalize agent with neg reward and reset counter
        if(punishCounter > 3.0f)
        {
            AddReward(-0.01f);
            punishCounter = 0;
        }

    }

    // means the target is colliding with the agent, so take away points
    public void TakeAwayPoints()
    {
        AddReward(-0.01f); // penalize agent
        targetMoving.ResetTarget(); // reset pos and local rotation of target

        UpdateStats(); // since we know something happens, capture new reward, overall new steps, show on Canvas

        EndEpisode();
        StartCoroutine(SwapGroundMaterial(failureMaterial, 0.5f)); // update material with red cause failure
    }

    private void UpdateStats()
    {
        overallReward += this.GetCumulativeReward();
        overallSteps += this.StepCount;
        rewardValue.text = $"{overallReward.ToString("F2")}"; // show those updates from TakeAwayPoints on Canvas
        episodeValue.text = $"{this.CompletedEpisodes}";
        stepValue.text = $"{overallSteps}";
    }

    // in the case, the target collides with the wall, our agent is doing okay, so reward it
    public void GivePoints()
    {
        AddReward(1.0f); // reward agent, inc by 1
        targetMoving.ResetTarget(); // reset target cause we're done

        UpdateStats(); // show latest stats on canvas

        EndEpisode();
        StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f)); // update material with green cause success
    }

    // Overrode Heuristic, recommended to work with this if starting ML, so you know how your agent is going to move
    // If we go into Unity GUI, set use Only Heuristics, Unity ML Agents will say I dont want to calculate these from
    // the ML Agents tools, so I'm going to get the feedback from input. If I do down arrow, idle; left arrow, move left pos, etc.
    // Heuristic results will be passed into OnActionReceived(); Heuristics will help you understand how the agents are moving
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        // idle
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 0;
        }
        
        // move left
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 1;
        }

        // move right
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2;
        }
    }
}
