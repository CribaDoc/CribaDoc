FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CribaDoc.sln ./
COPY src/CribaDoc.Core/CribaDoc.Core.csproj src/CribaDoc.Core/
COPY src/CribaDoc.Server/CribaDoc.Server.csproj src/CribaDoc.Server/

RUN dotnet restore src/CribaDoc.Server/CribaDoc.Server.csproj

COPY src/CribaDoc.Core/ src/CribaDoc.Core/
COPY src/CribaDoc.Server/ src/CribaDoc.Server/

WORKDIR /src/src/CribaDoc.Server
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CribaDoc.Server.dll"]