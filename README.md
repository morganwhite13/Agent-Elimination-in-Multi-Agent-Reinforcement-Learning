# Agent Elimination in Multi-Agent Reinforcement Learning

[![Unity ML-Agents](https://img.shields.io/badge/Unity%20ML--Agents-2.3.0-blue)](https://github.com/Unity-Technologies/ml-agents)
[![Python](https://img.shields.io/badge/Python-3.8%2B-green)](https://www.python.org/)

> An honours research project investigating whether agent elimination mechanisms can improve cooperation in Multi-Agent Reinforcement Learning environments where individual greediness is rewarded but group cooperation is optimal.

## 📋 Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Research Question](#research-question)
- [Environment Design](#environment-design)
- [Installation](#installation)
- [Usage](#usage)
- [Experimental Results](#experimental-results)
- [Technical Architecture](#technical-architecture)
- [Future Work](#future-work)
- [Citation](#citation)
- [Acknowledgments](#acknowledgments)

## 🎯 Overview

This project explores the **Sequential Social Dilemma (SSD)** problem in multi-agent systems, where agents must choose between cooperation (beneficial for the group) and defection (beneficial for individuals). By implementing configurable elimination mechanics, we test whether the "fear of punishment" can encourage cooperative behavior among RL agents.

**Key Finding**: Agents learned to eliminate competitors early rather than punishing greedy behavior, leading to "kill first, harvest alone" strategies instead of cooperative enforcement.

### Project Highlights

- ✅ Custom Unity ML-Agents environment (about 1,000 lines of C# across 8 scripts, built on Unity's FoodCollector example)
- ✅ Configurable elimination thresholds (1-9 agents)
- ✅ Three RL algorithms tested: PPO, POCA, and SAC
- ✅ 20+ normalized observation features per agent
- ✅ Density-based food respawn mechanics inspired by DeepMind's Harvest
- ✅ Comprehensive experimental analysis with 500K+ training steps

## 🔑 Key Features

### Environment Mechanics
- **80×80 unit bounded arena** (`areaRange: 40`, positions drawn from ±40) with multiple agents (1-10 configurable)
- **Resource dynamics**: Food respawns based on local density, encouraging sustainable harvesting
- **Elimination system**: Configurable thresholds requiring 1-9 agents to eliminate/freeze a target
- **Laser weapons**: Agents can eliminate or temporarily freeze other agents
- **Visual feedback**: Material changes indicate frozen/eliminated states

### Observation Space (20+ features)
- Normalized agent velocity
- Frozen/eliminated status flags
- Weapon cooldown timers
- Spatial awareness (distances to food/agents)
- Other agents' states and positions
- Hit tracking for strategic elimination decisions

### Reward Structure
```
Collect Good Food:     +1.0   (hard-coded)
Collect Bad Food:      -1.0   (hard-coded)
Fire Laser (miss):      0     (rewardForFiring is loaded but never applied)
Zap Another Agent:     rewardForZapping      (AgentSettings default +0.5)
Get Zapped:            rewardForBeingZapped  (AgentSettings default -0.5)
Freeze duration:       4 s    (hard-coded as m_FrozenTime + 4f; AgentSettings.frozenTime is unused)
```

The zap rewards and the freeze/eliminate thresholds are read at start-up from an `AgentSettings` asset that `Agent.prefab` references but that is not in this repository, so the exact values used in training can't be recovered; the numbers above are the defaults in `AgentSettings.cs`. `Agent.prefab` serializes `agentsToEliminateThreshold: 1` and `permanentlyEliminates: 1`; `permanentlyEliminates` is used as-is, while the threshold is overwritten from the asset in `Initialize()`.

## ❓ Research Question

**Can allowing agents to eliminate each other create a "fear of punishment" that encourages cooperation in Sequential Social Dilemmas?**

### Hypothesis
Agents would learn to eliminate consistently greedy members, leading to improved group cooperation and sustainable resource harvesting.

### Reality
Agents optimized for individual reward by eliminating competitors early, reducing competition rather than enforcing cooperation.

## 🎮 Environment Design

### Evolution from MeltingPot to Unity

**Original Approach**: Google DeepMind's MeltingPot Harvest environment
- **Challenge**: Poor documentation, library incompatibilities
- **Result**: 3 months of development abandoned

**Final Solution**: Custom Unity ML-Agents environment
- Built on Unity's FoodCollector example
- Added elimination mechanics and density-based respawning
- Integrated visual debugging and configurable parameters
- Modified food spawning mechanics

### Resource Respawn Logic

Food spawns probabilistically based on local density:

```csharp
P(respawn) = proximityFactor × nearbyFoodCount
```

This creates natural "tragedy of the commons" scenarios where overharvesting depletes resources.

## 🚀 Installation

### Prerequisites

- Unity 2021.3 LTS or later
- Python 3.8+
- Git

### Setup Instructions

1. **Clone the repository**
```bash
git clone https://github.com/morganwhite13/Agent-Elimination-in-Multi-Agent-Reinforcement-Learning.git
cd Agent-Elimination-in-Multi-Agent-Reinforcement-Learning
```

> **Note:** the repository contains the scripts, prefabs, scene, models and configs, but not the Unity project structure (`.meta` files, `ProjectSettings/`, `Packages/manifest.json`), so a fresh clone can't be opened directly as a Unity project.

2. **Install Python dependencies**
```bash
pip install mlagents==0.30.0
pip install torch torchvision
pip install numpy matplotlib
```

3. **Open in Unity**
- Launch Unity Hub
- Add project from disk
- Open the `Harvest.unity` scene

4. **Verify ML-Agents Package**
- Window → Package Manager
- Ensure ML-Agents (2.3.0) is installed

## 💻 Usage

### Training Agents

**Train with PPO (recommended for efficiency)**
```bash
mlagents-learn FoodCollectorPPO.yaml --run-id=ppo_baseline
```

**Train with SAC (better performance, slower)**
```bash
mlagents-learn FoodCollectorSac.yaml --run-id=sac_experiment
```

**Monitor training with TensorBoard**
```bash
tensorboard --logdir results/
```

### Configuration Options

Edit `FoodCollectorArea` in Unity Inspector:
- `numAgents`: Number of agents (1-10)
- `agentsToFreezeThreshold`: Agents needed to freeze target
- `agentsToEliminateThreshold`: Agents needed to eliminate permanently
- `permanentlyEliminates`: Toggle freeze vs. elimination
- `initialFoodCount`: Starting food quantity
- `respawnInterval`: Food respawn frequency (seconds)

### Running Trained Agents

1. Load trained model in Unity (`.onnx` file)
2. Set `Behavior Type` to "Inference Only"
3. Press Play to watch trained agents

## 📊 Experimental Results

### Experiment 1: Elimination Threshold Effects

| Configuration | Cumulative Reward (500K steps) | Key Finding |
|--------------|-------------------------------|-------------|
| No Elimination | ~12,000 | Baseline |
| 1 Agent Required | ~11,500 | **2nd Best** - Early elimination reduces competition |
| 2 Agents Required | ~10,000 | Moderate cooperation |
| 3 Agents Required | ~9,500 | Difficult coordination |
| 4 Agents Required | ~9,000 | Poorest performance |

**Insight**: Individual elimination performed better than group elimination, contrary to our hypothesis. Agents eliminated early to harvest alone.

With one hit enough to permanently eliminate an agent (the setting serialized in `Agent.prefab`; the missing settings asset may override the threshold) and no cost for firing, eliminating rivals early is close to built into the incentives: a shot costs nothing, and a hit removes a competitor for the rest of the episode. This helps explain the "kill first, harvest alone" finding.

### Experiment 2: Algorithm Comparison

| Algorithm | Performance | Training Time | Best Use Case |
|-----------|-------------|---------------|---------------|
| **SAC** | ~11,800 (Best) | 24 hours | Exploration of diverse strategies |
| **PPO** | ~10,500 | 8 hours | Efficient baseline training |
| **POCA** | ~10,200 | 10 hours | Cooperative scenarios |

**Insight**: SAC's entropy maximization discovered more effective elimination strategies, though at 3× the training cost.

**Caveat**: this comparison isn't like-for-like. Beyond the algorithm, the three configs differ: PPO and POCA run for 500K steps vs. 2M for SAC; SAC uses 1 layer, `normalize: false` and the `simple` encoder vs. 2 layers, `normalize: true` and `nature_cnn` for PPO/POCA; curiosity (strength 0.02) is used for PPO and SAC but not POCA. No seeds are set and each result is a single run. There is also no `SimpleMultiAgentGroup` in the code, so POCA trains without a multi-agent group and behaves close to independent agents.

### Experiment 3: Freeze vs. Elimination

- **Permanent Elimination**: ~11,500 reward
- **Temporary Freeze (4s)**: ~10,800 reward

**Insight**: Temporary punishment created revenge cycles, while permanent elimination removed problematic agents decisively.

## 🏗️ Technical Architecture

### Core Components

```
Scripts/
├── FoodCollectorAgent.cs       # Agent logic, observations, actions (about 440 lines)
├── FoodCollectorArea.cs        # Environment management (about 240 lines)
├── AgentSettings.cs            # Configurable parameters (ScriptableObject)
├── FoodCollectorSettings.cs    # Global environment settings
└── FoodLogic.cs                # Food item behavior

Config/
├── FoodCollectorPPO.yaml       # PPO hyperparameters
├── FoodCollectorPOCA.yaml      # POCA hyperparameters
└── FoodCollectorSac.yaml       # SAC hyperparameters

Prefabs/
├── Agent.prefab                # Agent with elimination mechanics
├── Food.prefab                 # Good food collectible
├── BadFood.prefab              # Penalty food
└── FoodCollectorArea.prefab    # Complete training environment
```

### Algorithm Highlights

**Hit Tracking System**
```csharp
private static Dictionary<FoodCollectorAgent, HashSet<FoodCollectorAgent>> hitTracker;

void TrackHit(FoodCollectorAgent target, FoodCollectorAgent shooter) {
    if (!hitTracker.ContainsKey(target))
        hitTracker[target] = new HashSet<FoodCollectorAgent>();
    
    if (!hitTracker[target].Contains(shooter)) {
        hitTracker[target].Add(shooter);
        CheckAgentStatus(target);
    }
}
```

**Known Code Issues** (not fixed here; fixing them needs a Unity run to verify)
- `hitTracker` is `static` and `ClearHitTracker()` is never called, so a shooter can only freeze/eliminate a given target once across the whole run, and the hit-count observation carries over between episodes.
- Eliminated agents keep collecting observations and requesting decisions.
- Observations use scene-wide `FindObjectsOfType`, so the order of other-agent slots isn't stable, and observations would mix areas if more than one area were used.

**PPO Hyperparameters**
```yaml
batch_size: 1024
buffer_size: 10240
learning_rate: 0.0002
time_horizon: 256
curiosity_strength: 0.02
hidden_units: 256
num_layers: 2
max_steps: 500000
```

## 🔮 Future Work

### Immediate Extensions
1. **Explicit Greed Detection**: Add "food per minute" metric to observations
2. **Communication Protocol**: Allow agents to signal intentions before eliminating
3. **Democratic Enforcement**: Leader election for designated "enforcer" agents
4. **Curriculum Learning**: Gradually introduce elimination mechanics during training

### Advanced Research Directions
1. **Longer Episodes**: Extend from 1,000 to 10,000+ steps to capture long-term effects
2. **Custom RL Algorithm**: Design architecture specifically for SSD cooperation
3. **Multi-Environment Training**: Vary respawn rates for robustness
4. **Real-World Applications**: 
   - Autonomous vehicle coordination
   - Distributed computing resource allocation
   - Robot swarm self-policing

### Technical Improvements
- Cloud GPU training (AWS/GCP) for faster SAC experiments
- Incremental punishment (jail time vs. elimination)
- Human-AI mixed agent studies
- Cross-environment testing (Cleanup, Stag Hunt, Public Goods)

## 🙏 Acknowledgments

- **Unity ML-Agents Team** - For the excellent framework and documentation
- **DeepMind** - For inspiring the Harvest environment design

### Key References

1. Leibo et al. (2017) - "Multi-agent Reinforcement Learning in Sequential Social Dilemmas"
2. Jaques et al. (2018) - "Social Influence as Intrinsic Motivation for Multi-Agent Deep RL"
3. Schulman et al. (2017) - "Proximal Policy Optimization Algorithms"
4. Haarnoja et al. (2018) - "Soft Actor-Critic: Off-Policy Maximum Entropy Deep RL"

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📧 Contact

- [**Portfolio**](https://morganwhite13.github.io/)
- [**Email**](mailto:morgan13white@icloud.com)
- [**LinkedIn**](https://www.linkedin.com/in/morgan-white-95b245237/)
