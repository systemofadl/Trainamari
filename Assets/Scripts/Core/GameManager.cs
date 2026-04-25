using UnityEngine;
using UnityEngine.SceneManagement;
using Trainamari.Train;
using Trainamari.Input;
using Trainamari.UI;

namespace Trainamari.Core
{
    /// <summary>
    /// Main game manager - ties everything together.
    /// Handles level flow, game state, and coordination between systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            TitleScreen,
            LevelSelect,
            Playing,
            Paused,
            StationArrival,
            LevelComplete,
            GameOver,
            Derailment
        }
        
        [Header("References")]
        [SerializeField] private TrainController train;
        [SerializeField] private TrainInput trainInput;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private CargoManager cargoManager;
        [SerializeField] private TrainHUD hud;
        
        [Header("Level")]
        [SerializeField] private Level.LevelConfig currentLevel;
        [SerializeField] private int currentStationIndex = 0;
        [SerializeField] private float levelTimer;
        
        [Header("Game State")]
        [SerializeField] private GameState state = GameState.TitleScreen;
        
        public GameState State => state;
        public Level.LevelConfig CurrentLevel => currentLevel;
        
        // Events for UI/Audio
        public System.Action<GameState> OnStateChanged;
        public System.Action<LevelResult> OnLevelComplete;
        public System.Action OnDerailment;
        public System.Action<string> OnStationApproach;
        
        private void Start()
        {
            SetState(GameState.TitleScreen);
        }
        
        private void Update()
        {
            switch (state)
            {
                case GameState.Playing:
                    UpdatePlaying();
                    break;
                case GameState.StationArrival:
                    UpdateStationArrival();
                    break;
                case GameState.Derailment:
                    UpdateDerailment();
                    break;
            }
            
            // Global pause
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (state == GameState.Playing)
                    SetState(GameState.Paused);
                else if (state == GameState.Paused)
                    SetState(GameState.Playing);
            }
        }
        
        private void UpdatePlaying()
        {
            // Update timer
            if (currentLevel != null && currentLevel.hasTimeLimit)
            {
                levelTimer -= Time.deltaTime;
                if (levelTimer <= 0f)
                {
                    levelTimer = 0f;
                    // Time's up - calculate final score
                    CompleteLevel();
                    return;
                }
            }
            
            // Update cargo physics
            if (cargoManager != null && train != null)
            {
                float decel = train.CurrentSpeed > 0 ? 
                    Mathf.Abs(trainInput.Throttle) * GameConstants.BRAKE_RATE : 0f;
                cargoManager.UpdateCargoPhysics(
                    train.CurrentSpeed,
                    decel,
                    0f, // wobble comes from TrainController
                    Time.deltaTime
                );
            }
            
            // Check for approaching station
            if (currentLevel != null && currentStationIndex < currentLevel.stations.Length)
            {
                var station = currentLevel.stations[currentStationIndex];
                float distToStation = station.trackDistance - train.DistanceTraveled;
                
                if (distToStation > 0 && distToStation < 100f)
                {
                    hud?.ShowStationAlert(station.stationName, distToStation);
                    OnStationApproach?.Invoke(station.stationName);
                }
            }
        }
        
        private void UpdateStationArrival()
        {
            // Wait for train to stop, then process station
            if (train != null && train.CurrentSpeed < 0.5f)
            {
                ProcessStationArrival();
            }
        }
        
        private void UpdateDerailment()
        {
            // Wait a moment, then offer restart
            // TODO: Show derailment animation, camera shake
        }
        
        /// <summary>
        /// Start a level.
        /// </summary>
        public void StartLevel(Level.LevelConfig level)
        {
            currentLevel = level;
            currentStationIndex = 0;
            levelTimer = level.timeLimit;
            
            // Reset systems
            train?.ResetAfterDerail();
            scoreManager = FindObjectOfType<ScoreManager>();
            cargoManager = FindObjectOfType<CargoManager>();
            
            SetState(GameState.Playing);
        }
        
        /// <summary>
        /// Called when the train reaches a station zone.
        /// </summary>
        public void OnTrainReachedStation()
        {
            if (currentStationIndex >= currentLevel.stations.Length) return;
            
            SetState(GameState.StationArrival);
        }
        
        private void ProcessStationArrival()
        {
            var station = currentLevel.stations[currentStationIndex];
            
            // Calculate stop quality
            float stopAccuracy = train.StopAtStation(
                station.trackDistance,
                station.platformLength
            );
            
            // Score the stop
            scoreManager.OnStationStop(stopAccuracy, cargoManager.PassengerSatisfaction, cargoManager);
            
            // Board/exchange passengers
            cargoManager.BoardPassengers(station.passengersWaiting);
            cargoManager.DisembarkPassengers(station.passengersExiting);
            
            // Load cargo
            if (station.cargoType != CargoType.Standard || !string.IsNullOrEmpty(station.cargoName))
            {
                cargoManager.LoadCargo(new CargoItem(
                    station.cargoType,
                    station.cargoName,
                    station.cargoPoints
                ));
            }
            
            // Check if final station
            if (station.isFinalStation)
            {
                CompleteLevel();
                return;
            }
            
            currentStationIndex++;
            SetState(GameState.Playing);
        }
        
        /// <summary>
        /// Called when train derails.
        /// </summary>
        public void OnTrainDerailed()
        {
            SetState(GameState.Derailment);
            scoreManager.OnDerailment();
            OnDerailment?.Invoke();
        }
        
        /// <summary>
        /// Complete the level and calculate final score.
        /// </summary>
        private void CompleteLevel()
        {
            float timeRemaining = currentLevel?.hasTimeLimit == true ? levelTimer : currentLevel.timeLimit;
            var result = scoreManager.CalculateLevelResult(timeRemaining, currentLevel.timeLimit, cargoManager);
            
            SetState(GameState.LevelComplete);
            OnLevelComplete?.Invoke(result);
        }
        
        private void SetState(GameState newState)
        {
            state = newState;
            OnStateChanged?.Invoke(newState);
            
            // Pause/unpause simulation
            Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
        }
        
        /// <summary>
        /// Restart the current level.
        /// </summary>
        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        /// <summary>
        /// Return to level select.
        /// </summary>
        public void ReturnToMenu()
        {
            SetState(GameState.TitleScreen);
            // TODO: Load title screen scene
        }
    }
}