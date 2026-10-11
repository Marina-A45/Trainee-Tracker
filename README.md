# Trainee Tracker

A web application developed as part of the Software Project (SoPro) 2026 at the University of Augsburg in collaboration with makandra GmbH.

## Overview

The Trainee Tracker is designed to support trainees throughout their training program and help mentors monitor learning progress. The application brings together lesson management, curriculum planning, progress tracking, feedback, and analytics.

The project was developed based on requirements provided by makandra GmbH as part of SoPro 2026.

## Key Features

- **User and role management** for administrators, mentors, and trainees
- **Curriculum and lesson management** to organize learning content
- **Learning progress tracking** with individual lesson statuses
- **Weekly planning** for current and upcoming lessons
- **Progress analytics** based on lesson effort estimates and working hours
- **Feedback management** for completed lessons
- **External API integration** for working-hour data
- **Automated testing** of important application functionality

*These features describe the project's scope; the implementation status may vary.*

## Architecture and Design

The application follows a three-layer architecture, separating the presentation layer, application logic, and data access.

The project included analysis and design activities before implementation. A domain model and a design class diagram were created to document the domain concepts and the planned software structure. These materials, along with further project documentation, are available in the `wiki/` directory.

Development started with the frontend. During a usability session with the client, the frontend was reviewed and feedback was gathered. Backend implementation followed this stage.

## Technology Stack

- C#
- .NET
- HTML and JavaScript
- SQL / SQLite
- Git and GitLab
- Unit testing and end-to-end testing

## Repository Contents

- **Application source code:** The existing project structure and three-layer architecture
- **Tests:** Automated tests separated from the application code
- **Wiki:** Project documentation, including analysis, design, requirements, and development guidelines

## Documentation

The project wiki contains documentation covering requirements analysis, system design, user scenarios, and team processes.

- [Domain Model](Wiki/Domänenmodell.md)
- [Design Class Diagram](Wiki/Designklassendiagramm.md)
- [System Operations](Wiki/Systemoperationen.md)
- [Scenarios](Wiki/Szenarien.md)
- [Storyboards](Wiki/Storyboards.md)
- [Definition of Done](Wiki/Definition-of-Done.md)
- [Customer Requirements and Acceptance Criteria](Wiki/Kundenanfoderung-Kriterien.md)
- [Style Guide](Wiki/Style-Guide.md)
- [Icons Set](Wiki/Icons-Set.md)
- [Responsibilities](Wiki/Verantwortlichkeiten.md)
- [Contracts](Wiki/Kontrakte.md)