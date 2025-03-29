# Pixel Animator Documentation

## Overview

Pixel Animator is a Unity tool designed specifically for frame-by-frame 2D pixel art animations. It offers a specialized alternative to Unity's default animation system, focusing on synchronizing frame-by-frame animations with BoxCollider2D components and events. The tool is particularly useful for game developers who want precise control over hitboxes and events during animation playback.

## Key Features

- **Frame-by-frame animation**: Create and manage sprite-based animations with precise timing control
- **BoxCollider2D integration**: Synchronize collision boxes with specific animation frames
- **Event system**: Trigger Unity events at specific frames of the animation
- **Custom editor**: A dedicated editor window for visualizing and editing animations
- **Performance optimization**: Group animations using controllers to improve runtime performance

## Core Components

### Scriptable Objects

1. **PixelAnimation**
   - Stores animation data including sprites, frame rate, and loop settings
   - Contains BoxGroup data that defines collision shapes for each frame
   - Handles sprite-based events

2. **PixelAnimationController**
   - Groups multiple PixelAnimation objects for better performance
   - Acts as a container for related animations used by a specific animator

### MonoBehaviours

1. **PixelAnimator**
   - Main component responsible for playing animations
   - Manages the creation and updating of collision boxes
   - Handles event triggering at specific frames
   - Requires a SpriteRenderer component

2. **ColliderInfo/CollisionInfo**
   - Manage trigger/collision events for BoxCollider2D components
   - Handle method invocation for OnEnter, OnStay, and OnExit events

## Animation Data Structure

### PixelSprite
Represents a single frame in an animation with:
- Sprite reference
- Unique identifier
- Method storage for frame-specific events

### BoxGroup
Represents a collection of collision boxes with:
- Reference to BoxData (name, color, layer, physics material)
- Collision type (Trigger or Collider)
- List of BoxLayers

### BoxLayer
Represents a single collision box across all frames with:
- List of BoxFrames for each animation frame
- Methods to manage frame types (KeyFrame, CopyFrame, EmptyFrame)

### BoxFrame
Represents the state of a collision box on a specific frame with:
- Rect data (position, size)
- Frame type (KeyFrame, CopyFrame, EmptyFrame)
- Event method storage for OnEnter, OnStay, and OnExit events

## Event System

Pixel Animator provides a flexible event system that allows:

1. **Sprite-based events**: Execute methods when a specific sprite is displayed
2. **Collision-based events**: Trigger methods during collision/trigger interactions (Enter, Stay, Exit)

The event system uses:
- **MethodStorage**: Stores and manages Unity events
- **MethodData**: Serializes method information and parameters
- **SerializableData**: Handles serialization of different data types

## Getting Started

### Installation

Add the package to your Unity project using the Package Manager with this Git URL:
```
https://github.com/biinci/PixelAnimator.git
```

### Creating an Animation

1. Create a new PixelAnimation asset:
   - Navigate to `Assets > Create > Pixel Animation > New Animation`
   - Add sprites to the "Pixel Sprites" field
   
2. Open the PixelAnimator window:
   - Go to `Window > PixelAnimator`
   
3. Configure your animation:
   - Set animation properties (FPS, loop)
   - Add sprite-based events if needed
   - Configure collision boxes through the timeline interface

### Setting Up Collision Boxes

1. Open animation preferences:
   - Click the burger menu in the timeline and select "Go to preferences"
   - Configure box types (name, color, layer, physics material)
   
2. Add a box group to your animation:
   - Use the burger menu to add a box group
   - Configure the collision type (Trigger or Collider)
   
3. Design hitboxes:
   - Adjust box positions and sizes for each keyframe
   - Set frame types (KeyFrame, CopyFrame, EmptyFrame)
   - Add collision events if needed

### Using the Animation

1. Create a new PixelAnimationController:
   - Navigate to `Assets > Create > Pixel Animation > New Animation Controller`
   - Add your animations to the controller
   
2. Set up a GameObject:
   - Add a SpriteRenderer component
   - Add the PixelAnimator component
   - Assign the SpriteRenderer reference
   - Assign the PixelAnimationController reference
   
3. Play animations in code:
   ```csharp
   [SerializeField] private PixelAnimator animator;
   [SerializeField] private PixelAnimation idleAnimation;
   
   void Start() {
       animator.Play(idleAnimation);
   }
   ```

## Advanced Features

### Frame Types

- **KeyFrame**: Defines hitbox data for a specific frame
- **CopyFrame**: Copies hitbox data from the previous KeyFrame
- **EmptyFrame**: No hitbox data (box is disabled)

### Event Types

1. **Sprite Events**:
   - Triggered when a specific sprite is displayed
   - Can call any method on components attached to the same GameObject

2. **Collision Events**:
   - **OnEnter**: Called when collision/trigger begins
   - **OnStay**: Called while collision/trigger is active
   - **OnExit**: Called when collision/trigger ends

## Best Practices

1. **Animation Organization**:
   - Group related animations in the same controller
   - Name animations clearly for easier management
   
2. **Performance Optimization**:
   - Use CopyFrames when hitboxes don't change between frames
   - Only add events when necessary
   - Consider using fewer, more versatile hitboxes

3. **Workflow Efficiency**:
   - Configure box preferences before creating animations
   - Create hitbox templates for common patterns
   - Use the timeline view to visualize animation flow

## Troubleshooting

### Common Issues

1. **Events not firing**:
   - Ensure components referenced in events exist on the GameObject
   - Check that collision layers are set up correctly
   - Verify isTrigger settings match your event type

2. **Hitboxes not appearing**:
   - Make sure the animation is playing
   - Check that box groups are properly configured
   - Confirm frame types are set correctly

3. **Animation not playing**:
   - Verify the SpriteRenderer reference is assigned
   - Check that the animation controller contains your animation
   - Ensure the Play method is being called

## Extending The System

Pixel Animator uses a modular architecture that allows for extension:

1. **Custom Event Types**:
   - Extend BaseMethodStorage and BaseMethodData
   - Register new types with the serialization system

2. **Additional Animation Features**:
   - Add properties to PixelAnimation
   - Extend the PixelAnimator to handle new features

3. **Editor Enhancements**:
   - Create custom editors for new components
   - Add tools to the PixelAnimator window
