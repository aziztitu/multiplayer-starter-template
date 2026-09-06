# Multiplayer Starter Template

Unity **6000.6.0f1** starter for a standalone multiplayer game: lobby first, then one playable level.

This template does **not** use Git LFS. Leave it that way here so GitHub does not charge for LFS storage.

If you copy the starter into your own game and want LFS for art or audio:

1. Install [Git LFS](https://git-lfs.com) and run `git lfs install`.
2. Track binaries in `.gitattributes` (for example `git lfs track "*.fbx" "*.png" "*.wav" "*.mp3" "*.psd"`), then commit that file.
3. New matching files go to LFS. Files already in this repo stay as normal Git objects unless you run `git lfs migrate`.

## Play

1. Open the project in Unity 6 (`6000.6.0f1`).
2. Load `Assets/Scenes/Lobby.unity`.
3. Play, host or join, then **Start**. The lobby loads `SampleLevel` automatically.

## Packages

Pinned in `Packages/manifest.json`:

- [AZ Utilities](https://github.com/aziztitu/az-utils) `1.0.2`
- [AZ Multiplayer Core](https://github.com/aziztitu/multiplayer-core) `1.0.5`

Use `AZNetworkManager` from AZ Multiplayer Core (not the stock NGO `NetworkManager`). Put Steam / WebRTC transport components on the same GameObject.

### Scripting defines

| Define | Where |
|---|---|
| `DOTWEEN` | Standalone player settings + every build profile (DOTween is in `Assets/_External`, not UPM) |
| `STEAMWORKS_NET` | Standalone |
| `DISABLESTEAMWORKS` | Web and Android build profiles |
| `UNITY_NETCODE` / `NETWORK_DICTIONARY` | Build profiles |

## Project layout

| Path | Role |
|---|---|
| `Assets/Scenes/Lobby.unity` | Host / join (includes `GameManager`) |
| `Assets/Scenes/Sample/SampleLevel.unity` | The playable scene (`LevelManager`) |
| `Assets/Prefabs/Network` | `AZ Network Manager`, default network prefabs list |
| `Assets/Prefabs/Managers` | `GameManager`, `LevelManager`, `LobbyManager` |
| `Assets/Prefabs/Player` | `PlayerCharacter`, `PlayerNetworkIdentity` |
| `Assets/Prefabs/UI` | `LobbyUI`, `PlayerList`, `PauseMenu` |
| `Assets/Prefabs/Camera` | Camera rigs |
| `Assets/Scripts/Data` | Sample `[GenerateNetworkSerialization]` structs |
| `Assets/Generated` | Auto-written `*.g.cs` (commit these; do not edit) |

This project owns the lobby (`LobbyUI`, nested `PlayerList`, `LobbyManager`), `AZ Network Manager`, the network prefabs list, `PlayerNetworkIdentity`, and `PauseMenu`. AZ Multiplayer Core owns `AZNetworkManager` and `SimplePlayerCharacterSpawner`.

To use a custom lobby roster, subclass `LobbyPlayerList`, replace the nested `PlayerList` on `LobbyUI`, and assign it to the **Player List** field.

If you rename the level scene, add it to **File → Build Profiles** and set that name on `LobbyUI` `gameSceneNames`. The lobby starts the first name in that list.

## Network serialization

NGO needs `INetworkSerializable` (and usually `IEquatable<T>`) on custom structs you send over the network. Mark a **`partial struct`** with `[GenerateNetworkSerialization]` instead of writing that by hand.

Examples: `Assets/Scripts/Data/SimpleData.cs`, `TestData.cs`, `TestNestedData.cs`. Generated output lands in `Assets/Generated/`.

1. Requires the **.NET 8 SDK** (`dotnet` on PATH).
2. Save the struct, or run **Tools → AZ → Generate Network Serialization**.
3. Commit both the struct and the `.g.cs` file. Do not edit the generated files.

The struct **must** be `partial`. Classes are not supported. Prefer `FixedString*Bytes` over `string`, and `FixedList*Bytes<T>` over `List<T>`.

Full field list and nested-struct rules: [AZ Multiplayer Core README](https://github.com/aziztitu/multiplayer-core#network-serialization-generator).

## Steam

### Dependencies

- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net)
- [Heathen SystemCore](https://github.com/heathen-engineering/SystemCore.git?path=/com.heathen.systemcore)
- [SteamNetworkingSockets Transport](https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.steamnetworkingsockets)
- [Heathen Toolkit for Steamworks Foundation](https://github.com/heathen-engineering/SteamworksFoundation.git?path=/Unity/com.heathen.steamworksfoundation)

Use `SteamCustomTransport` from AZ Multiplayer Core instead of `SteamNetworkingSocketsTransport` so asset references survive switching build targets.

Set the Steam App ID on that component and in `steam_appid.txt` at the project root.

## WebRTC

Community transport: [aziztitu/unity-multiplayer-community-contributions](https://github.com/aziztitu/unity-multiplayer-community-contributions/tree/transport/webrtc) (`transport/webrtc` branch).

```
https://github.com/aziztitu/unity-multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.webrtc#transport/webrtc
```

Also needs [Socket.IO Unity](https://github.com/itisnajim/SocketIOUnity.git) and `com.unity.webrtc`.

### Signaling

Dev/test: [https://signal.multiplayer.azeesoft.com/](https://signal.multiplayer.azeesoft.com/) — generate a token, then set **Signaling Server URL** to `wss://signal.multiplayer.azeesoft.com` and **Signaling Server Auth Token** to that token.

Production: run [webrtc-ngo-signaling](https://github.com/aziztitu/webrtc-ngo-signaling) (sample listens on `http://localhost:4000`) or implement the same Socket.IO events.

### Unity setup

1. Attach `WebRTCTransport` to the same GameObject as `AZNetworkManager`.
2. Set **Signaling Server URL** and **Signaling Server Auth Token**.
3. Optionally add TURN before shipping — STUN-only fails on some NATs.
4. Host: leave `roomId` empty. Client: set `roomId` before connecting.

## Android

If the build crashes:

- **Player Settings → Android → Other Settings**
  - Uncheck Auto Graphics API
  - Put `OpenGLES3` first; drop Vulkan if you do not need it
  - Enable Require ES3.1 / ES3.1+AEP / ES3.2
