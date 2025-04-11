This is the back-end API built with ASP.NET Core that interacts with the official Hacker News API to fetch and cache the newest stories. It is designed to support an Angular front-end and provides features like story search, pagination, caching, and more.

🔹 Features
•	Provides the newest stories from the Hacker News API.
•	Caches story data using in-memory caching for improved performance.
•	Allows story search by title.
•	Supports pagination to limit stories per request.
•	Implements dependency injection for service classes.
•	Includes unit tests using xUnit.
•	Integrated Swagger for interactive API documentation.
•	CORS enabled for Angular front-end communication.

🔹 Prerequisites
•	.NET SDK 8 or higher
•	IDE like Visual Studio or VS Code

🔹 Getting Started
1. Clone the Repository: https://github.com/AnjaliFartare/HackerNewsApi.git
2. Navigate to cd /HackerNews.API
3. Restore Dependencies: dotnet restore
4. Run the API
5. API will start on : https://localhost:7107/api/hackernews

🔹 Swagger Documentation
•	Once running, open your browser and go to: https://localhost:7107/swagger/index.html


