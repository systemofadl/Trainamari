using UnityEngine;
using Trainamari.Core;
using Trainamari.Train;
using Trainamari.Input;

namespace Trainamari.Level
{
    /// <summary>
    /// Procedural level generator for quick prototyping.
    /// Creates a track, stations, and environment from a LevelConfig.
    /// 
    /// For the final game, levels will be hand-designed in the editor,
    /// but this lets us test gameplay immediately.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject trackPrefab;
        [SerializeField] private GameObject stationPrefab;
        [SerializeField] private GameObject trainPrefab;
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private GameObject treePrefab;
        [SerializeField] private GameObject buildingPrefab;
        
        [Header("Generation Settings")]
        [SerializeField] private float groundSegmentSize = 50f;
        [SerializeField] private int treeCount = 100;
        [SerializeField] private int buildingCount = 20;
        [SerializeField] private float trackVisualWidth = 1.5f;
        
        [Header("PS1 Style")]
        [SerializeField] private Material ps1GroundMaterial;
        [SerializeField] private Material ps1TrackMaterial;
        [SerializeField] private Material ps1StationMaterial;
        [SerializeField] private Material ps1TreeMaterial;
        [SerializeField] private Material ps1BuildingMaterial;
        
        private GameObject levelRoot;
        private TrackSpline trackSpline;
        
        /// <summary>
        /// Generate a level from a LevelConfig.
        /// </summary>
        public void GenerateLevel(LevelConfig config)
        {
            // Clean up any existing level
            if (levelRoot != null)
                Destroy(levelRoot);
            
            levelRoot = new GameObject("Level Root");
            
            // Create track
            GenerateTrack(config);
            
            // Create ground
            GenerateGround(config);
            
            // Create stations
            GenerateStations(config);
            
            // Create environment (trees, buildings)
            GenerateEnvironment(config);
            
            // Place train at start
            PlaceTrain();
            
            // Set up weather/environment
            ApplyWeather(config);
        }
        
        private void GenerateTrack(LevelConfig config)
        {
            GameObject trackObj = new GameObject("Track");
            trackObj.transform.parent = levelRoot.transform;
            
            trackSpline = trackObj.AddComponent<TrackSpline>();
            
            if (config.trackControlPoints != null && config.trackControlPoints.Length > 0)
            {
                trackSpline.SetControlPoints(config.trackControlPoints);
            }
            else
            {
                // Generate a default oval track
                int pointCount = 8;
                Vector3[] points = new Vector3[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    float angle = (i / (float)pointCount) * Mathf.PI * 2f;
                    float radiusX = 100f;
                    float radiusZ = 60f;
                    points[i] = new Vector3(Mathf.Cos(angle) * radiusX, 0f, Mathf.Sin(angle) * radiusZ);
                }
                trackSpline.SetControlPoints(points);
            }
        }
        
        private void GenerateGround(LevelConfig config)
        {
            // Flat ground plane with PS1 material
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.parent = levelRoot.transform;
            ground.transform.localScale = new Vector3(10f, 1f, 10f); // 100x100m
            
            if (ps1GroundMaterial != null)
                ground.GetComponent<Renderer>().material = ps1GroundMaterial;
        }
        
        private void GenerateStations(LevelConfig config)
        {
            if (config.stations == null) return;
            
            foreach (var station in config.stations)
            {
                Vector3 stationPos = trackSpline.GetPointAtDistance(station.trackDistance);
                Vector3 stationDir = trackSpline.GetDirectionAtDistance(station.trackDistance);
                Quaternion stationRot = Quaternion.LookRotation(stationDir, Vector3.up);
                
                // Offset station to the side of the track
                Vector3 right = Vector3.Cross(Vector3.up, stationDir).normalized;
                Vector3 stationWorldPos = stationPos + right * 5f;
                
                GameObject stationObj;
                if (stationPrefab != null)
                {
                    stationObj = Instantiate(stationPrefab, stationWorldPos, stationRot);
                }
                else
                {
                    // Create a simple box as placeholder
                    stationObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stationObj.transform.position = stationWorldPos;
                    stationObj.transform.rotation = stationRot;
                    stationObj.transform.localScale = new Vector3(station.platformLength, 2f, 4f);
                    
                    if (ps1StationMaterial != null)
                        stationObj.GetComponent<Renderer>().material = ps1StationMaterial;
                }
                
                stationObj.name = $"Station_{station.stationName}";
                stationObj.transform.parent = levelRoot.transform;
                
                // Add a StationTrigger component for collision detection
                var trigger = stationObj.AddComponent<StationTrigger>();
                trigger.stationName = station.stationName;
                trigger.trackDistance = station.trackDistance;
                trigger.platformLength = station.platformLength;
            }
        }
        
