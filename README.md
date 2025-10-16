# VaultOfShadows

## Project Description
VaultOfShadows is a console-based game project focused on demonstrating secure data management using modern cryptography. The application allows users to create accounts, log in with either username or email, and play a game where their progress (monsters killed, distance traveled, etc.) is securely saved and synchronized. The project showcases encryption, hashing, and integrity verification for user and game data.

## Installation and Execution Instructions

### Prerequisites
- .NET 8.0 SDK or later
- Windows OS (recommended)

### Installation
1. Clone the repository:
   ```
   git clone <repository-url>
   ```
2. Navigate to the project directory:
   ```
   cd VaultOfShadows
   ```
3. Restore dependencies:
   ```
   dotnet restore
   ```

### Execution
To run the application:
```
dotnet run
```

To build and publish for deployment:
```
dotnet publish -c Release
```

## Technical Choices Explained
- **C# / .NET 8.0**: Chosen for its robust cryptography libraries and cross-platform capabilities.
- **PBKDF2 with SHA256**: Used for password hashing to ensure strong resistance against brute-force attacks.
- **AES-GCM**: Used for encrypting game saves, providing both confidentiality and integrity.
- **MongoDB**: Used for storing user accounts and progression, allowing flexible schema and scalability.
- **API Client**: All server communication is abstracted via a dedicated client for maintainability and testability.
- **Separation of Concerns**: The codebase is organized into distinct classes for account management, game logic, cryptography, and API communication.

## Task Distribution
For this project, there was no task distribution since I worked alone and not in a pair.

---
_Last updated: 2025-10-15_
