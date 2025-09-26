using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class AgentMoveToTarget : Agent
{
    [SerializeField] private int targetsHit = 0;
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;
    [SerializeField] Renderer headbandRenderer;
    MaterialPropertyBlock mpb;

    [SerializeField] private float maxTime = 10f; // tiempo máximo por episodio
    private float timer;

    void SetHeadbandColor(Color c)
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();
        headbandRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", c); 
        headbandRenderer.SetPropertyBlock(mpb);
    }

    public override void OnEpisodeBegin()
    {
        targetsHit = 0;
        targetA.gameObject.SetActive(true);
        targetB.gameObject.SetActive(true);

        timer = maxTime; // reinicia el contador

        float minDistance = 1.5f; 

        transform.localPosition = GetRandomPosition();

        do
        {
            targetA.localPosition = GetRandomPosition();
        }
        while (Vector3.Distance(transform.localPosition, targetA.localPosition) < minDistance);

        do
        {
            targetB.localPosition = GetRandomPosition();
        }
        while (Vector3.Distance(transform.localPosition, targetB.localPosition) < minDistance ||
               Vector3.Distance(targetA.localPosition, targetB.localPosition) < minDistance);
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-4f, 4f), 0.53529f, Random.Range(-4f, 4f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetA.localPosition);
        sensor.AddObservation(targetB.localPosition);

        // Observación adicional: tiempo restante
        sensor.AddObservation(timer / maxTime); // normalizado entre 0 y 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float movementSpeed = 5f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * movementSpeed;

        // Penalización por cada paso
        AddReward(-0.001f);

        // Actualizamos el timer
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            // Penalización si no completó ambos objetivos
            if (targetsHit < 2)
            {
                SetHeadbandColor(Color.red);
                AddReward(-2f);
            }
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform == targetA || collision.transform == targetB)
        {
            if (targetsHit == 1)
            {
                AddReward(4f);
                SetHeadbandColor(new Color32(0x29, 0x95, 0xFA, 0xFF));
                EndEpisode();
            }
            else
            {
                AddReward(2f);
                collision.gameObject.SetActive(false);
                targetsHit++;
            }
        }
        else if (collision.TryGetComponent(out Wall wall))
        {
            AddReward(-2.0f);
            SetHeadbandColor(Color.red);
            EndEpisode();
        }
    }
}