# TeamUp - Real-Time Collaboration Platform

**TeamUp** is a .NET Framework 4.8 desktop application built using WPF (Windows Presentation Foundation), designed to provide a seamless, real-time collaboration environment for students. It allows users to work together on essays and code simultaneously, with live synchronization.

## Features

- **Real-Time Collaboration**: Edit documents or code with teammates in real time.
- **Workspace System**: Create, manage, and persist custom workspaces.
- **User Authentication**: Unsecure registration and login using JSON-stored credentials, not recommened for use.
- **Live Sync**: Auto-sync changes every 5 seconds to prevent version conflicts.
- **Modern UI**: Clean, responsive WPF interface with animations and custom styles.
- **Cross-User Interaction**: View and collaborate with other users inside the same workspace.

## Technologies Used

- **.NET Framework 4.8**
- **C# (WPF)**
- **JSON** for data storage and communication
- **Local C++ WebSocket server** for backend logic
- **Storyboard animations** for fluid UI transitions

## Application Architecture

TeamUp follows a **layered architecture**:

- **Presentation Layer (C# WPF)**: User interface, animations, and user experience.
- **Logic Layer (C++)**: Processes client requests and handles real-time sync via WebSockets.
- **Data Layer (JSON)**: Lightweight storage for user accounts and workspace data.

## UI Screens

- **Home**: Displays user stats like weekly activity and points.
- **Workspaces**: Manage and edit collaborative documents.
- **MyProfile**: View personal data and log out.
- **Settings**: Modify account options, reset settings, or enable topmost mode.

C++ (WS) : https://github.com/chr1srbx/Local-Server-CPP
