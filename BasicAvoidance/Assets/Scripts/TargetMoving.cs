// using DvGames.Core;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class TargetMoving : MonoBehaviour
{
    [SerializeField]
    private AgentAvoidance agent = null; // need to know how to call agent

    [SerializeField]
    [Range(0.5f, 25.0f)]
    private float minSpeed = 5.0f; // determines how fast the target moves

    [SerializeField]
    [Range(5.0f, 150.0f)]
    private float maxSpeed = 150.0f; // determines how fast the target moves

    private float speed = 0;

    [SerializeField]
    private float maxDistance = -3.5f; // before we can reset the target

    private Vector3 originalPosition; // restore target to its original position after its done

    void Awake()
    {
        originalPosition = transform.localPosition;
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        // if we are beyond the max distance, restart the position of the target
        if(transform.localPosition.z <= maxDistance)
        {
            transform.localPosition = originalPosition;
        }
        else
        {
            // move towards max distance
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y,
                transform.localPosition.z - (Time.deltaTime * maxSpeed));
        }
    }

    public void ResetTarget()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        transform.localPosition = originalPosition;
        transform.localRotation = Quaternion.identity;
    }

    void OnCollisionEnter(Collision collision)
    {
        // if target is colliding with the player, take points away
        if(collision.transform.tag.ToLower() == "player")
        {
            Debug.Log("Points taken away");
            agent.TakeAwayPoints(); // call into the agent and use agent TakeAwayPoints
        } // if target collides with wall, give agent some points
        else if(collision.transform.tag.ToLower() == "wall")
        {
            Debug.Log("Points gained");
            agent.GivePoints();
        }
    }
}
