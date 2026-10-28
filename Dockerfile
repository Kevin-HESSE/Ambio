FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ENV HUSKY=0
WORKDIR /source

COPY global.json Directory.Build.props Directory.Build.targets Directory.Packages.props .editorconfig ./
COPY src/ src/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/Ambio.Web -c Release -a $TARGETARCH -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN mkdir /data && chown $APP_UID /data
COPY --from=build /app .

ENV ConnectionStrings__DefaultConnection="Data Source=/data/ambio.db" \
    Ambio__Storage__Root=/data \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command=Warning

VOLUME /data
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Ambio.Web.dll"]
