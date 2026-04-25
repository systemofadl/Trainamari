using UnityEngine;
using Trainamari.Input;

namespace Trainamari.Train
{
    /// <summary>
    /// Core train physics controller.
    /// Handles acceleration, braking, momentum, track following, and derailment.
    /// 
    /// The train feels HEAVY. It doesn't stop on a dime. That's the game.
    /// 
    /// Speed is in km/h internally, position follows a spline track.
    /// </summary>
    [RequireComponent(typeof(TrainInput))]
    public class TrainController : MonoBehaviour
    {
        [Header("Train Properties")]
        [SerializeField] private float mass = 100f;              // arbitrary mass units
        [SerializeField] private float maxSpeed = GameConstants.MAX_SPEED;
        [SerializeField] private float accelerationRate = GameConstants.ACCELERATION_RATE;
        [SerializeField] private float brakeRate = GameConstants.BRAKE_RATE;
        [SerializeField] private float emergencyBrakeRate = GameConstants.EMERGENCY_BRAKE_RATE;
        [SerializeField] private float frictionDecel = GameConstants.FRICTION_DECEL;
        
        [Header("Track Following")]
        [SerializeField] private TrackSpline track;
        [SerializeField] private float trackDistance = 0f;       // distance along track in meters
        [SerializeField] private bool autoGenerateTrack = true;  // create a default loop if none assigned
        
        [Header("Visuals")]
        [SerializeField] private float wobbleIntensity = 0f;     // set by high speed on curves
        [SerializeField] private GameObject wobbleEffect;        // visual derailment warning
        
        // State
        public float CurrentSpeed { get; private set; }          // km/h
        public float DistanceTraveled { get; private set; }     // total meters traveled
        public bool IsDerailed { get; private set; }
        public float ThrottlePosition { get; private set; }      // -1 to 1 from input
        public float CurveBanking { get; private set; }          // current curve intensity
        
        // Components
        private TrainInput trainInput;
        private Rigidbody rb;
        
        // Track data
        private Vector3[] trackPoints;
        private float[] trackCurvatures;
        private float totalTrackLength;
        
        private void Awake()
        {
            trainInput = GetComponent<TrainInput>();
            
            if (track == null && autoGenerateTrack)
            {
                track = CreateDefaultTrack();
            }
        }
        
        private void Start()
        {
            InitializeTrack();
            CurrentSpeed = 0f;
            IsDerailed = false;
            DistanceTraveled = 0f;
        }
        
        private void Update()
        {
            if (IsDerailed) return;
            
            ThrottlePosition = trainInput.Throttle;
            float dt = Time.deltaTime;
            
            // Calculate acceleration based on throttle input
            float acceleration = 0f;
            
            if (trainInput.EmergencyBrake)
            {
                // Emergency stop
                acceleration = -emergencyBrakeRate;
            }
            else if (ThrottlePosition > 0.05f)
            {
                // Accelerating
                acceleration = ThrottlePosition * accelerationRate;
            }
            else if (ThrottlePosition < -0.05f)
            {
                // Braking
                acceleration = ThrottlePosition * brakeRate; // negative * positive = negative
            }
            else
            {
                // Neutral - coast with friction
                acceleration = -Mathf.Sign(CurrentSpeed) * frictionDecel;
            }
            
            // Apply curve friction (slow down on curves)
            float curveFriction = GetCurveAtPosition(trackDistance) * GameConstants.CURVE_FRICTION_MULT;
            if (CurrentSpeed > 0)
            {
                acceleration -= curveFriction * (CurrentSpeed / maxSpeed);
            }
            
            // Apply acceleration
            CurrentSpeed += acceleration * dt;
            CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0f, maxSpeed);
            
            // Check for derailment on curves
            CheckDerailment();
            
            // Move along track
            float distanceThisFrame = (CurrentSpeed / 3.6f) * dt; // km/h to m/s
            trackDistance += distanceThisFrame;
            DistanceTraveled += distanceThisFrame;
            
            // Wrap track distance
            if (trackDistance >= totalTrackLength)
            {
                trackDistance -= totalTrackLength;
            }
            
            // Update position and rotation on track
            UpdateTransform();
        }
        
