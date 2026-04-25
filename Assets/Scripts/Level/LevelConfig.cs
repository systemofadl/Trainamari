using UnityEngine;
using System.Collections.Generic;
using Trainamari.Train;

namespace Trainamari.Level
{
    /// <summary>
    /// Defines a station on the route.
    /// </summary>
    [System.Serializable]
    public class StationDefinition
    {
        public string stationName;
        public float trackDistance;      // distance along track in meters
        public float platformLength = 20f; // meters
        public int passengersWaiting;    // how many want to board
        public int passengersExiting;    // how many get off
        public CargoType cargoType;      // what cargo is available here
        public string cargoName;         // name of cargo item
        public int cargoPoints;          // point value of cargo
        public float stopWindow = 30f;   // seconds the train can be at the platform
        public bool isFinalStation;
    }
    
    /// <summary>
    /// Level configuration - ScriptableObject for easy level design.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Trainamari/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Info")]
        public string levelName;
        public string levelDescription;
        public int levelNumber;
        public AudioClip musicTrack;
        public AudioClip ambientTrack;
        
        [Header("Time")]
        public float timeLimit = 300f;        // seconds
        public bool hasTimeLimit = true;
        
        [Header("Track")]
        public Vector3[] trackControlPoints;
        public bool closedLoop = false;
        
        [Header("Stations")]
        public StationDefinition[] stations;
        
        [Header("Weather / Environment")]
        public WeatherType weather = WeatherType.Clear;
        public float visibility = 1f;         // 0-1, affects night/fog
        public float trackGrip = 1f;          // 0-1, rain/ice reduces grip
        public bool hasObstacles = false;
        
        [Header("Difficulty")]
        public float speedMultiplier = 1f;     // >1 = faster required pace
        public float curveIntensity = 1f;       // >1 = tighter curves
        public int cargoCount = 0;              // extra cargo to deliver
        public bool allowDerailment = true;
        
        [Header("Scoring Targets")]
        public int scoreForStar1 = 1000;
        public int scoreForStar2 = 3000;
        public int scoreForStar3 = 5000;
    }
    
    public enum WeatherType
    {
        Clear,
        Rain,
        Snow,
        Fog,
        Storm,
        Night,
        NightRain,
        Ghost  // supernatural level
    }
}