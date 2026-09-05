# Unity Multiplayer Test

## Overview
A cross-platform multiplayer test project built in Unity with support for:
- **Steam** networking (via Steamworks)
- **WebRTC** for peer-to-peer communication (with Desktop, Mobile, and WebGL support)
- **Custom Network Manager** for better runtime control

## Setup Instructions

### General Usage
- Use [CustomNetworkManager](Assets/Scripts/Network/CustomNetworkManager.cs) instead of the default `NetworkManager`.
  - Prevents duplicate instances and adds utility functions for transport handling.
- Attach all transport components (e.g., Steam, WebRTC) to the same GameObject as the `CustomNetworkManager`
- Read the Steam/WebRTC sections for more information on their usage

### Define Symbols
- Add the following symbol to **non-Steam** platforms (e.g., WebGL, Android):
  ```
  DISABLESTEAMWORKS
  ```

### Android Build Tips
If you experience crashes when building for Android:
- Go to **Player Settings > Android > Other Settings**
  - Uncheck `Auto Graphics API`
  - Reorder graphics APIs so `OpenGLES3` is first
  - Remove `Vulkan` if unnecessary
  - Enable `Require ES3.1`, `Require ES3.1+AEP`, and `Require ES3.2`


## Steam Support

### Dependencies
- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net)
- [Heathen SystemCore](https://github.com/heathen-engineering/SystemCore.git?path=/com.heathen.systemcore)
- [SteamNetworkingSockets Transport](https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.steamnetworkingsockets)
- [Heathen Toolkit for Steamworks Foundation](https://github.com/heathen-engineering/Toolkit-for-Steamworks-Foundation.git?path=/Unity/com.heathen.steamworksfoundation)

### Added Features
- [SteamCustomTransport.cs](Assets/Scripts/Network/Steam/SteamCustomTransport.cs)
  - Wrapper around `SteamNetworkingSocketsTransport` to avoid asset reference breakages when switching build targets

### Usage
- Use `SteamCustomTransport` instead of `SteamNetworkingSocketsTransport`
- Set Steam App ID in:
  - The inspector on `SteamCustomTransport`
  - A file named `steam_appid.txt` at the project root


## WebRTC Support

This project uses the community WebRTC transport from the `transport/webrtc` branch of [aziztitu/unity-multiplayer-community-contributions](https://github.com/aziztitu/unity-multiplayer-community-contributions/tree/transport/webrtc):

```
https://github.com/aziztitu/unity-multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.webrtc#transport/webrtc
```

See that package README for install, signaling, ICE/TURN, and WebGL details.

### Dependencies
- [Socket.IO Unity](https://github.com/itisnajim/SocketIOUnity.git) (Editor / Desktop / Mobile signaling)
- `com.unity.webrtc` (pulled in by the transport package)
- The WebRTC transport package itself

### Signaling server

For development and testing, you can use the public server at [https://signal.multiplayer.azeesoft.com/](https://signal.multiplayer.azeesoft.com/). Open that page, generate a token, then set **Signaling Server URL** to `wss://signal.multiplayer.azeesoft.com` and **Signaling Server Auth Token** to that token.

For production, run your own server from [webrtc-ngo-signaling](https://github.com/aziztitu/webrtc-ngo-signaling), or implement the same Socket.IO events. The sample listens on `http://localhost:4000` by default.

### Unity configuration

1. Attach `WebRTCTransport` to the same GameObject as your `CustomNetworkManager` (or stock `NetworkManager`).
2. Set **Signaling Server URL**:
   - Public test server: `wss://signal.multiplayer.azeesoft.com`
   - Local sample: `http://localhost:4000`
   - Your production server: `https://your-domain.com` (or `wss://your-domain.com`)
3. Set **Signaling Server Auth Token** (token from the public page, or the token your own server expects).
4. Optionally add custom ICE / TURN servers. Add a TURN server before production — STUN-only connections fail on some NAT types.
5. Host: leave `roomId` empty. Client: set `roomId` before connecting.

