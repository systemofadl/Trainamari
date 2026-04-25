using UnityEngine;

namespace Trainamari.Core
{
    /// <summary>
    /// Global game constants and configuration.
    /// </summary>
    public static class GameConstants
    {
        // Physics
        public const float MAX_SPEED = 120f;           // km/h - top speed
        public const float ACCELERATION_RATE = 8f;     // km/h per second at full throttle
        public const float BRAKE_RATE = 15f;           // km/h per second at full brake
        public const float EMERGENCY_BRAKE_RATE = 30f; // km/h per second emergency
        public const float FRICTION_DECEL = 2f;        // km/h per second natural friction
        public const float CURVE_FRICTION_MULT = 1.5f; // extra decel on curves
        
        // Scoring
        public const float SPEED_BONUS_PER_KMH = 0.5f;       // points per km/h sustained
        public const float SMOOTH_STOP_BONUS = 100f;          // points for perfect stop
        public const float COMBO_MULTIPLIER_STEP = 0.5f;     // combo increase per smooth stop
        public const float CARGO_INTEGRITY_BONUS = 50f;       // per intact fragile cargo
        public const float TIME_BONUS_MAX = 200f;            // max bonus for on-time arrival
        
        // Stop accuracy
        public const float PERFECT_STOP_DISTANCE = 2f;       // meters - "perfect" stop
        public const float GOOD_STOP_DISTANCE = 5f;           // meters - "good" stop
        public const float OVERSHOOT_PENALTY_PER_METER = 10f; // points lost per meter past station
        
        // Derailment
        public const float CURVE_SPEED_WARNING = 0.8f;  // percentage of max speed where wobble starts
        public const float CURVE_SPEED_DERAIL = 1.0f;    // percentage of max speed = derail
    }
}