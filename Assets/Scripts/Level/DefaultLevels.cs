using UnityEngine;
using Trainamari.Core;
using Trainamari.Train;
using Trainamari.Level;

namespace Trainamari.Core
{
    /// <summary>
    /// Default level configurations for all 10 levels.
    /// These are code-generated configs that can be replaced with
    /// ScriptableObject assets in the Unity editor for easier tuning.
    /// </summary>
    public static class DefaultLevels
    {
        public static LevelConfig[] GetAllLevels()
        {
            return new LevelConfig[]
            {
                CreateLevel1(),
                CreateLevel2(),
                CreateLevel3(),
                CreateLevel4(),
                CreateLevel5(),
                CreateLevel6(),
                CreateLevel7(),
                CreateLevel8(),
                CreateLevel9(),
                CreateLevel10()
            };
        }
        
        // Level 1: Morning Commute - gentle, learn the lever
        private static LevelConfig CreateLevel1()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Morning Commute";
            config.levelDescription = "Easy does it. Learn the lever, hit your stops.";
            config.levelNumber = 1;
            config.timeLimit = 300f;
            config.hasTimeLimit = false;
            config.weather = WeatherType.Clear;
            config.trackGrip = 1f;
            config.allowDerailment = false; // No derailment in tutorial
            config.scoreForStar1 = 500;
            config.scoreForStar2 = 1500;
            config.scoreForStar3 = 3000;
            
