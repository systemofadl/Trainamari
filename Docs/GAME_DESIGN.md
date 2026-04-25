# Trainamari - Game Design Document

## Elevator Pitch
A wacky, fun train driving game with Katamari Damacy vibes. You drive a train with the Densha de Go lever controller (or keyboard), picking up passengers and cargo on increasingly absurd routes. Smooth driving chains combos, rough handling sends passengers flying. It's not a sim - it's a vibe.

## Core Aesthetic
- PS1 era low-poly with CRT texture warping
- Bright, saturated colors - think Katamari's palette
- Vertex jitter on camera rotation (PS1 wobble)
- Affine texture mapping (no perspective correction)
- Lo-fi charm, not "bad on purpose" - nostalgic and intentional
- J-pop/eletronic soundtrack, upbeat and bouncy
- UI in that chunky PS1 font style

## Core Gameplay Loop
1. Select a route/level
2. Drive the train - accelerate, brake, manage speed through curves
3. Stop at stations (accuracy matters - over/undershoot costs points)
4. Pick up passengers and cargo at stations
5. Chain smooth stops and on-time arrivals for combos
6. Reach the final station with high score

## Scoring
- **Speed bonus**: Maintaining high speed on straights
- **Smooth stop**: How precisely you stop at the platform marker
- **Combo multiplier**: Chain smooth stops without rough handling
- **Cargo integrity**: Fragile cargo arrives undamaged
- **Style points**: Horn at the right time, drifting (yes, train drifting)
- **Time bonus**: Arriving on schedule

## Input
### Densha de Go Controller (Primary)
- Brake lever (positions 1-8 + emergency)
- Accelerate lever (positions 1-5 or similar)
- Horn button
- Door close button

### Keyboard (Fallback)
- W/S or Up/Down: Throttle (hold to increase/decrease)
- A/D or Left/Right: Horn / Door (contextual)
- Space: Emergency brake
- Shift: Door close

### Gamepad (Secondary)
- Left stick or triggers: Throttle
- Face buttons: Horn, doors, etc.

## Levels (10)
1. **Morning Commute** - Suburban line, gentle curves, learn the lever
2. **Rush Hour** - Packed train, tight schedule, lots of stops
3. **Mountain Express** - Steep grades, brake management is critical
4. **Cargo Chaos** - Fragile cargo, penalties for rough handling
5. **Night Rider** - Low visibility, animals on tracks, atmospheric
6. **Festival Express** - Drunken passengers, low grip, party vibes
7. **Storm Runner** - Lightning strikes, flooding, debris on tracks
8. **Cross Country** - Marathon level, multiple fuel/pit stops
9. **Ghost Line** - Supernatural route, apparitions, eerie atmosphere
10. **Final Run** - Everything goes wrong, the ultimate test

## Train Physics
- Momentum-based movement (heavy train feel)
- Braking distance depends on speed + cargo weight
- Over-speed on curves = derailment risk (visual wobble warning first)
- Track-following: train stays on rails, player controls speed and braking
- Slight physics randomness for replayability

## Passengers & Cargo
- Passengers visible through windows (cheering/screaming sprites)
- Cargo types: Standard, Fragile (glass icon), Livestock (escapes on rough ride), Explosive (detonates on hard brake)
- Each station has a passenger/cargo manifest

## Audio
- Satisfying lever clunk sounds
- Brake screech at high deceleration
- Horn (different horns unlockable?)
- Chugging that matches speed
- Station arrival chime
- Crowd cheering/panicking sounds
- Lo-fi electronic soundtrack

## Future Considerations
- PS1 hardware port (SCPH-5501 target)
- Additional controller support (other Densha de Go models)
- Leaderboards
- Unlockable trains and horns
- Multiplayer (tag team driving?)