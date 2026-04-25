using UnityEngine;
using System.Collections.Generic;

namespace Trainamari.Train
{
    /// <summary>
    /// Cargo system - what the train is carrying.
    /// Different cargo types have different gameplay effects.
    /// </summary>
    public enum CargoType
    {
        Standard,       // No special effects
        Fragile,        // Takes damage on rough handling (hard braking, wobble)
        Livestock,      // Escapes on very rough handling
        Explosive,      // Detonates on emergency brake or derailment
        VIP,            // Bonus points for smooth ride
        Mystery         // Random effect revealed during transport
    }
    
    /// <summary>
    /// Represents a single cargo item being transported.
    /// </summary>
    [System.Serializable]
    public class CargoItem
    {
        public CargoType type;
        public string name;
        public int integrity = 100;    // 0-100, drops on rough handling
        public int pointValue;
        public bool delivered;
        
        public CargoItem(CargoType type, string name, int pointValue)
        {
            this.type = type;
            this.name = name;
            this.pointValue = pointValue;
            this.integrity = 100;
            this.delivered = false;
        }
        
        public void ApplyForce(float force)
        {
            switch (type)
            {
                case CargoType.Standard:
                case CargoType.VIP:
                    // No damage from normal handling
                    break;
                case CargoType.Fragile:
                    integrity -= Mathf.RoundToInt(force * 2f);
                    break;
                case CargoType.Livestock:
                    integrity -= Mathf.RoundToInt(force * 1.5f);
                    break;
                case CargoType.Explosive:
                    if (force > 80f)
                    {
                        integrity = 0; // BOOM
                    }
                    break;
                case CargoType.Mystery:
                    integrity -= Mathf.RoundToInt(force * Random.Range(0.5f, 3f));
                    break;
            }
            integrity = Mathf.Max(0, integrity);
        }
        
        public string GetStatusEmoji()
        {
            if (delivered) return "✅";
            if (integrity <= 0) return type == CargoType.Explosive ? "💥" : "❌";
            if (integrity < 50) return "⚠️";
            return "📦";
        }
    }
    
    /// <summary>
    /// Manages all cargo and passengers on the train.
    /// </summary>
    public class CargoManager : MonoBehaviour
    {
        [Header("Current Cargo")]
        [SerializeField] private List<CargoItem> cargo = new List<CargoItem>();
        [SerializeField] private int passengerCount = 0;
        [SerializeField] private int maxPassengers = 200;
        [SerializeField] private float passengerSatisfaction = 100f; // 0-100
        
        public int PassengerCount => passengerCount;
        public float PassengerSatisfaction => passengerSatisfaction;
        public List<CargoItem> Cargo => cargo;
        
        /// <summary>
        /// Add passengers at a station.
        /// </summary>
        public void BoardPassengers(int count)
        {
            passengerCount = Mathf.Min(passengerCount + count, maxPassengers);
        }
        
        /// <summary>
        /// Remove passengers at a station.
        /// Returns the number that disembarked.
        /// </summary>
        public int DisembarkPassengers(int count)
        {
            int departing = Mathf.Min(passengerCount, count);
            passengerCount -= departing;
            return departing;
        }
        
        /// <summary>
        /// Add a cargo item.
        /// </summary>
        public void LoadCargo(CargoItem item)
        {
            cargo.Add(item);
        }
        
        /// <summary>
        /// Remove cargo by index. Returns the delivered item.
        /// </summary>
        public CargoItem UnloadCargo(int index)
        {
            if (index < 0 || index >= cargo.Count) return null;
            CargoItem item = cargo[index];
            item.delivered = true;
            cargo.RemoveAt(index);
            return item;
        }
        
        /// <summary>
        /// Called every frame - applies physics forces to cargo based on train handling.
        /// </summary>
        public void UpdateCargoPhysics(float speed, float deceleration, float wobble, float dt)
        {
            // Passenger satisfaction drops with rough handling
            if (deceleration > 8f)
            {
                passengerSatisfaction -= deceleration * 0.5f * dt;
            }
            if (wobble > 0.3f)
            {
                passengerSatisfaction -= wobble * 10f * dt;
            }
            passengerSatisfaction = Mathf.Clamp(passengerSatisfaction, 0f, 100f);
            
            // Apply force to cargo
            float force = Mathf.Abs(deceleration) + wobble * 20f;
            if (force > 2f)
            {
                foreach (var item in cargo)
                {
                    item.ApplyForce(force * dt * 10f);
                }
            }
        }
        
        /// <summary>
        /// Get the total point value of all cargo and passengers.
        /// </summary>
        public int CalculateCargoScore()
        {
            int score = 0;
            foreach (var item in cargo)
            {
                if (item.delivered)
                {
                    score += Mathf.RoundToInt(item.pointValue * (item.integrity / 100f));
                }
            }
            score += Mathf.RoundToInt(passengerCount * (passengerSatisfaction / 100f));
            return score;
        }
    }
}