using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
public class PredatorAgent : Agent
{
    private Rigidbody rb;
    private NatureEnvController envController;
    private Animator animator;
    private string currentState;

    private const string RUN = "Tiger_001_run"; // ajusta al nombre exacto de tu animación

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envController = GetComponentInParent<NatureEnvController>();

        animator = GetComponent<Animator>();
        ChangeAnimationState(RUN);
    }

    public override void OnEpisodeBegin()
    {
        // Reinicia la velocidad y posición del depredador
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (envController.placeRandomly)
        {
            float rndX = Random.Range(-envController.rnd_x_width / 2, envController.rnd_x_width / 2);
            float rndZ = Random.Range(-envController.rnd_z_width / 2, envController.rnd_z_width / 2);
            transform.localPosition = new Vector3(rndX, 0f, rndZ);
            transform.localRotation = Quaternion.Euler(0f, Random.Range(envController.rotMin, envController.rotMax), 0f);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Con RayPerception el depredador ya "ve" presas y obstáculos
        // Aquí añadimos solo info adicional si quieres (ej: velocidad propia)
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveForward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rotate = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 forwardMove = transform.forward * (moveForward * envController.predatorMoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + forwardMove);

        Quaternion turn = Quaternion.Euler(0f, rotate * envController.predatorRotateSpeed, 0f);
        rb.MoveRotation(rb.rotation * turn);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical");   // adelante/atrás
        continuousActions[1] = Input.GetAxisRaw("Horizontal"); // girar
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Prey"))
        {
            Agent preyAgent = other.GetComponent<Agent>();
            envController.PredatorPreyCollision(preyAgent, this);
        }
    }

    private void ChangeAnimationState(string newState)
    {
        if (animator != null && newState != currentState)
        {
            animator.Play(newState);
            currentState = newState;
        }
    }
}
