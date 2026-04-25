using UnityEngine;
using Trainamari.Core;
using Trainamari.Train;
using Trainamari.Input;

namespace Trainamari.Audio
{
    /// <summary>
    /// Train audio system - handles all train sounds.
    /// Satisfying lever clunks, brake screeches, chugging, horn.
    /// This is where the "feel" lives - sound is 50% of the experience.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class TrainAudio : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TrainController train;
        [SerializeField] private TrainInput trainInput;
        
        [Header("Engine Sounds")]
        [SerializeField] private AudioClip engineIdle;
        [SerializeField] private AudioClip engineLow;
        [SerializeField] private AudioClip engineMid;
        [SerializeField] private AudioClip engineHigh;
        [SerializeField] private AudioClip engineMax;
        
        [Header("Action Sounds")]
        [SerializeField] private AudioClip brakeSqueal;
        [SerializeField] private AudioClip emergencyBrake;
        [SerializeField] private AudioClip hornSound;
        [SerializeField] private AudioClip doorClose;
        [SerializeField] private AudioClip leverClunk;
        [SerializeField] private AudioClip stationChime;
        [SerializeField] private AudioClip derailSound;
        
        [Header("Sound Settings")]
        [SerializeField] private float brakeSquealThreshold = 10f;  // km/h decel to start squealing
        [SerializeField] private float maxEnginePitch = 2f;
        [SerializeField] private float idleEnginePitch = 0.3f;
        
        // Audio sources (pooled for overlapping sounds)
        private AudioSource engineSource;
        private AudioSource brakeSource;
        private AudioSource oneShotSource;
        private AudioSource hornSource;
        
        // State tracking
        private float previousThrottle = 0f;
        private bool wasEmergencyBrake = false;
        private bool wasHorn = false;
        private float currentEnginePitch;
        
        private void Awake()
        {
            // Create audio source pool
            engineSource = gameObject.AddComponent<AudioSource>();
            brakeSource = gameObject.AddComponent<AudioSource>();
            oneShotSource = gameObject.AddComponent<AudioSource>();
            hornSource = gameObject.AddComponent<AudioSource>();
            
            engineSource.loop = true;
            brakeSource.loop = true;
            hornSource.loop = false;
            oneShotSource.loop = false;
        }
        
        private void Start()
        {
            // Start engine idle sound
            if (engineIdle != null)
            {
                engineSource.clip = engineIdle;
                engineSource.pitch = idleEnginePitch;
                engineSource.Play();
            }
        }
        
        private void Update()
        {
            if (train == null || trainInput == null) return;
            
            UpdateEngineSound();
            UpdateBrakeSound();
            CheckLeverClunk();
            CheckHorn();
        }
        
        private void UpdateEngineSound()
        {
            float speed = train.CurrentSpeed;
            float speedRatio = speed / GameConstants.MAX_SPEED;
            
            // Pitch shifts with speed
            float targetPitch = Mathf.Lerp(idleEnginePitch, maxEnginePitch, speedRatio);
            currentEnginePitch = Mathf.Lerp(currentEnginePitch, targetPitch, Time.deltaTime * 3f);
            engineSource.pitch = currentEnginePitch;
            
            // Volume based on throttle
            float targetVolume = Mathf.Lerp(0.3f, 1f, Mathf.Abs(trainInput.Throttle));
            engineSource.volume = Mathf.Lerp(engineSource.volume, targetVolume, Time.deltaTime * 5f);
            
            // Swap clips based on speed range
            AudioClip targetClip = engineIdle;
            if (speedRatio > 0.75f) targetClip = engineMax;
            else if (speedRatio > 0.5f) targetClip = engineHigh;
            else if (speedRatio > 0.25f) targetClip = engineMid;
            else if (speedRatio > 0.05f) targetClip = engineLow;
            
            if (targetClip != null && targetClip != engineSource.clip)
            {
                engineSource.clip = targetClip;
                engineSource.Play();
            }
        }
        
        private void UpdateBrakeSound()
        {
            float decelForce = -trainInput.Throttle; // positive when braking
            float speed = train.CurrentSpeed;
            
            if (decelForce > 0.3f && speed > 5f)
            {
                // Brake squeal
                if (!brakeSource.isPlaying && brakeSqueal != null)
                {
                    brakeSource.clip = brakeSqueal;
                    brakeSource.Play();
                }
                brakeSource.volume = Mathf.Lerp(0f, decelForce * 0.8f, Time.deltaTime * 10f);
                brakeSource.pitch = Mathf.Lerp(0.8f, 1.5f, speed / GameConstants.MAX_SPEED);
            }
            else
            {
                brakeSource.volume = Mathf.Lerp(brakeSource.volume, 0f, Time.deltaTime * 5f);
                if (brakeSource.volume < 0.01f)
                {
                    brakeSource.Stop();
                }
            }
            
            // Emergency brake sound
            if (trainInput.EmergencyBrake && !wasEmergencyBrake)
            {
                if (emergencyBrake != null)
                    oneShotSource.PlayOneShot(emergencyBrake, 1f);
                wasEmergencyBrake = true;
            }
            else if (!trainInput.EmergencyBrake)
            {
                wasEmergencyBrake = false;
            }
        }
        
        private void CheckLeverClunk()
        {
            // Play lever clunk when throttle changes direction
            float currentThrottle = trainInput.Throttle;
            if (Mathf.Sign(currentThrottle) != Mathf.Sign(previousThrottle) && previousThrottle != 0f)
            {
                if (leverClunk != null)
                    oneShotSource.PlayOneShot(leverClunk, 0.7f);
            }
            previousThrottle = currentThrottle;
        }
        
        private void CheckHorn()
        {
            if (trainInput.HornDown && !wasHorn)
            {
                if (hornSound != null)
                    hornSource.PlayOneShot(hornSound, 0.8f);
                wasHorn = true;
            }
            else if (!trainInput.Horn)
            {
                wasHorn = false;
            }
        }
        
        /// <summary>
        /// Play the station arrival chime.
        /// </summary>
        public void PlayStationChime()
        {
            if (stationChime != null)
                oneShotSource.PlayOneShot(stationChime, 0.5f);
        }
        
        /// <summary>
        /// Play door close sound.
        /// </summary>
        public void PlayDoorClose()
        {
            if (doorClose != null)
                oneShotSource.PlayOneShot(doorClose, 0.6f);
        }
        
        /// <summary>
        /// Play derailment sound.
        /// </summary>
        public void PlayDerailment()
        {
            if (derailSound != null)
                oneShotSource.PlayOneShot(derailSound, 1f);
        }
    }
}