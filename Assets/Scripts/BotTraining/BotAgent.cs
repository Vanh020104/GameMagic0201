using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class BotAgent : Agent
{
    public Transform target;
    private Rigidbody rb;

    public float moveSpeed = 2f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset vị trí bot và target mỗi lần học xong 1 vòng
        rb.velocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
        target.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Quan sát vị trí của bot và target
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("Received Action!");
         Debug.Log("Agent got action!");
        // Di chuyển bot theo action từ neural net
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        rb.AddForce(move * moveSpeed);

        // Tính khoảng cách tới target
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);

        // Thưởng nếu tới gần
        if (distanceToTarget < 1.5f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Phạt nếu đi quá xa
        if (transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        Debug.Log($"Move input: {actions.ContinuousActions[0]}, {actions.ContinuousActions[1]}");

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Cho phép điều khiển bằng phím tay khi cần test
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
