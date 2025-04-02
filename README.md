# Pixel Animator
## Complete Documentation

<div align="center">
  <img width="180" alt="Pixel Animator logo" src="https://github.com/user-attachments/assets/bfd74fa0-289d-459a-9a33-fec06b99d303">
  <br>
  <a href="https://unity.com/releases/editor/whats-new/2023.2.20#installs">
    <img src="https://img.shields.io/badge/Unity-2023.2.7f1%2b-blue?logo=unity">
  </a>
</div>

---

## Table of Contents
1. [Introduction](#introduction)
2. [Key Features](#key-features)
3. [Getting Started](#getting-started)
    - [Installation](#installation)
    - [Core Components](#core-components)
4. [Creating Animations](#creating-animations)
    - [Setting Up a Pixel Animation](#setting-up-a-pixel-animation)
    - [Working with Sprite Events](#working-with-sprite-events)
    - [Using Box Colliders](#using-box-colliders)
    - [Box Types and Properties](#box-types-and-properties)
5. [Animation Controller](#animation-controller)
6. [Runtime Implementation](#runtime-implementation)
    - [Setting Up the Animator](#setting-up-the-animator)
    - [Playing Animations](#playing-animations)
7. [Event System](#event-system)
    - [Sprite Events](#sprite-events)
    - [Box Collision Events](#box-collision-events)
8. [Technical Architecture](#technical-architecture)
    - [Animation Data Structure](#animation-data-structure)
    - [Box Collider System](#box-collider-system)
    - [Event Handling](#event-handling)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

---

## Introduction

Pixel Animator is a specialized Unity tool designed for frame-by-frame 2D pixel art animations. Inspired by AdamCYounis's RetroBox, this tool focuses on synchronizing frame animations with BoxCollider2D components and events, providing a more intuitive and less complex alternative to Unity's built-in animator.

The primary goal of Pixel Animator is to create a direct causal relationship between sprite animations and collision boxes while adding flexible event triggering that works in perfect sync with your animation frames.

## Key Features

- **Simplified Animation Workflow**: Avoids the complexity of Unity's animation state machine for frame-by-frame animations
- **Box Collider Synchronization**: Automatically updates BoxCollider2D objects in sync with animation frames
- **Flexible Event System**: Add events to specific frames or box colliders that trigger at precise moments
- **Visual Editor**: User-friendly editor window for creating and managing animations
- **Performance Optimization**: Group animations with controllers for more efficient resource usage

## Getting Started

### Installation

You can add this package to your Unity project in two ways:

1. **Using Unity Package Manager**:
   - Open Window > Package Manager
   - Click the "+" button > "Add package from git URL..."
   - Enter: `https://github.com/biinci/PixelAnimator.git`

2. **Manual Installation**:
   - Clone or download the repository
   - Copy the files into your Unity project's Assets folder

### Core Components

Pixel Animator works with three primary components:

1. **PixelAnimation** (ScriptableObject)
   - Contains the animation data including sprites, frame rate, and collider information
   - Created via Assets > Create > Pixel Animation > New Animation

2. **PixelAnimationController** (ScriptableObject)
   - Groups animations for performance optimization
   - Created via Assets > Create > Pixel Animation > New Animation Controller

3. **PixelAnimator** (MonoBehaviour)
   - The runtime component that plays animations and handles collisions
   - Attached to GameObjects with SpriteRenderer components

## Creating Animations

### Setting Up a Pixel Animation

1. Create a new PixelAnimation asset:
   - Right-click in the Project window
   - Select Create > Pixel Animation > New Animation
   - Name your animation (e.g., "PlayerIdle")

2. Configure basic settings:
   - **Loop**: Toggle whether the animation should loop
   - **FPS**: Set the frames per second rate
   
3. Add sprites:
   - Drag your sprite frames to the **Pixel Sprites** section
   - Ensure sprites are in correct sequential order

### Working with Sprite Events

You can add events that trigger when specific frames are displayed:

1. Open the Pixel Animator window:
   - Navigate to Window > Pixel Animator
   
2. Select your animation asset

3. Add sprite-based events:
   - In the editor window, select the "Sprite" tab
   - Click "+" to add a new event
   - First dropdown: Select the component type (script) to call a method from
   - Second dropdown: Select the method to call when the sprite is shown

### Using Box Colliders

Pixel Animator allows you to create and manage BoxCollider2D objects that sync with animation frames:

1. Configure box types:
   - In the Pixel Animator window, click the burger menu (☰) and select "Go to preferences"
   - Set up box types with their properties (color, name, layer, physics material)

2. Add a box group to your animation:
   - Return to the animation editor
   - Click the burger menu again and select "Add Box Group"
   - Choose the box type you want to use

3. Configure the box group:
   - Set the collision type (Trigger or Collider)
   - Use the box group buttons to customize its behavior

4. Edit box colliders:
   - Select frames in the timeline
   - Adjust the size and position of box colliders in the editor
   - Use frame type options (KeyFrame, CopyFrame, EmptyFrame) to control how boxes behave across frames

### Box Types and Properties

Each box type can have the following properties:

- **Color**: Visual color in the editor (not visible during runtime)
- **Name**: Identifier for the box type
- **Layer**: Unity layer the BoxCollider2D GameObject will use
- **Physics Material**: Optional PhysicsMaterial2D to apply to the colliders

Frame types for boxes:
- **KeyFrame**: A frame with a defined box position/size
- **CopyFrame**: Inherits box properties from the previous KeyFrame
- **EmptyFrame**: No box collider appears on this frame

## Animation Controller

The PixelAnimationController improves performance by grouping animations:

1. Create a controller:
   - Create > Pixel Animation > New Animation Controller
   
2. Add animations:
   - Drag your PixelAnimation assets to the controller's list

3. Set the controller:
   - Assign the controller to your PixelAnimator component

## Runtime Implementation

### Setting Up the Animator

1. Add the PixelAnimator component:
   - Attach to a GameObject with a SpriteRenderer
   - Assign the SpriteRenderer to the component's reference
   - Assign a PixelAnimationController containing your animations

2. When the PixelAnimator initializes:
   - It loads animation preferences
   - Creates container objects for colliders
   - Compiles all event functions

### Playing Animations

To play an animation in code:

```csharp
// Reference to your animator component
private PixelAnimator pixelAnimator;

// Reference to animation, could be from controller or direct
public PixelAnimation idleAnimation;

void Start() {
    pixelAnimator = GetComponent<PixelAnimator>();
    pixelAnimator.Play(idleAnimation);
}
```

When an animation plays:
1. The appropriate sprite is displayed
2. Box colliders are created/updated based on the current frame
3. Frame events are triggered

## Event System

### Sprite Events

Sprite events trigger when a specific frame is shown:

1. Events are defined using UnityEvents
2. You can select any component on the GameObject and call its methods
3. Methods are compiled at runtime for performance

### Box Collision Events

Box colliders can trigger events on collision/trigger interactions:

1. Each box can have three event types:
   - **OnEnter**: Triggered when collision begins
   - **OnStay**: Triggered while collision continues
   - **OnExit**: Triggered when collision ends

2. Event behavior is determined by the box group's collision type:
   - **Trigger**: Uses OnTriggerEnter2D/Stay2D/Exit2D with Collider2D parameter
   - **Collider**: Uses OnCollisionEnter2D/Stay2D/Exit2D with Collision2D parameter

## Technical Architecture

### Animation Data Structure

Pixel Animator uses a carefully designed data structure:

- **PixelSprite**: Contains a sprite and associated method storage
- **BoxGroup**: Groups related BoxCollider2D objects with shared properties
- **BoxLayer**: Collection of BoxFrames across the animation timeline
- **BoxFrame**: Per-frame box collider data including position, size, and events

### Box Collider System

During runtime:

1. The PixelAnimator creates a container GameObject for colliders
2. For each box group in the animation, a child GameObject is created
3. BoxCollider2D components are added based on the animation data
4. Appropriate handlers (ColliderInfo or CollisionInfo) are attached
5. As frames change, box collider properties update automatically

### Event Handling

Events use a sophisticated serialization system:

1. **MethodStorage**: Contains UnityEvents and MethodData structures
2. **MethodData**: Serializes component references, methods, and parameters
3. At runtime, functions are compiled using expression trees for performance
4. Parameters are preserved through serialization with SerializableData<T>

## Best Practices

1. **Organize animations by state**:
   - Create separate animations for different character states (idle, run, jump, etc.)

2. **Group related animations in controllers**:
   - Put all player animations in one controller, enemy animations in another, etc.

3. **Keep FPS consistent**:
   - Use the same FPS across similar animations to maintain visual coherence

4. **Optimize box colliders**:
   - Use the minimum number of box colliders needed
   - Utilize frame types (especially CopyFrame) to reduce redundancy

5. **Use layers effectively**:
   - Assign appropriate layers to different box types to control collision detection

## Troubleshooting

Common issues and solutions:

1. **Animation doesn't play**:
   - Ensure SpriteRenderer is properly assigned
   - Check that sprites are added to the PixelAnimation
   - Verify that Play() is being called

2. **Box colliders not appearing**:
   - Check if box groups are properly set up
   - Make sure boxes are created as KeyFrames
   - Verify box size is greater than zero

3. **Events not firing**:
   - Ensure methods have compatible parameters
   - Check component references
   - Make sure collision layers are set up correctly

4. **Performance issues**:
   - Use PixelAnimationController to group animations
   - Minimize the number of box colliders
   - Reduce event complexity

---

This documentation covers the Pixel Animator tool developed for Unity frame-by-frame animations with synchronized box colliders. For further assistance or to contribute to the project, visit the GitHub repository at https://github.com/biinci/PixelAnimator.