        private void GenerateEnvironment(LevelConfig config)
        {
            // Scatter trees along the track
            for (int i = 0; i < treeCount; i++)
            {
                float distance = (i / (float)treeCount) * trackSpline.GetLength();
                Vector3 trackPos = trackSpline.GetPointAtDistance(distance);
                Vector3 offset = Random.onUnitSphere * Random.Range(10f, 40f);
                offset.y = 0f;
                
                // Don't place too close to stations
                bool tooClose = false;
                if (config.stations != null)
                {
                    foreach (var station in config.stations)
                    {
                        Vector3 stationPos = trackSpline.GetPointAtDistance(station.trackDistance);
                        if (Vector3.Distance(trackPos + offset, stationPos) < 15f)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                }
                
                if (tooClose) continue;
                
                GameObject tree;
                if (treePrefab != null)
                {
                    tree = Instantiate(treePrefab, trackPos + offset, Quaternion.identity);
                }
                else
                {
                    // Simple low-poly tree placeholder: cone on cylinder
                    tree = CreatePlaceholderTree(trackPos + offset);
                }
                
                tree.transform.parent = levelRoot.transform;
                tree.transform.localScale = Vector3.one * Random.Range(0.8f, 1.5f);
            }
        }
        
        private GameObject CreatePlaceholderTree(Vector3 position)
        {
            GameObject tree = new GameObject("Tree");
            tree.transform.position = position;
            
            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.parent = tree.transform;
            trunk.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
            trunk.transform.localPosition = Vector3.up * 1f;
            
            // Canopy (Unity has no Cone primitive — Capsule gives a chunky PS1-tree look)
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            canopy.transform.parent = tree.transform;
            canopy.transform.localScale = new Vector3(2f, 3f, 2f);
            canopy.transform.localPosition = Vector3.up * 3.5f;
            
            if (ps1TreeMaterial != null)
            {
                trunk.GetComponent<Renderer>().material = ps1TreeMaterial;
                canopy.GetComponent<Renderer>().material = ps1TreeMaterial;
            }
            
            return tree;
        }
        
        private void PlaceTrain()
        {
            Vector3 startPos = trackSpline.GetPointAtDistance(0f);
            Vector3 startDir = trackSpline.GetDirectionAtDistance(0f);
            
            GameObject trainObj;
            if (trainPrefab != null)
            {
                trainObj = Instantiate(trainPrefab, startPos, Quaternion.LookRotation(startDir, Vector3.up));
            }
            else
            {
                // Simple box train placeholder
                trainObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trainObj.transform.position = startPos;
                trainObj.transform.rotation = Quaternion.LookRotation(startDir, Vector3.up);
                trainObj.transform.localScale = new Vector3(3f, 2.5f, 8f);
                trainObj.name = "Train";
                
                if (ps1GroundMaterial != null)
                    trainObj.GetComponent<Renderer>().material = ps1GroundMaterial;
            }
            
            trainObj.transform.parent = levelRoot.transform;
            
            // Add required components
            var controller = trainObj.GetComponent<TrainController>();
            if (controller == null)
                controller = trainObj.AddComponent<TrainController>();
        }
        
        private void ApplyWeather(LevelConfig config)
        {
            switch (config.weather)
            {
                case WeatherType.Night:
                case WeatherType.Ghost:
                    RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.2f);
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.15f);
                    RenderSettings.fogDensity = 0.02f;
                    break;
                    
                case WeatherType.Rain:
                case WeatherType.Storm:
                case WeatherType.NightRain:
                    RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.35f);
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = new Color(0.3f, 0.3f, 0.35f);
                    RenderSettings.fogDensity = 0.015f;
                    break;
                    
                case WeatherType.Fog:
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = Color.gray;
                    RenderSettings.fogDensity = 0.04f;
                    break;
                    
                case WeatherType.Snow:
                    RenderSettings.ambientLight = new Color(0.8f, 0.85f, 0.9f);
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = new Color(0.9f, 0.9f, 0.95f);
                    RenderSettings.fogDensity = 0.02f;
                    break;
                    
                default: // Clear
                    RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.35f);
                    RenderSettings.fog = false;
                    break;
            }
        }
    }
    
    /// <summary>
    /// Simple trigger zone for station detection.
    /// </summary>
    public class StationTrigger : MonoBehaviour
    {
        public string stationName;
        public float trackDistance;
        public float platformLength;
        
        private void OnTriggerEnter(Collider other)
        {
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.State == GameManager.GameState.Playing)
            {
                gameManager.OnTrainReachedStation();
            }
        }
    }
}