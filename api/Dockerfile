FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Nabiz.Domain/*.csproj Nabiz.Domain/
COPY Nabiz.Application/*.csproj Nabiz.Application/
COPY Nabiz.Infrastructure/*.csproj Nabiz.Infrastructure/
COPY Nabiz.Api/*.csproj Nabiz.Api/
RUN dotnet restore Nabiz.Api
COPY . .
RUN dotnet publish Nabiz.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nabiz.Api.dll"]
