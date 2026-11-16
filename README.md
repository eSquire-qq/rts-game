# RTS Game

A 2D real-time strategy game built with Unity. Multiplayer is handled by an authoritative Java server: the client sends commands and renders the state the server returns.

**Game server:** [rts-java-server](https://github.com/eSquire-qq/rts-java-server)

## Screenshots

![Gameplay](docs/gameplay.png)

## Features

- 2D RTS gameplay: [units, selection, movement, building, resources — вкажи, що є]
- Multiplayer through an authoritative server (client never decides game state)
- [Other features]

## Tech stack

- Unity [версія з ProjectSettings/ProjectVersion.txt]
- C#
- [Networking library/approach]

## Getting started

1. Start the server: see [rts-java-server](https://github.com/eSquire-qq/rts-java-server#getting-started).
2. Clone this repository:
   ```bash
   git clone https://github.com/eSquire-qq/rts-game.git
   ```
3. Open Unity Hub → **Add project from disk** → select the cloned folder (use Unity [версія]).
4. Open the scene `[Assets/Scenes/...]` and press **Play**.
5. If the server is not on localhost, set its address in `[where to configure it]`.

## Controls

| Action | Input |
|--------|-------|
| [Select unit] | [Left mouse button] |
| [Move/command] | [Right mouse button] |

## Status

[Prototype / work in progress. Short note on what works and what is planned.]

## Author

Artem Teslia – [GitHub](https://github.com/eSquire-qq)
