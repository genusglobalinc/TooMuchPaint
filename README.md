# Too Much Paint

A fun painting game where you need to paint portraits of NPCs before your paint runs out!

## Game Concept

You've ordered way too much paint (500 trucks worth!) and now you need to use it all before it overflows and goes to waste. NPCs will come to you with their portrait requests, and you'll need to paint them quickly and accurately to keep your paint supply from running out.

## Core Game Loop

1. NPC approaches and shows their face attributes for a limited time
2. Player pulls out a canvas and tries to recreate the attributes "MS Paint style"
3. NPC approves or rejects the painting
4. Success gives you more paint, failure makes you lose paint
5. Game ends when you run out of paint

## Project Structure

- **Assets/Scripts/**
  - `GameManager.cs` - Manages game state and flow
  - `NPCController.cs` - Handles NPC behavior and attributes
  - `PlayerController.cs` - Handles player input and painting
  - `UIManager.cs` - Manages all UI interactions

## Setup Instructions

1. Open the project in Unity (2022.3 or later recommended)
2. Set up the following GameObjects in your scene:
   - A Canvas with UI elements (start screen, game UI, game over screen)
   - A Player object with the PlayerController component
   - An NPC object with the NPCController component
   - A Camera for the painting view
3. Assign all necessary references in the Unity Inspector
4. Press Play to test the game

## Controls

- **Left Mouse Button**: Paint on the canvas
- **Number Keys 1-9**: Select different colors
- **Enter**: Submit your painting
- **C**: Clear the canvas

## How to Play

1. Click "Start Game" on the main menu
2. Watch the NPC's attributes carefully when they appear
3. Use your mouse to paint their portrait
4. Try to match the attributes as closely as possible
5. Submit your painting before time runs out
6. Keep your paint meter from emptying by making accurate paintings

## Future Improvements

- Add more NPC attributes and variations
- Implement a more sophisticated painting recognition system
- Add sound effects and music
- Create a tutorial level
- Add different game modes
- Implement a scoring system based on accuracy
