# Postulo

**Track your job applications, keep every CV and cover letter you sent, and generate tailored CVs from a single profile.**

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?logo=blazor)
![SQLite](https://img.shields.io/badge/SQLite-EF%20Core-003B57?logo=sqlite)
![QuestPDF](https://img.shields.io/badge/PDF-QuestPDF-2C3E50)
![Docker](https://img.shields.io/badge/Docker-amd64%20%7C%20arm64-2496ED?logo=docker)
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

> 🚧 Early development — see the [roadmap](docs/roadmap.md).

## Features

- **Application tracking**: companies, job offers, status workflow (Applied → Interview → Offer / Rejected…) with a full history, and a dashboard.
- **Cover letters**: plain-text letters attached to each application.
- **CVs per application**: upload the CV you sent, or generate one from your profile by selecting the relevant experiences and tailoring the summary. Each CV is frozen, so you always know what was sent.
- **General CV**: a CV not tied to any offer, ready to share on LinkedIn and other networks.
- **Template plugins**: CV layouts are [QuestPDF](https://www.questpdf.com/) templates loaded from a `/plugins` folder. Write your own in C#.
- **Self-hosted**: a single Docker image for amd64 and arm64, with all data in one volume.
- **Secure by default**: single-user, 2FA, optional GitHub login.
- **FR / EN** interface.

## Screenshots

_Coming soon._ Screenshots will live in [`docs/screenshots/`](docs/screenshots/).

<!--
![Dashboard](docs/screenshots/dashboard.png)
![Application detail](docs/screenshots/application-detail.png)
![CV generation](docs/screenshots/cv-generation.png)
-->

## Quick start

### Docker

```bash
docker run -d --name postulo \
  -p 8080:8080 \
  -v ./data:/data \
  -v ./plugins:/plugins \
  <dockerhub-user>/postulo:latest
```

Open <http://localhost:8080> and create your account. Add `-e Postulo__Demo__Enabled=true` to explore with demo data.

See [deployment](docs/deployment.md) for `docker compose`, reverse proxy, email and backups.

### From source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/Postulo.Web
```

## Write your own CV template

```csharp
public sealed class ModernTemplate : ICvTemplate
{
    public string Id => "acme.modern";
    public string Name => "Modern";
    public Version Version => new(1, 0, 0);

    public void Compose(IDocumentContainer container, CvModel model) =>
        container.Page(page =>
        {
            page.Header().Text(model.FullName).FontSize(24).Bold();
            // ...
        });
}
```

Publish it, drop the folder into `/plugins`, and restart. See the [plugin guide](docs/plugins.md).

## Documentation

- [Roadmap](docs/roadmap.md)
- [Architecture](docs/architecture.md)
- [CV template plugins](docs/plugins.md)
- [Deployment](docs/deployment.md)
- [Testing](docs/testing.md)
- [Architecture Decision Records](docs/adr/README.md)

## Tech stack

- .NET 10 
- Blazor Web App (Interactive Server) 
- ASP.NET Core Identity 
- EF Core + SQLite 
- QuestPDF 
- xUnit + bUnit 
- Docker 
- GitHub Actions

## License

Postulo is released under the [MIT License](LICENSE).

PDF generation uses [QuestPDF](https://www.questpdf.com/), used under its [Community License](https://www.questpdf.com/license/), which applies to open-source projects like this one.
