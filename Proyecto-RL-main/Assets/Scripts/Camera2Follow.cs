using UnityEngine;

namespace Unity.MLAgentsExamples
{
    public class Camera2Follow : MonoBehaviour
    {
        public Transform target1;
        public Transform target2;

        public float smoothingTime = 0.3f;
        public float targetSmoothLerp = 0.1f;      // suavizado individual del agente
        public float midpointLerp = 0.15f;         // suavizado del midpoint
        public float maxMidpointDelta = 1.0f;      // límite anti-jitter

        private Vector3 smoothT1;
        private Vector3 smoothT2;

        private Vector3 m_Offset;
        private Vector3 m_CamVelocity;
        private Vector3 smoothedMidpoint;

        void Start()
        {
            smoothT1 = target1.position;
            smoothT2 = target2.position;

            Vector3 midpoint = (smoothT1 + smoothT2) * 0.5f;

            m_Offset = transform.position - midpoint;
            m_Offset.z += 25f;
            m_Offset.y += 15f;
            smoothedMidpoint = midpoint;
        }

        void FixedUpdate()
        {
            // Suavizamos cada target individualmente (clave para gusanos)
            smoothT1 = Vector3.Lerp(smoothT1, target1.position, targetSmoothLerp);
            smoothT2 = Vector3.Lerp(smoothT2, target2.position, targetSmoothLerp);

            // Midpoint crudo
            Vector3 rawMidpoint = (smoothT1 + smoothT2) * 0.5f;

            // NO seguir altura loca del gusano
            rawMidpoint.y = smoothedMidpoint.y;

            // Limitador anti-jitter
            Vector3 delta = rawMidpoint - smoothedMidpoint;
            if (delta.magnitude > maxMidpointDelta)
                delta = delta.normalized * maxMidpointDelta;

            smoothedMidpoint += delta;

            // Suavizado final
            smoothedMidpoint = Vector3.Lerp(smoothedMidpoint, rawMidpoint, midpointLerp);

            // Nueva posición
            Vector3 newPosition = smoothedMidpoint + m_Offset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                newPosition,
                ref m_CamVelocity,
                smoothingTime,
                Mathf.Infinity,
                Time.fixedDeltaTime
            );

            transform.LookAt(smoothedMidpoint);
        }
    }
}
