FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_HTTP_PORTS=5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Trainee_Tracker.API/TraineeTracker.csproj", "Trainee_Tracker.API/"]
COPY ["Trainee_Tracker.Core/Trainee_Tracker.Core.csproj", "Trainee_Tracker.Core/"]
COPY ["Trainee_Tracker.Service/Trainee_Tracker.Service.csproj", "Trainee_Tracker.Service/"]
COPY ["Trainee_Tracker.Repository/Trainee_Tracker.Repository.csproj", "Trainee_Tracker.Repository/"]

RUN dotnet restore "Trainee_Tracker.API/TraineeTracker.csproj"

COPY . .

RUN dotnet publish "Trainee_Tracker.API/TraineeTracker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TraineeTracker.dll"]