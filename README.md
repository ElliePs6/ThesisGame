
# ThesisGame — VR Counseling Skills Training

A Unity virtual reality prototype developed for an undergraduate thesis comparing **static, rule-based AI** with **adaptive, case-based AI** in a counseling-skills training scenario.

The player interacts with a virtual client in a simulated office and selects how to respond to situations that may arise during a counseling conversation. The project examines how different AI approaches affect the client's responses, the feedback given to the player, and the overall experience.

## Research Focus

**Research question:** How does a static, scripted virtual client compare with an adaptive virtual client in a VR counseling-skills training experience?

The two modes are:

| Static AI | Adaptive AI |
| --- | --- |
| Uses predefined evaluation logic and outcome data. | Uses case-based reasoning (CBR) to match the current situation and player action to a case library. |
| Applies the defined influence change for the selected outcome. | Can adjust influence based on response time, correctness, and tracked behavior. |
| Displays the predefined NPC response for the selected outcome. | Can select an alternative NPC line for sufficiently strong case matches, with a fallback to the predefined response. |
| Does not use adaptive session memory or difficulty reordering. | Can refer to previously resolved contexts and reorder remaining rounds based on player performance. |

Both modes share the same round and action framework, allowing their behavior to be compared within the same prototype.

## Gameplay

The player is presented with a situation involving a virtual client and chooses one of four actions:

- **Probe (Διευκρίνιση):** Ask for clarification or explore missing information.
- **Support (Σταθεροποίηση):** Acknowledge emotions and help stabilize the interaction.
- **Challenge (Αντιπαράθεση):** Address a contradiction or challenge an unhelpful pattern.
- **Commit (Τοποθέτηση):** Move toward a decision or next step.

Each choice has a defined outcome quality (**Best**, **Acceptable**, **Bad**, or **VeryBad**) and affects an influence score. The game then presents feedback and the client's response, accompanied by NPC animation states.

The prototype includes **14 authored scenarios**, divided into two subsets of **7 scenarios each**.

In the standard study flow, each participant completes two blocks: one using Static AI and the other Adaptive AI, with different scenario subsets.

Mode order and subset assignment are counterbalanced across four condition groups.

## Adaptive AI Implementation

The adaptive mode includes:

### 1. Case-Based Reasoning (CBR)

The system searches a case library to find situations similar to the current interaction.

Case similarity is calculated using:

- The player's selected action.
- The virtual client's current state.
- Contextual tags describing the situation.

The most relevant case is selected according to its similarity score.

### 2. Case Confidence

The system considers the strength of the best match and compares it with the second-best case.

When a match is uncertain, the influence adjustment can be moderated.

This reduces the impact of ambiguous case matches.

### 3. Player Behavior Tracking

The game tracks several player performance metrics:

- Correct and incorrect actions.
- Reaction time.
- Success rate.
- Action variety.
- Risk-related behavior.
- Influence score changes.

These metrics support the adaptive behavior of the system.

### 4. Dynamic Difficulty Adjustment (DDA)

The prototype includes two mechanisms for adapting the experience.

**Level 1 — Scenario Reordering**

The system adjusts the order of remaining scenarios according to player performance.

- Strong performance brings more difficult scenarios forward.
- Low performance brings easier scenarios forward.
- Moderate performance preserves the existing order.

The system changes only the order of scenarios within the assigned subset, not the scenarios themselves.

**Level 2 — Influence Adaptation**

The system can adjust influence changes according to:

- Player reaction time.
- Correctness of selected actions.
- Previous mistakes.
- Risk-related behavior.
- Case matching confidence.

This allows the system to respond differently to different player behavior patterns.

### 5. Session Context Memory

The adaptive mode maintains information about contexts that were previously resolved successfully.

When a similar situation appears later in the same block, the virtual client can refer to the earlier interaction.

This allows the adaptive mode to demonstrate limited session-level memory.

The Static AI mode does not use this mechanism.

## AI System Characteristics

The project uses a curated case library, including cases informed by questionnaires with relevant professionals.

The prototype combines predefined rules, case-based reasoning, player behavior tracking, and difficulty adjustments.

**It is not a generative AI chatbot or a large language model (LLM).**

Player interactions are based on predefined choices rather than unrestricted voice conversations.

The system is designed for educational research and is not intended for clinical decision-making.

## Research Methodology

The prototype supports a counterbalanced within-subjects research design.

