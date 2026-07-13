using SaveAndLoad;
using UnityEngine;

namespace Components.Test
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsPatrol : MonoBehaviour, ISaveable
    {
        public Vector3 pointA = new(-35f, 13f, 45f);
        public Vector3 pointB = new(35f, 13f, 45f);

        public float speed = 2f;
        public float arriveDistance = 1f;
        public float acceleration = 5f;

        [SerializeField] private Rigidbody rb;
        private Vector3 target;
        private bool targetInitialized;

        [Header("Save data")] 
        private bool isTargetA;

        private void Awake()
        {
            if (targetInitialized) return;
            target = pointB;
            targetInitialized = true;
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

        public string GetSaveData()
        {
            return (target == pointA).ToString();
        }

        public void LoadSaveData(string data)
        {
            target = bool.Parse(data) ? pointA : pointB;
            targetInitialized = true;
        }
    }
}