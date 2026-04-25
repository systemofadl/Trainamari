# Trainamari

A wacky, fun train driving game with Katamari Damacy vibes. Uses the Densha de Go lever controller (or keyboard fallback).

## Quick Start

1. Open this project in Unity 2022.3 LTS (URP template)
2. Open the `Trainamari` scene
3. Hit Play
4. Controls:
   - **W/S** or **Up/Down**: Throttle (accelerate/brake)
   - **Space**: Emergency brake
   - **A**: Horn
   - **D**: Door close

## Densha de Go Controller

Plug in your PS1 Densha de Go controller via USB adapter. The game will auto-detect it. If it shows up as a DirectInput device with "Densha" in the name, it'll work automatically.

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameConstants, GameManager, ScoreManager
│   ├── Train/          # TrainController, TrackSpline, CargoManager
│   ├── Input/          # TrainInput (keyboard/gamepad/Densha)
│   ├── UI/             # TrainHUD
│   ├── Audio/          # TrainAudio
│   └── Level/          # LevelConfig, LevelGenerator, DefaultLevels
├── Shaders/            # PS1Unlit, PS1Lit, CRTPostProcess, TrackShader
├── Materials/          # PS1-style materials
├── Prefabs/            # Train, stations, environment
├── Scenes/             # Game scenes
├── Textures/           # Low-res PS1-style textures
├── Models/             # Low-poly 3D models
├── Animations/         # Animation clips
└── ScriptableObjects/  # Level configs
```

## Design Principles

- **PS1 aesthetic**: Low-poly, vertex jitter, affine textures, CRT shader
- **Heavy feel**: The train has momentum. You can't stop on a dime.
- **Katamari vibes**: Bright colors, absurd scenarios, satisfying combos
- **Controller first**: Densha de Go lever is the primary input. Keyboard works but feels different.
- **Accessibility**: Keyboard/mouse always works. Gamepad supported. Densha de Go is the "premium" experience.

## Scoring

- **Smooth stops**: Park precisely at station markers
- **Speed maintenance**: Keep your speed up on straights
- **Combo chains**: Consecutive smooth stops multiply your score
- **Cargo integrity**: Deliver fragile stuff without breaking it
- **Style**: Horn at crossings, drift through curves (yes, train drifting)

## Levels

1. **Morning Commute** - Learn the lever, gentle route
2. **Rush Hour** - Packed train, tight schedule
3. **Mountain Express** - Steep grades, brake management
4. **Cargo Chaos** - Fragile cargo, penalties for rough handling
5. **Night Rider** - Low visibility, animals on tracks
6. **Festival Express** - Drunk passengers, slick rails
7. **Storm Runner** - Lightning, flooding, debris
8. **Cross Country** - Marathon, multiple stops
9. **Ghost Line** - Supernatural, eerie
10. **FINAL RUN** - Everything goes wrong

## Tech

- Unity 2022.3 LTS (URP)
- C# scripts, custom shaders
- Catmull-Rom spline track system
- DirectInput for Densha de Go controller
- PS1 post-processing stack

## Future

- PS1 hardware port (SCPH-5501 target via PSn00bSDK)
- Additional controller support
- Unlockable trains and horns
- Leaderboards
- Multiplayer?