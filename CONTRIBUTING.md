
# Contributing Guide

Thank you for considering contributing to **PixelAnimator**!
This guide explains coding conventions, and how to submit changes.

## Getting Started

* Unity version: Developed and tested with Unity 2023.2 LTS or later.
* .NET / C#: Use Unity’s default C# runtime.
* IDE: Visual Studio, Rider, or VS Code recommended.

## Project Structure

* Runtime/ → Core runtime components (PixelAnimator, PixelAnimation, PixelAnimationController).
* Editor/ → Custom Editor windows, inspectors, utilities.
* Tests/ → EditMode and PlayMode tests (if any).

## Coding Style

* Follow Unity C# conventions:

    * PascalCase for classes/properties.
    * camelCase for private fields.
* Avoid leaving Debug.Log statements in production code.
* Wrap editor-only code in `#if UNITY_EDITOR`.
* Resolve TODO comments before submitting a PR if possible.

## Commit Messages

We follow Conventional Commits ([https://www.conventionalcommits.org/](https://www.conventionalcommits.org/)):

* feat: new feature
* fix: bug fix
* refactor: code change that doesn’t affect behavior
* docs: documentation only
* test: adding or updating tests
* chore: build, CI, or other maintenance

Examples:

feat: add playback speed control to PixelAnimator
fix: correct collider refresh when looping animations

## Issues

When opening an issue:

* For bugs: Include Unity version, reproduction steps, expected behavior, actual behavior, screenshots/GIFs if helpful.
* For feature requests: Explain the motivation, proposed solution, and alternatives considered.

## Pull Requests

1. Fork the repo and create a new branch: feature/your-feature or fix/your-bug.
2. Keep changes small and focused.
3. Add tests if possible.
4. Write a clear PR description with:

    * What changed
    * Why it changed
    * How it was tested
5. PRs will be reviewed before merging. Squash merges are preferred.

## Code of Conduct

By participating in this project, you agree to follow the Code of Conduct