# VaultOfShadows

## Project Description
VaultOfShadows is a Unity-based game project focused on secure data management and engaging gameplay. Players can create accounts, log in with either username or email, and play in a procedurally generated arena with monsters, items, and doors to other rooms. Progress (monsters killed, distance traveled, HP, etc.) is securely saved and synchronized using modern cryptography. The project demonstrates encrypted saves, integrity verification, and multi-room gameplay logic, all with a modern UI.

## Features
- Multi-account login (username or email) and registration
- Password masking in UI (TMP InputFields)
- Encrypted save/load (local and server)
- Arena gameplay: monsters, items, doors, rooms
- Room transitions: doors spawn on arena borders, player keeps stats (HP, inventory) between rooms
- Leaderboard: fetches and displays stats (username, monsters killed, distance, integrity, timestamp)
- UI panels: instant switching, animated transitions, stat display
- Game menu, leaderboard, and disconnect logic
- .dll usage for SaveApp.Core (game logic, encryption)
- Repository management with .gitignore

## Installation and Execution Instructions

### Prerequisites
- Unity 2022.3 LTS or later
- Windows OS (recommended)
- .NET 8.0 SDK (for SaveApp.Core if rebuilding)

### Installation
1. Clone the repository:
   ```
   git clone https://github.com/RoGxGlory/VaultOfShadows.git
   ```
2. Open the project in Unity Hub or Unity Editor.
3. If SaveApp.Core.dll is missing, build SaveApp.Core from source (see SaveApp.Core folder) or copy the .dll to `Assets/Core/`.
4. Ensure TMP (TextMeshPro) is installed via Unity Package Manager.

### Execution
- Open the project in Unity and press Play.
- For API/server features, ensure the backend is running and configured.
- To rebuild SaveApp.Core.dll, open SaveApp.Core in Visual Studio and build for .NET Standard 2.1 or .NET 8.0.

## Usage
- Login/register with username or email.
- Play the game: move, fight, pick up items, go through doors to new rooms.
- Stats (HP, monsters killed, distance) are shown in the UI and saved securely.
- Leaderboard panel fetches and displays latest stats from the API.
- Panels switch instantly; animations play for success/failure events.
- Passwords are masked in UI fields.

## Technical Choices Explained
- **Unity**: For modern UI, animation, and cross-platform support.
- **TextMeshPro (TMP)**: For advanced text rendering and input fields.
- **SaveApp.Core.dll**: Game logic, encryption, and save/load are handled in a separate .dll for modularity.
- **PBKDF2 with SHA256**: Password hashing for security.
- **AES (CBC)**: Encrypted saves for confidentiality and integrity.
- **API Client**: Abstracts server communication for maintainability.
- **Panel/Animation Logic**: Animator parameters (Success, ShouldPlay) control feedback for login, save, and other actions.
- **Repository Structure**: .gitignore filters out build artifacts, Library, obj, and other non-essential files.

## Repository Structure
```
VaultOfShadows/
  ├── Assets/
  ├── ProjectSettings/
  ├── SaveApp.Core/   # Source for .dll (not always pushed)
  ├── SaveApp/        # API client and related logic
  ├── UserSettings/
  ├── VaultOfShadows.sln
  ├── Assembly-CSharp.csproj
  ├── Dockerfile      # (optional, for containerized builds)
  ├── .gitignore
```
- The SaveApp.Core.dll is required in `Assets/Core/` for game logic and encryption.
- The API backend (SaveAppApi) is managed separately and not included in this repo.

## Known Issues / Notes
- TMP InputFields mask passwords visually, but are not as secure as console masking.
- Panel animations may have slight delays due to Unity Animator logic.
- When switching panels, use instant switch functions for best UX.
- HP and stats persist between rooms; leaderboard only updates after save.
- Dockerfile is optional and not required for Unity development.

---
_Last updated: 2025-10-17_