            // Gentle oval track
            config.trackControlPoints = CreateOvalTrack(100f, 60f, 16);
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Maple Grove", trackDistance = 0f, platformLength = 30f, passengersWaiting = 10, passengersExiting = 0 },
                new StationDefinition { stationName = "Oak Street", trackDistance = 80f, platformLength = 25f, passengersWaiting = 15, passengersExiting = 5 },
                new StationDefinition { stationName = "Riverside", trackDistance = 200f, platformLength = 25f, passengersWaiting = 8, passengersExiting = 10 },
                new StationDefinition { stationName = "Elm Park", trackDistance = 320f, platformLength = 30f, passengersWaiting = 5, passengersExiting = 15, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 2: Rush Hour - packed, tight schedule
        private static LevelConfig CreateLevel2()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Rush Hour";
            config.levelDescription = "The 8:05 to Downtown. Don't be late.";
            config.levelNumber = 2;
            config.timeLimit = 240f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Clear;
            config.trackGrip = 1f;
            config.speedMultiplier = 1.2f;
            config.scoreForStar1 = 1000;
            config.scoreForStar2 = 2500;
            config.scoreForStar3 = 4000;
            
            config.trackControlPoints = CreateOvalTrack(120f, 80f, 20);
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Suburban Station", trackDistance = 0f, platformLength = 30f, passengersWaiting = 50, passengersExiting = 0 },
                new StationDefinition { stationName = "Midtown", trackDistance = 70f, platformLength = 25f, passengersWaiting = 30, passengersExiting = 15 },
                new StationDefinition { stationName = "Commerce Ave", trackDistance = 160f, platformLength = 25f, passengersWaiting = 20, passengersExiting = 25 },
                new StationDefinition { stationName = "Financial District", trackDistance = 280f, platformLength = 20f, passengersWaiting = 10, passengersExiting = 40 },
                new StationDefinition { stationName = "Downtown Terminal", trackDistance = 400f, platformLength = 35f, passengersWaiting = 0, passengersExiting = 30, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 3: Mountain Express - steep grades, brake management
        private static LevelConfig CreateLevel3()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Mountain Express";
            config.levelDescription = "Thin air, steep grades. Brake smart or brake hard.";
            config.levelNumber = 3;
            config.timeLimit = 360f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Clear;
            config.trackGrip = 0.85f;
            config.curveIntensity = 1.5f;
            config.scoreForStar1 = 1500;
            config.scoreForStar2 = 3000;
            config.scoreForStar3 = 5000;
            
            config.trackControlPoints = CreateMountainTrack();
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Valley Base", trackDistance = 0f, platformLength = 25f, passengersWaiting = 20, passengersExiting = 0 },
                new StationDefinition { stationName = "Pine Ridge", trackDistance = 120f, platformLength = 20f, passengersWaiting = 10, passengersExiting = 5 },
                new StationDefinition { stationName = "Summit Lodge", trackDistance = 280f, platformLength = 20f, passengersWaiting = 5, passengersExiting = 15 },
                new StationDefinition { stationName = "Alpine Village", trackDistance = 450f, platformLength = 25f, passengersWaiting = 0, passengersExiting = 10, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 4: Cargo Chaos - fragile cargo, penalties
        private static LevelConfig CreateLevel4()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Cargo Chaos";
            config.levelDescription = "Glass, livestock, and... explosives. Handle with care.";
            config.levelNumber = 4;
            config.timeLimit = 300f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Clear;
            config.scoreForStar1 = 1000;
            config.scoreForStar2 = 2500;
            config.scoreForStar3 = 4500;
            
            config.trackControlPoints = CreateOvalTrack(100f, 70f, 16);
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Industrial Park", trackDistance = 0f, platformLength = 30f, passengersWaiting = 5, passengersExiting = 0, cargoType = CargoType.Fragile, cargoName = "Crystal Vases", cargoPoints = 200 },
                new StationDefinition { stationName = "Farm District", trackDistance = 100f, platformLength = 25f, passengersWaiting = 3, passengersExiting = 2, cargoType = CargoType.Livestock, cargoName = "Prize Chickens", cargoPoints = 150 },
                new StationDefinition { stationName = "Chemical Plant", trackDistance = 220f, platformLength = 20f, passengersWaiting = 0, passengersExiting = 3, cargoType = CargoType.Explosive, cargoName = "Fireworks (Handle with Care)", cargoPoints = 300 },
                new StationDefinition { stationName = "City Market", trackDistance = 350f, platformLength = 35f, passengersWaiting = 0, passengersExiting = 3, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 5: Night Rider - low visibility
        private static LevelConfig CreateLevel5()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Night Rider";
            config.levelDescription = "You can barely see the tracks. Trust the lever.";
            config.levelNumber = 5;
            config.timeLimit = 300f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Night;
            config.visibility = 0.4f;
            config.scoreForStar1 = 1200;
            config.scoreForStar2 = 2800;
            config.scoreForStar3 = 5000;
            
            config.trackControlPoints = CreateOvalTrack(110f, 65f, 18);
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Depot", trackDistance = 0f, platformLength = 30f, passengersWaiting = 8, passengersExiting = 0 },
                new StationDefinition { stationName = "Crossroads", trackDistance = 90f, platformLength = 20f, passengersWaiting = 5, passengersExiting = 3 },
                new StationDefinition { stationName = "Old Mill", trackDistance = 200f, platformLength = 20f, passengersWaiting = 3, passengersExiting = 5 },
                new StationDefinition { stationName = "Harbor", trackDistance = 330f, platformLength = 25f, passengersWaiting = 0, passengersExiting = 5, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 6: Festival Express - party vibes
        private static LevelConfig CreateLevel6()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Festival Express";
            config.levelDescription = "Everyone's drunk. The rails are slick. Party on?";
            config.levelNumber = 6;
            config.timeLimit = 280f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Clear;
            config.trackGrip = 0.7f; // Wet rails from... rain. Definitely rain.
            config.scoreForStar1 = 1500;
            config.scoreForStar2 = 3000;
            config.scoreForStar3 = 5500;
            
            config.trackControlPoints = CreateOvalTrack(100f, 75f, 16);
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Festival Gate", trackDistance = 0f, platformLength = 35f, passengersWaiting = 40, passengersExiting = 0, cargoType = CargoType.Standard, cargoName = "Kegs of Beer", cargoPoints = 100 },
                new StationDefinition { stationName = "Dance Floor Central", trackDistance = 80f, platformLength = 25f, passengersWaiting = 25, passengersExiting = 10 },
                new StationDefinition { stationName = "Food Court", trackDistance = 180f, platformLength = 25f, passengersWaiting = 15, passengersExiting = 15, cargoType = CargoType.Fragile, cargoName = "Birthday Cake", cargoPoints = 150 },
                new StationDefinition { stationName = "Home Base", trackDistance = 310f, platformLength = 30f, passengersWaiting = 0, passengersExiting = 40, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 7: Storm Runner
        private static LevelConfig CreateLevel7()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Storm Runner";
            config.levelDescription = "Lightning, flooding, debris. Just another day on the job.";
            config.levelNumber = 7;
            config.timeLimit = 350f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Storm;
            config.trackGrip = 0.6f;
            config.visibility = 0.5f;
            config.hasObstacles = true;
            config.scoreForStar1 = 2000;
            config.scoreForStar2 = 4000;
            config.scoreForStar3 = 6000;
            
            config.trackControlPoints = CreateOvalTrack(130f, 80f, 20);
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Shelter Bay", trackDistance = 0f, platformLength = 25f, passengersWaiting = 20, passengersExiting = 0 },
                new StationDefinition { stationName = "Flood Plain", trackDistance = 100f, platformLength = 20f, passengersWaiting = 10, passengersExiting = 5 },
                new StationDefinition { stationName = "Lightning Ridge", trackDistance = 230f, platformLength = 20f, passengersWaiting = 5, passengersExiting = 10 },
                new StationDefinition { stationName = "Safe Harbor", trackDistance = 380f, platformLength = 30f, passengersWaiting = 0, passengersExiting = 15, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 8: Cross Country - marathon
        private static LevelConfig CreateLevel8()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Cross Country";
            config.levelDescription = "The long haul. Pace yourself.";
            config.levelNumber = 8;
            config.timeLimit = 600f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Clear;
            config.scoreForStar1 = 3000;
            config.scoreForStar2 = 6000;
            config.scoreForStar3 = 10000;
            
            config.trackControlPoints = CreateLongTrack();
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "West End", trackDistance = 0f, platformLength = 30f, passengersWaiting = 30, passengersExiting = 0 },
                new StationDefinition { stationName = "Plains Junction", trackDistance = 150f, platformLength = 25f, passengersWaiting = 20, passengersExiting = 10 },
                new StationDefinition { stationName = "River Crossing", trackDistance = 320f, platformLength = 25f, passengersWaiting = 15, passengersExiting = 15 },
                new StationDefinition { stationName = "Mountain Pass", trackDistance = 500f, platformLength = 20f, passengersWaiting = 10, passengersExiting = 20 },
                new StationDefinition { stationName = "Lakeside", trackDistance = 700f, platformLength = 25f, passengersWaiting = 10, passengersExiting = 15 },
                new StationDefinition { stationName = "East Terminal", trackDistance = 900f, platformLength = 35f, passengersWaiting = 0, passengersExiting = 15, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 9: Ghost Line - supernatural
        private static LevelConfig CreateLevel9()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "Ghost Line";
            config.levelDescription = "The last train runs at midnight. Some say it never arrives.";
            config.levelNumber = 9;
            config.timeLimit = 300f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Ghost;
            config.visibility = 0.3f;
            config.trackGrip = 0.8f;
            config.curveIntensity = 1.3f;
            config.scoreForStar1 = 2000;
            config.scoreForStar2 = 4500;
            config.scoreForStar3 = 7000;
            
            config.trackControlPoints = CreateOvalTrack(90f, 55f, 24); // tighter, twistier
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Abandoned Platform", trackDistance = 0f, platformLength = 20f, passengersWaiting = 3, passengersExiting = 0, cargoType = CargoType.Mystery, cargoName = "Old Luggage", cargoPoints = 200 },
                new StationDefinition { stationName = "Whispering Tunnel", trackDistance = 70f, platformLength = 15f, passengersWaiting = 2, passengersExiting = 1 },
                new StationDefinition { stationName = "The Crossing", trackDistance = 160f, platformLength = 15f, passengersWaiting = 1, passengersExiting = 2, cargoType = CargoType.Mystery, cargoName = "Locked Box", cargoPoints = 300 },
                new StationDefinition { stationName = "Final Stop", trackDistance = 280f, platformLength = 20f, passengersWaiting = 0, passengersExiting = 3, isFinalStation = true }
            };
            
            return config;
        }
        
        // Level 10: Final Run - everything goes wrong
        private static LevelConfig CreateLevel10()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelName = "FINAL RUN";
            config.levelDescription = "Everything. Goes. Wrong.";
            config.levelNumber = 10;
            config.timeLimit = 360f;
            config.hasTimeLimit = true;
            config.weather = WeatherType.Storm;
            config.visibility = 0.5f;
            config.trackGrip = 0.5f;
            config.curveIntensity = 1.8f;
            config.hasObstacles = true;
            config.scoreForStar1 = 3000;
            config.scoreForStar2 = 6000;
            config.scoreForStar3 = 10000;
            
            config.trackControlPoints = CreateChaoticTrack();
            
            config.stations = new StationDefinition[]
            {
                new StationDefinition { stationName = "Last Chance Depot", trackDistance = 0f, platformLength = 25f, passengersWaiting = 30, passengersExiting = 0, cargoType = CargoType.Explosive, cargoName = "Nitroglycerin (NO SERIOUSLY)", cargoPoints = 500 },
                new StationDefinition { stationName = "Flood Zone", trackDistance = 80f, platformLength = 20f, passengersWaiting = 15, passengersExiting = 5 },
                new StationDefinition { stationName = "Wind Gap", trackDistance = 170f, platformLength = 15f, passengersWaiting = 10, passengersExiting = 10, cargoType = CargoType.Fragile, cargoName = "Great-Grandma's China", cargoPoints = 400 },
                new StationDefinition { stationName = "Debris Alley", trackDistance = 280f, platformLength = 15f, passengersWaiting = 5, passengersExiting = 10, cargoType = CargoType.Livestock, cargoName = "Race Horses", cargoPoints = 350 },
                new StationDefinition { stationName = "The Light", trackDistance = 420f, platformLength = 30f, passengersWaiting = 0, passengersExiting = 30, isFinalStation = true }
            };
            
            return config;
        }
        
        // Track generation helpers
        private static Vector3[] CreateOvalTrack(float radiusX, float radiusZ, int points)
        {
            Vector3[] controlPoints = new Vector3[points];
            for (int i = 0; i < points; i++)
            {
                float angle = (i / (float)points) * Mathf.PI * 2f;
                controlPoints[i] = new Vector3(Mathf.Cos(angle) * radiusX, 0f, Mathf.Sin(angle) * radiusZ);
            }
            return controlPoints;
        }
        
        private static Vector3[] CreateMountainTrack()
        {
            return new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(40f, 5f, 30f),
                new Vector3(80f, 15f, 50f),
                new Vector3(120f, 25f, 40f),
                new Vector3(160f, 35f, 20f),
                new Vector3(200f, 45f, 0f),      // summit
                new Vector3(240f, 35f, -30f),
                new Vector3(280f, 20f, -50f),
                new Vector3(320f, 10f, -40f),
                new Vector3(360f, 5f, -20f),
                new Vector3(400f, 0f, 0f),
                new Vector3(200f, 0f, -30f),      // return leg
            };
        }
        
        private static Vector3[] CreateLongTrack()
        {
            return new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(100f, 0f, 30f),
                new Vector3(200f, 5f, 60f),
                new Vector3(300f, 0f, 30f),
                new Vector3(400f, -2f, 0f),
                new Vector3(500f, 5f, -40f),
                new Vector3(600f, 10f, -60f),
                new Vector3(700f, 5f, -30f),
                new Vector3(800f, 0f, 0f),
                new Vector3(900f, 0f, 0f),
            };
        }
        
        private static Vector3[] CreateChaoticTrack()
        {
            return new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(30f, 0f, 40f),
                new Vector3(80f, 3f, 20f),
                new Vector3(120f, 0f, -30f),
                new Vector3(160f, 5f, -50f),
                new Vector3(200f, 2f, -20f),
                new Vector3(250f, 8f, 30f),
                new Vector3(300f, 3f, 60f),
                new Vector3(340f, 0f, 30f),
                new Vector3(380f, 5f, -10f),
                new Vector3(420f, 0f, 0f),
            };
        }
    }
}