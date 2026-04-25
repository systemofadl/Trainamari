using UnityEngine;
using Trainamari.Train;

namespace Trainamari.Core
{
    /// <summary>
    /// Scoring system - tracks combos, multipliers, and final scores.
    /// Katamari-style: everything chains, big numbers feel good.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score State")]
        [SerializeField] private int totalScore = 0;
        [SerializeField] private float comboMultiplier = 1f;
        [SerializeField] private int comboCount = 0;
        [SerializeField] private int stationsVisited = 0;
        
        [Header("Combo Settings")]
        [SerializeField] private float comboDecayTime = 30f;      // seconds before combo resets
        [SerializeField] private float comboTimer = 0f;
        
        public int TotalScore => totalScore;
        public float ComboMultiplier => comboMultiplier;
        public int ComboCount => comboCount;
        
        private void Update()
        {
            if (comboCount > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }
        
        /// <summary>
        /// Add points with current combo multiplier.
        /// </summary>
        public void AddScore(int basePoints, string reason = "")
        {
            int points = Mathf.RoundToInt(basePoints * comboMultiplier);
            totalScore += points;
            
            if (!string.IsNullOrEmpty(reason))
            {
                Debug.Log($"[Score] +{points} ({reason}, x{comboMultiplier:F1})");
            }
        }
        
        /// <summary>
        /// Called when train stops at a station.
        /// </summary>
        public void OnStationStop(float stopAccuracy, float passengerSatisfaction, CargoManager cargo)
        {
            stationsVisited++;
            
            // Smooth stop bonus
            if (stopAccuracy <= GameConstants.PERFECT_STOP_DISTANCE)
            {
                AddScore(GameConstants.SMOOTH_STOP_BONUS, "PERFECT STOP!");
                AddCombo();
            }
            else if (stopAccuracy <= GameConstants.GOOD_STOP_DISTANCE)
            {
                AddScore(Mathf.RoundToInt(GameConstants.SMOOTH_STOP_BONUS * 0.6f), "Good stop");
                AddCombo();
            }
            else
            {
                // Penalty for overshooting
                int penalty = Mathf.RoundToInt(stopAccuracy * GameConstants.OVERSHOOT_PENALTY_PER_METER);
                totalScore = Mathf.Max(0, totalScore - penalty);
                ResetCombo();
                Debug.Log($"[Score] -{penalty} (overshot station by {stopAccuracy:F1}m)");
            }
            
            // Passenger satisfaction bonus
            int passengerBonus = Mathf.RoundToInt(passengerSatisfaction * 0.5f);
            AddScore(passengerBonus, "passenger comfort");
            
            // Cargo delivery bonus
            if (cargo != null)
            {
                AddScore(cargo.CalculateCargoScore(), "cargo delivery");
            }
        }
        
        /// <summary>
        /// Called for maintaining high speed.
        /// </summary>
        public void OnSpeedMaintained(float speedKmh, float dt)
        {
            if (speedKmh > GameConstants.MAX_SPEED * 0.7f)
            {
                AddScore(Mathf.RoundToInt(GameConstants.SPEED_BONUS_PER_KMH * speedKmh * dt), "speed bonus");
            }
        }
        
        /// <summary>
        /// Called when horn is used (style points if near crossing or crowd).
        /// </summary>
        public void OnHorn(bool nearCrossingOrCrowd)
        {
            if (nearCrossingOrCrowd)
            {
                AddScore(25, "horn style");
            }
        }
        
        /// <summary>
        /// Increment the combo chain.
        /// </summary>
        private void AddCombo()
        {
            comboCount++;
            comboMultiplier = 1f + (comboCount * GameConstants.COMBO_MULTIPLIER_STEP);
            comboTimer = comboDecayTime;
            
            Debug.Log($"[Score] Combo x{comboMultiplier:F1}! ({comboCount} chain)");
        }
        
        /// <summary>
        /// Reset the combo (rough handling, missed station, etc.).
        /// </summary>
        public void ResetCombo()
        {
            if (comboCount > 0)
            {
                Debug.Log($"[Score] Combo broken at x{comboMultiplier:F1} ({comboCount} chain)");
            }
            comboCount = 0;
            comboMultiplier = 1f;
            comboTimer = 0f;
        }
        
        /// <summary>
        /// Called on derailment - big penalty.
        /// </summary>
        public void OnDerailment()
        {
            totalScore = Mathf.Max(0, totalScore - 500);
            ResetCombo();
            Debug.Log("[Score] DERAILMENT! -500 points, combo broken");
        }
        
        /// <summary>
        /// Get the final score for a completed level.
        /// </summary>
        public LevelResult CalculateLevelResult(float timeRemaining, float maxTime, CargoManager cargo)
        {
            // Time bonus
            float timeRatio = timeRemaining / maxTime;
            int timeBonus = Mathf.RoundToInt(GameConstants.TIME_BONUS_MAX * timeRatio);
            AddScore(timeBonus, "time bonus");
            
            return new LevelResult
            {
                totalScore = totalScore,
                maxCombo = comboMultiplier,
                stationsVisited = stationsVisited,
                timeBonus = timeBonus,
                cargoScore = cargo?.CalculateCargoScore() ?? 0,
                grade = CalculateGrade(totalScore)
            };
        }
        
        private char CalculateGrade(int score)
        {
            if (score >= 5000) return 'S';
            if (score >= 4000) return 'A';
            if (score >= 3000) return 'B';
            if (score >= 2000) return 'C';
            if (score >= 1000) return 'D';
            return 'F';
        }
    }
    
    [System.Serializable]
    public class LevelResult
    {
        public int totalScore;
        public float maxCombo;
        public int stationsVisited;
        public int timeBonus;
        public int cargoScore;
        public char grade;
        
        public string GetGradeEmoji()
        {
            return grade switch
            {
                'S' => "🌟",
                'A' => "🅰️",
                'B' => "✨",
                'C' => "👍",
                'D' => "😅",
                'F' => "💀",
                _ => "?"
            };
        }
    }
}