# CI/CD Pipeline

> Build, test, pack a publish pipeline pro MetaForge. GitHub Actions jako primární CI platforma.

---

## Principy

1. **Každý PR musí projít buildem a testy** — žádný merge bez zeleného CI.
2. **Testy jsou rozděleny do vrstev** — Core testy rychlé, Translator testy pomalejší.
3. **NuGet balíčky se publikují pouze z main větve** — žádné pre-release z feature branche.
4. **ForgeBlock balíčky jsou publikovány samostatně** — každý ForgeBlock má vlastní verzi.
5. **Docker image pouze pro WebApi** — CLI a MCP jsou konzolové aplikace.

---

## GitHub Actions — workflow struktura

```
.github/workflows/
├── build.yml              # Build + test pro všechny PR a commity
├── publish-nuget.yml      # Publikace NuGet balíčků
├── docker-webapi.yml      # Docker image pro WebApi
└── release.yml            # Release pipeline (tag-based)
```

---

## build.yml — Build a test

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Restore
      run: dotnet restore MetaForge.slnx

    - name: Build
      run: dotnet build MetaForge.slnx --no-restore --configuration Release

    - name: Test Core
      run: dotnet test Tests/MetaForge.Core.Tests --no-build --configuration Release

    - name: Test BusinessModel
      run: dotnet test Tests/MetaForge.BusinessModel.Tests --no-build --configuration Release

    - name: Test Translator
      run: dotnet test Tests/MetaForge.Translator.Tests --no-build --configuration Release

    - name: Test Generators
      run: dotnet test Tests/MetaForge.Generators.Tests --no-build --configuration Release
```

---

## publish-nuget.yml — Publikace NuGet

```yaml
name: Publish NuGet Packages

on:
  push:
    tags:
      - 'v*'

jobs:
  publish:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Pack Core
      run: dotnet pack Src/MetaForge.Core --configuration Release -o ./packages

    - name: Pack BusinessModel
      run: dotnet pack Src/MetaForge.BusinessModel --configuration Release -o ./packages

    - name: Pack Translator
      run: dotnet pack Src/MetaForge.Translator --configuration Release -o ./packages

    - name: Pack Generators
      run: dotnet pack Src/MetaForge.Generators --configuration Release -o ./packages

    - name: Push to NuGet
      run: dotnet nuget push ./packages/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

---

## docker-webapi.yml — Docker image

```yaml
name: Docker WebApi

on:
  push:
    branches: [ main ]
    paths:
      - 'Src/MetaForge.WebApi/**'

jobs:
  docker:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Build WebApi
      run: dotnet publish Src/MetaForge.WebApi --configuration Release -o ./publish

    - name: Build Docker image
      run: docker build -t metaforge-webapi:latest -f Src/MetaForge.WebApi/Dockerfile .

    - name: Push to registry
      run: |
        docker tag metaforge-webapi:latest ${{ secrets.REGISTRY_URL }}/metaforge-webapi:${{ github.sha }}
        docker push ${{ secrets.REGISTRY_URL }}/metaforge-webapi:${{ github.sha }}
```

---

## Dockerfile (WebApi)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore MetaForge.slnx
RUN dotnet publish Src/MetaForge.WebApi -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MetaForge.WebApi.dll"]
```

---

## Verzování

| Komponenta | Verzovací schéma |
|-----------|-----------------|
| MetaForge.Core | `{major}.{minor}.{patch}` — dle SDK spec |
| MetaForge.BusinessModel | `{major}.{minor}.{patch}` — dle změn v modelu |
| MetaForge.Translator | `{major}.{minor}.{patch}` — dle API změn |
| MetaForge.Generators | `{major}.{minor}.{patch}` — dle výstupu |
| ForgeBlocks.* | `{major}.{minor}.{patch}` — nezávislé na Core |

### Version tagging

```
v1.0.0           # Release Core + BusinessModel
v1.0.1           # Patch
v1.1.0           # Minor — nová feature
forgeblock-math-v1.0.0  # ForgeBlock release
```
