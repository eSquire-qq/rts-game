# RTS Game

A 2D real-time strategy game built with Unity 6. Multiplayer runs through an authoritative Java server: the client sends player commands and renders the state the server sends back.

**Game server:** [rts-java-server](https://github.com/eSquire-qq/rts-java-server)

## Features

- **Unit control.** Click or drag-box selection, move, attack units and buildings, stop, gather resources.
- **Economy.** Gold and lumber resource nodes, units gathering from them, supply limit.
- **Building.** Barracks, archery range and house, placed with a ghost preview.
- **Unit training.** Swordsmen from barracks, archers from the archery range.
- **Multiplayer.** Main menu with lobby creation, join by code and ready check. Matches are simulated on the server.
- **Server-driven world.** Units, buildings and resources are created, updated and removed from server state snapshots (`UnitsClientWorld`, `BuildingsClientWorld`, `ResourcesClientWorld`).
- **UI.** Top bar with resources and supply, selection info, command panel, health bars.
- **RTS camera.** Arrow keys, edge scrolling and mouse-wheel zoom.

## Tech stack

- Unity **6000.3.4f1** (Unity 6)
- C#
- Universal Render Pipeline (2D)
- Unity Input System
- TextMesh Pro, uGUI
- Networking: plain TCP (`System.Net.Sockets.TcpClient`), newline-delimited JSON parsed with `JsonUtility`. Network messages are handed to the Unity main thread through `MainThreadDispatcher`.

## How it works

```
 Player input ──▶ SelectionAndOrders / BuildPlacementManager
                          │
                          ▼
                 LobbyClient (cmd_move, cmd_attack, cmd_build, …)
                          │  TCP, JSON lines
                          ▼
                 rts-java-server (validates, simulates at 20 ticks/s)
                          │  "state" snapshot every tick
                          ▼
   UnitsClientWorld / BuildingsClientWorld / ResourcesClientWorld ──▶ views on screen
```

The client holds no game logic that decides outcomes. Positions, HP, resources and supply always come from the server.

## Getting started

1. Start the server, see [rts-java-server](https://github.com/eSquire-qq/rts-java-server#getting-started). It listens on `127.0.0.1:7777`.
2. Clone this repository:
   ```bash
   git clone https://github.com/eSquire-qq/rts-game.git
   ```
3. Open Unity Hub → **Add project from disk** → select the cloned folder. Use Unity **6000.3.4f1**.
4. Open `Assets/Scenes/MainMenuScene.unity` and press **Play**.
5. Create a lobby, or join with a lobby code, then press Ready. The match scene (`SampleScene`) loads when everyone is ready.

To connect to a server on another machine, change `host` and `port` on the `NetClient` component in `MainMenuScene`.

## Controls

| Action | Input |
|---|---|
| Select unit | Left click |
| Box select | Left mouse drag |
| Move / gather (click on a resource) | Right click |
| Attack | Select your units, then left click an enemy unit or building |
| Stop | `S` |
| Place building | Choose it in the command panel, then left click; `Esc` or right click to cancel |
| Move camera | Arrow keys or move the mouse to the screen edge |
| Zoom | Mouse wheel |

## Project structure

```
Assets/Code/
  Server/       – networking: NetClient (TCP), LobbyClient (protocol), MainThreadDispatcher,
                  *ClientWorld (sync server state into scene objects)
  Controller/   – selection, box selection, issuing orders
  Core/         – build placement, entity ids, resource views
  Units/        – UnitView
  Buildings/    – BuildingsView, building selection
  UI/           – main menu, top bar, command panel, selection info, training UI
  Camera/       – RTS camera controller
  Health/       – health bars
Assets/Scenes/
  MainMenuScene – auth, lobby
  SampleScene   – match
```

## Status

Work in progress / prototype. Current focus: polishing the UI and adding more units and buildings.

## Credits

Art: [Tiny Swords](https://pixelfrog-assets.itch.io/tiny-swords) by Pixel Frog.

## Author

Artem Teslia – [GitHub](https://github.com/eSquire-qq)
