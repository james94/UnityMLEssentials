using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class PlayerAgent : Agent
{
    #region Exposed Instance Variables
    [SerializeField] // TODO: Check what SerializeField means.
    private float speed = 10.0f;
    
    [SerializeField]
    // store original position and target; Want to know where the agent started, so we can random the x value
    private GameObject target;

    [SerializeField]
    // distance required from the agent to the target
    private float distanceRequired = 1.5f;

    [SerializeField]
    private MeshRenderer groundMeshRenderer;

    [SerializeField]
    private Material successMaterial;

    [SerializeField]
    private Material failureMaterial;

    [SerializeField]
    private Material defaultMaterial;
    #endregion

    #region Private Instance Variables
    private Rigidbody playerRigidbody;

    private Vector3 originalPosition;

    private Vector3 originalTargetPosition;
    #endregion

    // lifecycle of an agent

    public override void Initialize()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        originalPosition = transform.localPosition;
        originalTargetPosition = target.transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        transform.LookAt(target.transform);
        target.transform.localPosition = originalTargetPosition;
        transform.localPosition = originalPosition;
        transform.localPosition = new Vector3(originalPosition.x, originalPosition.y, Random.Range(-4, 4));
    }

    // tell python ML API about our Unity observations via sensor
    // Vector Observations Space Size in Unity UI = number of observations
    public override void CollectObservations(VectorSensor sensor)
    {
        // 3 observations   x, y, z
        sensor.AddObservation(transform.localPosition); // track agent's local pos

        // 3 observations   x, y, z
        sensor.AddObservation(target.transform.localPosition);

        // 1 observation
        sensor.AddObservation(playerRigidbody.velocity.x);

        // 1 observation
        sensor.AddObservation(playerRigidbody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        // Need to apply a force
        var vectorForce = new Vector3();
        vectorForce.x = vectorAction.ContinuousActions[0]; // horizontal
        vectorForce.z = vectorAction.ContinuousActions[1]; // vertical
        // do I need rotate?

        playerRigidbody.AddForce(vectorForce * speed);

        var distanceFromTarget = Vector3.Distance(transform.localPosition, target.transform.localPosition);

        // we are doing good
        if(distanceFromTarget <= distanceRequired)
        {
            SetReward(1.0f); // say good job to the agent for being close to the target
            EndEpisode();
            StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
        }

        // we are not doing so good
        if(transform.localPosition.y < 0)
        {
            EndEpisode();
            // go back and punish the agent for their performance

            StartCoroutine(SwapGroundMaterial(failureMaterial, 0.5f));
        }
    }

    // tell agent how to move on x axis and z axis
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal"); // x; horizontal
        continuousActions[1] = Input.GetAxis("Vertical"); // z; vertical
    }

    private IEnumerator SwapGroundMaterial(Material mat, float time)
    {
        groundMeshRenderer.material = mat;
        yield return new WaitForSeconds(time);
        groundMeshRenderer.material = defaultMaterial;
    }
}