Each participant experiences both AI conditions:

1. Static Rule-Based AI.
2. Adaptive Case-Based AI.

The available scenarios are divided into two subsets:

- **Subset A:** 7 scenarios.
- **Subset B:** 7 scenarios.

Participants complete both subsets, with each subset assigned to a different AI mode.

Four condition groups control the order of AI modes and scenario subsets.

This structure helps reduce order effects and repeated exposure to the same scenarios.

The research compares player performance and experience between the two approaches.

## Data Collection and Logging

The application exports gameplay information as CSV files for research analysis.

### Session Dataset

Each session dataset contains information such as:

- Session and participant identifiers.
- AI mode.
- Scenario identifier.
- Selected action.
- Preferred action.
- Outcome quality.
- Reaction time.
- Base influence change.
- Final influence change.
- Adaptive adjustment.
- Case matching information.
- Player behavior metrics.
- Session memory indicators.
- Scenario difficulty.
- Condition group and block information.

### Session Summary

Session summary files contain aggregated performance information, including:

- Final influence score.
- Total actions.
- Correct and incorrect actions.
- Success rate.
- Average reaction time.
- Action variety.
- Risk preference.
- Outcome quality counts.

### Android / Meta Quest Export

On supported Android and Meta Quest builds, exported files are also saved to:

`Download/AdaptiveAIPrototype/`

Participant data collected during research should be handled separately from this public repository.

## Technologies Used

The project was developed using:

- **Unity** — Game engine and VR development environment.
- **C#** — Gameplay logic and AI implementation.
- **Unity XR Interaction Toolkit** — Virtual reality interaction support.
- **TextMesh Pro** — User interface and dialogue presentation.
- **Universal Render Pipeline (URP)** — Rendering pipeline.
- **Android / Meta Quest** — VR deployment target.

## Installation and Setup

### 1. Clone the Repository

```bash
git clone https://github.com/ElliePs6/ThesisGame.git
```

### 2. Open Unity Hub

Open Unity Hub and select **Add project from disk**.

Choose the cloned `ThesisGame` directory.

### 3. Open the Project

Use the Unity Editor version specified in:

`ProjectSettings/ProjectVersion.txt`

Allow Unity to import all required packages and assets.

### 4. Open the Startup Scene

Open:

`Assets/Scenes/BootstrapScene.unity`

The project includes the following main scenes:

- **BootstrapScene:** Initializes the game systems.
- **MainMenuNew:** Main menu and participant setup.
- **OfficeSession:** Main VR counseling simulation.
- **Results:** Displays the session results.

### 5. VR Testing

For standalone VR testing:

1. Configure the Unity Android build target.
2. Verify the required XR settings.
3. Connect a compatible Meta Quest headset.
4. Build and deploy the application.

VR testing requires compatible hardware and the appropriate Unity/XR configuration.

The repository does not include a prebuilt application installer.

## Project Structure

| Directory | Description |
| --- | --- |
| `Assets/Scripts/AI/` | Static AI, Adaptive AI, case matching, and AI evaluation. |
| `Assets/Scripts/Core/` | Gameplay flow, round management, player tracking, adaptive difficulty, and CSV logging. |
| `Assets/Scripts/UI/` | Main menu and results interface. |
| `Assets/Scripts/Animation/` | NPC animation state handling. |
| `Assets/Scenes/` | Main Unity scenes. |
| `Assets/` | Game assets, materials, models, and other resources. |
| `Packages/` | Unity package dependencies. |
| `ProjectSettings/` | Unity project configuration. |

## Academic Context

This project was developed as part of an undergraduate thesis investigating the differences between **Static and Adaptive Artificial Intelligence in Virtual Reality counseling-skills training**.

The main objective is to explore whether adaptive virtual-client behavior can provide a different training experience compared with predefined, static behavior.

The work focuses on:

- Artificial Intelligence in serious games.
- Virtual Reality for educational simulations.
- Case-Based Reasoning.
- Dynamic Difficulty Adjustment.
- Virtual client interaction.
- Player performance and experience evaluation.

The prototype is intended for educational and research purposes.

It does not replace professional counseling training or provide psychological treatment.

## Repository Notes

Some imported 3D models, animations, and texture files are relatively large.

The repository includes the project assets required by the Unity application.

Generated build files, temporary Unity data, and local cache directories are excluded through `.gitignore`.
