# FPS Project

A Unity 6 first-person movement prototype built with the Universal Render Pipeline
and the Input System package. The current scope is a movement sandbox rather than
a complete shooter.

## Requirements

- Unity 6000.3.15f1
- Git LFS (the Mixamo FBX assets are stored through LFS)

Open `Assets/Scenes/SampleScene.unity` and enter Play Mode.

## Controls

| Action | Keyboard and mouse | Gamepad |
| --- | --- | --- |
| Move | WASD or arrow keys | Left stick |
| Look | Mouse | Right stick |
| Jump | Space | South button |
| Crouch | C, Left Ctrl, or Right Ctrl | East button |
| Sprint | Left Shift | Left-stick press |
| Fire | Left mouse button | Right trigger |
| Release cursor | Escape | — |
| Capture cursor | Left click | — |

## Runtime structure

- `PlayerInput` reads the shared `InputSystem_Actions` asset.
- `PlayerLocomotion` owns movement, grounding, jumping, gravity, and sprint state.
- `PlayerCrouch` changes the character capsule and prevents standing under obstacles.
- `MouseLook` rotates the player and camera.
- `PlayerCamera` applies state-based camera offsets in `LateUpdate`.
- `PlayerAnimationController` updates the Animator after movement has finished.

The project uses one build scene: `Assets/Scenes/SampleScene.unity`.

The sandbox contains three yellow humanoid guards grouped under `Enemies`. They
currently patrol short routes using the same humanoid rig and movement animations
as the player. Enemy hitboxes are attached to the animated skeleton: a headshot is
lethal, a torso hit deals 50 damage, and a limb hit deals 25 damage.

## Current scope

Implemented: first-person movement, variable-height jumping, coyote time, jump
buffering, crouching with overhead clearance, sprinting, basic character animation,
raycast shooting, reusable health/damage, body-part damage, three walking enemy
guards, gunshot and impact noise events, a test target, and an input debug overlay.

Not implemented yet: enemy perception or combat, production weapon presentation,
objectives, audio, saving, and final menus. These systems should be added as
separate features rather than folded into the movement components.
