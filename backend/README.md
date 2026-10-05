# Backend

API ASP.NET Core Minimal APIs em .NET 10, organizada por fatias verticais e persistida em PostgreSQL 18 com Entity Framework Core.

## Estrutura

```text
backend/
├── src/TodoList.Api/
│   ├── Common/                  # health, limite e logging seguro
│   ├── Data/                    # DbContext e design-time factory
│   ├── Features/Tasks/          # criação, leitura, edição e estado
│   └── Migrations/              # migrações versionadas
├── tests/TodoList.Api.UnitTests/
├── tests/TodoList.Api.IntegrationTests/
├── Directory.Packages.props     # versões centralizadas
└── TodoList.slnx
```

## Configuração

| Chave/variável | Exemplo local | Uso |
|---|---|---|
| `ConnectionStrings__TodoList` | `Host=localhost;Port=5432;Database=todolist;Username=todolist;Password=todolist-local` | conexão PostgreSQL |
| `Cors__AllowedOrigin` | `http://localhost:5173` | única origem HTTP(S) autorizada, sem curinga |

Os exemplos são apenas locais. Não versione credenciais reais nem habilite logging de dados sensíveis.

## Banco e migrações

```bash
docker-compose -f compose.yaml up -d --wait
dotnet tool restore
dotnet restore backend/TodoList.slnx --locked-mode
dotnet ef database update --project backend/src/TodoList.Api/TodoList.Api.csproj
```

Para criar uma migração futura, primeiro siga o fluxo SDD correspondente; então use `dotnet ef migrations add <Nome> --project backend/src/TodoList.Api/TodoList.Api.csproj`. A API não aplica migrações automaticamente no startup.

## Execução e testes

```bash
dotnet run --project backend/src/TodoList.Api/TodoList.Api.csproj --urls http://localhost:5080
dotnet build backend/TodoList.slnx --no-restore --disable-build-servers -m:1
dotnet format backend/TodoList.slnx --verify-no-changes --no-restore
dotnet test backend/TodoList.slnx --no-restore --disable-build-servers -m:1
```

- `/health/live` verifica o processo sem depender do banco;
- `/health/ready` inclui conectividade PostgreSQL;
- `/openapi/v1.json` fica disponível em `Development` e `Testing`;
- integração usa PostgreSQL efêmero via Testcontainers;
- testes de contrato comparam semanticamente o OpenAPI runtime ao artefato aprovado.

## Proteção pública

A SPEC-001 é deliberadamente de usuário único e não possui autenticação. A API de escrita **não deve ser publicada em acesso aberto**. Uma publicação exige barreira de acesso da plataforma ou uma nova spec de identidade/autorização.
