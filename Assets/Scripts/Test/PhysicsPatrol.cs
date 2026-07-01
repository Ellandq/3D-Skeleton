using UnityEngine;

namespace Test
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsPatrol : MonoBehaviour
    {
        public Vector3 pointA = new(-35f, 13f, 45f);
        public Vector3 pointB = new(35f, 13f, 45f);

        public float speed = 2f;
        public float arriveDistance = 1f;
        public float acceleration = 5f;

        private Rigidbody rb;
        private Vector3 target;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            target = pointB;
        }

        private void FixedUpdate()
        {
            var toTarget = target - rb.position;
            var distance = toTarget.magnitude;

            var desiredVelocity = Vector3.zero;

            if (distance > 0.001f)
            {
                var direction = toTarget / distance;

                var rampedSpeed = speed * (distance / arriveDistance);
                var clampedSpeed = Mathf.Min(rampedSpeed, speed);

                desiredVelocity = direction * clampedSpeed;
            }

            var steering = (desiredVelocity - rb.linearVelocity) * acceleration;

            rb.AddForce(steering, ForceMode.Acceleration);

            if (distance <= arriveDistance)
            {
                target = (target == pointA) ? pointB : pointA;
            }
        }
    }
}