        /// <summary>
        /// Get the curve intensity at a given track position.
        /// Returns 0 for straight, higher values for tighter curves.
        /// </summary>
        private float GetCurveAtPosition(float position)
        {
            if (trackCurvatures == null || trackCurvatures.Length == 0) return 0f;
            
            float normalizedPos = position / totalTrackLength;
            int index = Mathf.FloorToInt(normalizedPos * trackCurvatures.Length);
            index = Mathf.Clamp(index, 0, trackCurvatures.Length - 1);
            
            return trackCurvatures[index];
        }
        
        private void CheckDerailment()
        {
            float curve = GetCurveAtPosition(trackDistance);
            float speedRatio = CurrentSpeed / maxSpeed;
            
            // Wobble warning
            if (speedRatio > GameConstants.CURVE_SPEED_WARNING && curve > 0.3f)
            {
                wobbleIntensity = Mathf.Lerp(wobbleIntensity, (speedRatio - GameConstants.CURVE_SPEED_WARNING) / (1f - GameConstants.CURVE_SPEED_WARNING), Time.deltaTime * 5f);
            }
            else
            {
                wobbleIntensity = Mathf.Lerp(wobbleIntensity, 0f, Time.deltaTime * 3f);
            }
            
            // Actual derailment
            if (curve > 0 && speedRatio > GameConstants.CURVE_SPEED_DERAIL / (1f + curve))
            {
                Derail();
            }
        }
        
        private void Derail()
        {
            IsDerailed = true;
            CurrentSpeed = 0f;
            Debug.Log("[TrainController] DERAILMENT! Speed was too high for the curve.");
            // TODO: Play derailment animation, camera shake, sound effect
            // TODO: Score penalty, respawn after delay
        }
        
        /// <summary>
        /// Reset after derailment.
        /// </summary>
        public void ResetAfterDerail()
        {
            IsDerailed = false;
            CurrentSpeed = 0f;
            ThrottlePosition = 0f;
            wobbleIntensity = 0f;
        }
        
        private void UpdateTransform()
        {
            if (track == null) return;
            
            // Get position and direction from track spline
            Vector3 position = track.GetPointAtDistance(trackDistance);
            Vector3 direction = track.GetDirectionAtDistance(trackDistance);
            
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            
            // Apply wobble
            if (wobbleIntensity > 0.01f)
            {
                float wobbleX = Mathf.PerlinNoise(Time.time * 10f, 0f) * wobbleIntensity * 0.3f;
                float wobbleZ = Mathf.PerlinNoise(0f, Time.time * 10f) * wobbleIntensity * 0.15f;
                transform.Rotate(wobbleX, 0f, wobbleZ);
            }
        }
        
        private TrackSpline CreateDefaultTrack()
        {
            GameObject trackObj = new GameObject("Default Track");
            TrackSpline spline = trackObj.AddComponent<TrackSpline>();
            
            // Create an oval loop track
            int pointCount = 32;
            Vector3[] points = new Vector3[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                float angle = (i / (float)pointCount) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * 100f;
                float z = Mathf.Sin(angle) * 60f;
                points[i] = new Vector3(x, 0f, z);
            }
            
            spline.SetControlPoints(points);
            return spline;
        }
        
        private void InitializeTrack()
        {
            if (track != null)
            {
                totalTrackLength = track.GetLength();
                trackCurvatures = track.GetCurvatures();
            }
        }
        
        /// <summary>
        /// Called when train reaches a station platform.
        /// Returns the stop quality: 0 = perfect, higher = worse.
        /// </summary>
        public float StopAtStation(float platformCenter, float platformLength)
        {
            float distanceFromCenter = Mathf.Abs(trackDistance - platformCenter);
            float halfPlatform = platformLength / 2f;
            
            if (distanceFromCenter <= GameConstants.PERFECT_STOP_DISTANCE)
            {
                Debug.Log("[TrainController] PERFECT STOP!");
                return 0f;
            }
            else if (distanceFromCenter <= halfPlatform)
            {
                Debug.Log($"[TrainController] Good stop - {distanceFromCenter:F1}m from center");
                return distanceFromCenter;
            }
            else
            {
                float overshoot = distanceFromCenter - halfPlatform;
                Debug.Log($"[TrainController] Overshot station by {overshoot:F1}m!");
                return overshoot * GameConstants.OVERSHOOT_PENALTY_PER_METER;
            }
        }
    }
}