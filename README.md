# SafeCrack-Numbers

A Unity puzzle game where the player must crack a digital safe by solving number sequences.

## Gameplay

The player is shown four number sequences, one at a time. Each sequence has a missing last  
number that must be identified by recognising the underlying mathematical rule.  
The player has **3 attempts** per sequence. Exhausting all attempts resets the safe.

## Sequences

All sequences are defined using lambda formulas inside `SafeGameManager.cs`:

| Index | Formula | Example output |
|-------|---------|----------------|
| 0 | Triangular offset | 2, 4, 7, 11, 16, ? |
| 1 | Alternating ×3 / +1 | 2, 6, 7, 21, 22, ? |
| 2 | Fibonacci-like | 1, 2, 3, 5, 8, ? |
| 3 | Perfect squares | 1, 4, 9, 16, 25, ? |

## Tech Stack

- **Engine**: Unity 6 (URP)
- **UI**: Runtime-built via code (no prefabs)
- **Input**: Unity Input System
- **Text**: TextMeshPro

## Building

Open the project in Unity 6+, load `Assets/Scenes/SampleScene` and hit Play.
