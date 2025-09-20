# FootballTeamManager

A small web application to manage a football team's player roster.

## Features

- Add, view, update, and remove players from the roster
- Display all players in a Bootstrap-styled table with sorting and basic validation
- Simple integration with Football-Data.org to display Arsenal's results and upcoming fixtures
- RESTful API endpoint to retrieve all players as JSON

## Technology Stack

- **Backend**: ASP.NET MVC with Entity Framework (service-based architecture)
- **Frontend**: HTML, Bootstrap, JavaScript, jQuery, Razor Pages
- **Database**: SQL Server (local development)
- **External API**: Football-Data.org

## Database Setup

Create a Players table:

```sql
CREATE TABLE Players (
    PlayerId INT PRIMARY KEY IDENTITY(1,1),
    PlayerName NVARCHAR(100) NOT NULL,
    Position NVARCHAR(50) NOT NULL,
    JerseyNumber INT NOT NULL UNIQUE,
    GoalsScored INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    UpdatedDate DATETIME2 DEFAULT GETDATE()
);
```

**Note**: You can host the database locally or on Azure when deploying.

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/dwcbenson/FootballTeamManager.git
   ```

2. Open the solution in Visual Studio

3. Restore NuGet packages

4. Update the database connection string in `appsettings.json`

5. Set the user secret using '`dotnet user-secrets set "FootballDataApi:ApiKey" "YOUR_KEY_HERE"`

6. Run the application

## API Endpoints

### Player Management
- **GET** `/api/players` – Retrieve all players as JSON
- **GET** `/api/players/{id}` – Retrieve a specific player by ID
- **POST** `/api/players` – Create a new player
- **PATCH** `/api/players/{id}` – Update an existing player
- **DELETE** `/api/players/{id}` – Delete a player

### Arsenal Data
- **GET** `/api/arsenal/recent-results` – Get Arsenal's recent match results
- **GET** `/api/arsenal/upcoming-fixtures` – Get Arsenal's upcoming fixtures

## Frontend

### Pages
- **Index** (`/Players`) – Main player roster view
- **Create** (`/Players/Create`) – Add new player form  
- **Edit** (`/Players/Edit/{id}`) – Edit existing player form

### Player Display
- Players are displayed in a table with the following columns: Player Name, Position, Jersey Number, Goals Scored
- Buttons to edit or remove a player next to each entry
- Form to add a new player with basic validation:
  - PlayerName is required
  - JerseyNumber must be unique

## External API Integration

Football-Data.org results and next 5 fixtures for Arsenal are displayed in simple tables.

## Notes

- Full CRUD operations supported through both MVC views and REST API
- Comprehensive error handling with proper HTTP status codes
- Service-based architecture separating concerns between controllers and business logic
- Sorting players by goals scored is implemented in the frontend table
- API returns appropriate responses (NoContent, NotFound, Conflict) based on operation results