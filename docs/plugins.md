# CV template plugins

Ambio renders CVs with [QuestPDF](https://www.questpdf.com/). Templates are plugins: .NET assemblies that implement a small contract and are loaded from the `/plugins` folder when the app starts.

> Status: planned for [Phase 4](roadmap.md#phase-4--cv-template-plugins). The contract below is the target design and may change before release.

## Naming convention

Plugin projects are named after their purpose: `Ambio.Plugins.<Domain>.<Purpose>`.

| Project | Role |
|---|---|
| `Ambio.Plugins.CV.Abstractions` | Contract shared by the host and every CV template |
| `Ambio.Plugins.CV.Default` | Built-in template, also the reference implementation |
| `Ambio.Plugins.CV.<Name>` | Any other CV template, for example `Ambio.Plugins.CV.Modern` |

The `<Domain>` segment leaves room for other plugin families later.

## Contract

```csharp
namespace Ambio.Plugins.CV.Abstractions;

public interface ICvTemplate
{
    string Id { get; }          // stable, unique: "ambio.default"
    string Name { get; }        // shown in the template picker
    Version Version { get; }

    void Compose(IDocumentContainer container, CvModel model);
}
```

`CvModel` is a read-only snapshot of what the CV contains:

- identity: name, headline, contact details, links
- summary, which may have been overridden for a specific job offer
- selected experiences, education, skills, languages
- optional **photo** (`byte[]?`): templates can ignore it
- target application (company, job title) when the CV is tied to an application, `null` for a general CV

Templates only receive data. They never access the database or the file system.

## Packaging and installation

```text
/plugins
  Ambio.Plugins.CV.Modern/
    Ambio.Plugins.CV.Modern.dll
    <private dependencies>.dll
```

1. Reference `Ambio.Plugins.CV.Abstractions` and `QuestPDF` with `<Private>false</Private>` / `ExcludeAssets="runtime"` so they are **not** copied to the output.
2. `dotnet publish` the plugin.
3. Copy the output folder into the `/plugins` volume.
4. Restart the container.

## Loading

- Plugins are discovered **once, at startup** ([ADR 0003](adr/0003-runtime-plugin-loading-at-startup.md)). Adding or updating a plugin requires a restart.
- Each plugin folder gets its own `AssemblyLoadContext`.
- `Ambio.Plugins.CV.Abstractions` and `QuestPDF` are always resolved from the host, so the types match. Other dependencies are resolved from the plugin folder.
- A plugin that fails to load (missing dependency, incompatible contract version, duplicate `Id`) is logged and skipped. A template that throws while rendering produces an error for that CV only.

## Versioning

`Ambio.Plugins.CV.Abstractions` follows semantic versioning. The host refuses plugins built against a different **major** version.

## Developing a template

`tools/Ambio.CvPlayground` is a console app that renders a template with sample data and opens it in the [QuestPDF Companion](https://www.questpdf.com/companion/usage.html), which gives live preview while you edit the layout.

```bash
dotnet run --project tools/Ambio.CvPlayground -- --template ambio.default
```

Add a PDF snapshot test for every template (see [testing.md](testing.md)).

## Trust

Plugins run in-process with the same permissions as the app. Ambio is a single-user, self-hosted application: only install plugins you trust.
