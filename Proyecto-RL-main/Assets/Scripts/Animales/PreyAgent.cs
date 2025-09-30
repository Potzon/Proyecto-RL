using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
public class PreyAgent : Agent
{
    private Rigidbody rb;
    private NatureEnvController envController;
    private Animator animator;
    private string currentState;
    private const string RUN = "Walk";

    [SerializeField] private float surviveReward = 0.01f; // recompensa por cada paso vivo

    public int survivedSteps = 0; // usado si quieres loguear en NatureEnvController

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envController = GetComponentInParent<NatureEnvController>();

        animator = GetComponent<Animator>();
        ChangeAnimationState(RUN);
    }

    public override void OnEpisodeBegin()
    {
        // Reiniciar la velocidad y posición de la presa
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Coloca la presa en un sitio aleatorio dentro del área del EnvController
        if (envController.placeRandomly)
        {
            float rndX = Random.Range(-envController.rnd_x_width / 2, envController.rnd_x_width / 2);
            float rndZ = Random.Range(-envController.rnd_z_width / 2, envController.rnd_z_width / 2);
            transform.localPosition = new Vector3(rndX, 0f, rndZ);
            transform.localRotation = Quaternion.Euler(0f, Random.Range(envController.rotMin, envController.rotMax), 0f);
        }
    }

    // 👇 Si usas RayPerception, aquí no necesitas meter posiciones del predador.
    // Solo puedes añadir tu propia info adicional si quieres (ej: velocidad).
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveForward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rotate = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 forwardMove = transform.forward * (moveForward * envController.preyMoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + forwardMove);

        Quaternion turn = Quaternion.Euler(0f, rotate * envController.preyRotateSpeed, 0f);
        rb.MoveRotation(rb.rotation * turn);

        // Recompensa por sobrevivir
        AddReward(surviveReward);
        survivedSteps++;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical");   // adelante/atrás
        continuousActions[1] = Input.GetAxisRaw("Horizontal"); // girar
    }

    private void ChangeAnimationState(string newState)
    {
        if (animator != null && newState != currentState)
        {
            animator.Play(newState);
            currentState = newState;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Predator"))
        {
            // Avisamos al controlador de que esta presa fue cazada
            envController.PredatorPreyCollision(this, other.GetComponent<Agent>());
        }
    }
}
