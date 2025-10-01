<h1 align="center">Mottag API</h1>
<p align="center">API .NET 9 para gestão de Pátios, Motos e Tags com CRUD, paginação, filtros, HATEOAS leve, validação e Swagger sempre habilitado.</p>

---
### Integrantes
Gabriel Marques de Lima Sousa (RM554889)  
Leonardo Menezes Parpinelli Ribas (RM557908)  
Leonardo Matheus Teixeira (RM556629)

### Justificativa da Arquitetura
Aplicamos arquitetura em camadas para separar responsabilidades e facilitar testes e evolução:
- Domain: Entidades ricas + regras de negócio essenciais.
- Infrastructure: Persistência (EF Core / Migrations) isolada atrás de repositórios/DbContext.
- Application: DTOs, validações (FluentValidation), serviços orquestrando regras e mapeamentos (AutoMapper) sem depender de detalhes de infra.
- Api: Controllers enxutos, versionamento, ProblemDetails, filtros, documentação (Swagger) e middlewares transversais.
- Tests: Garantem integridade de serviços e validações sem acoplamento a infraestrutura real.
Benefícios: baixa acoplamento, alta coesão, testabilidade, possibilidade futura de trocar persistência (ex: outro banco) ou expor nova camada (ex: gRPC) sem reescrever domínio.

### Instruções de Execução Rápida
Pré-requisitos: .NET 9 SDK, SQL Server disponível e string de conexão configurada.
```bash
dotnet restore
dotnet build mottag-api.sln
dotnet run --project src/App.Api
```
Acessar: `/swagger` (docs) e `/health` (sanidade). Para rodar com user-secrets configure a connection string (ver bloco de Configuração mais abaixo).

### Exemplos de Uso de Endpoints
Base URL: `/api/v1`
- Criar Pátio: `POST /api/v1/patios`  
   Body: `{ "nome":"Pátio Central","cidade":"São Paulo","estado":"SP","pais":"BR","areaM2":1500 }`
- Criar Moto: `POST /api/v1/motos`  
   Body: `{ "patioId":"<GUID_PATIO>","placa":"ABC1D23","modelo":"Yamaha MT-07","status":0 }`
- Criar Tag: `POST /api/v1/tags`  
   Body: `{ "motoId":"<GUID_MOTO>","serial":"TAG-0001","tipo":0,"bateriaPct":100,"lastSeenAt":null }`
- Listar Motos filtrando: `GET /api/v1/motos?patioId=<GUID_PATIO>&status=0&page=1&pageSize=10`
- Atualizar Moto: `PUT /api/v1/motos/{id}`
- Excluir Tag: `DELETE /api/v1/tags/{id}`
Resposta paginada exemplo:
```json
{ "items":[], "total":0, "page":1, "pageSize":10, "links": {"self":"...","next":null,"prev":null} }
```
Erro de conflito exemplo:
```json
{ "title":"Conflict", "status":409, "detail":"Serial já cadastrado" }
```

### Comando para Rodar os Testes
```bash
dotnet test tests/App.Tests/App.Tests.csproj -v minimal
```

---
As seções abaixo detalham profundamente cada aspecto (configuração, deploy, troubleshooting, roadmap, etc.).

## 1. Visão Geral
Camadas: Domain → Infrastructure (EF Core) → Application (DTOs/Services/Validation/Mapping) → Api (Controllers/Middlewares/Swagger) → Tests.

Relacionamento:
```
Patio (1) --< Moto (N) --(0..1) Tag
```

Principais features:
- CRUD + paginação + filtros (`search`, `patioId`, `status`, `serial`, `sortBy/sortDir`)
- HATEOAS simples (links em coleções e itens)
- FluentValidation + ProblemDetails
- Unicidade: Placa (Moto), Serial (Tag)
- Swagger + exemplos (sempre ativo; opcional AccessKey)

## 2. Requisitos
- .NET 9 SDK (global.json exige 9.0.100)
- SQL Server / Azure SQL
- (Prod) Azure App Service (Windows) + Connection String configurada

## 3. Variáveis e Configuração
Obrigatória:
| Chave | Onde | Uso |
|-------|------|-----|
| ConnectionStrings:DefaultConnection | App Settings ou aba Connection Strings | EF Core | 

Opcionais:
| Chave | Finalidade |
|-------|-----------|
| ASPNETCORE_ENVIRONMENT | Normalmente Production em Azure |
| Swagger:AccessKey | Protege /swagger exigindo header X-Swagger-Key |
| Logging:LogLevel:Default | Ajustar verbosidade |
| Logging:LogLevel:Microsoft.AspNetCore | Reduz ruído |

Local (user-secrets):
```bash
dotnet user-secrets init --project src/App.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;User ID=...;Password=...;Encrypt=True;TrustServerCertificate=False;"
```

## 4. Executar Local
```bash
dotnet restore
dotnet build mottag-api.sln
dotnet run --project src/App.Api
```
Endpoints: `/swagger`, `/health`.

## 5. Migrations
`db.Database.Migrate()` roda no startup.
Gerar nova migration:
```bash
dotnet ef migrations add Nome -p src/App.Infrastructure -s src/App.Api
dotnet ef database update -p src/App.Infrastructure -s src.App.Api
```
Migração atual: `Initial`.

## 6. Deploy (Passo a Passo Correto)
1. Publicar:
   ```bash
   dotnet publish src/App.Api/App.Api.csproj -c Release -o publish
   ```
2. Conferir `publish/` (tem `web.config` + `App.Api.dll`).
3. Compactar conteúdo (não a pasta):
   ```bash
   Compress-Archive -Path publish/* -DestinationPath deploy.zip -Force
   ```
4. Portal Azure → Deployment Center → Zip Deploy (enviar `deploy.zip`).
5. Configurar Connection String `DefaultConnection`.
6. Restart Web App.
7. Testar: `/health` e `/swagger`.

Alternativas: VS Code (Deploy para Web App apontando a pasta `publish`) ou Visual Studio Publish Profile.

### Erro 403.14? (Deploy incorreto)
Significa que você subiu código-fonte cru (sem publish) → repita os passos acima.

## 7. Endpoints
Base `/api/v1`:
- Patios: `GET /patios`, `GET /patios/{id}`, `POST /patios`, `PUT /patios/{id}`, `DELETE /patios/{id}`
- Motos: `GET /motos`, `GET /motos/{id}`, `POST /motos`, `PUT /motos/{id}`, `DELETE /motos/{id}`
- Tags:  `GET /tags`,  `GET /tags/{id}`,  `POST /tags`,  `PUT /tags/{id}`,  `DELETE /tags/{id}`

Parâmetros de filtro/paginação:
| Recurso | Filtros |
|---------|---------|
| Patios | search, sortBy, sortDir, page, pageSize |
| Motos  | patioId, status, placa, sortBy, sortDir, page, pageSize |
| Tags   | serial, sortBy, sortDir, page, pageSize |

## 8. Exemplo de Fluxo (Swagger ou cURL)
1. Criar Pátio (POST /patios)
2. Criar Moto usando `patioId` (POST /motos)
3. Criar Tag (POST /tags – opcionalmente associada à moto)
4. Listar + filtrar (GET /motos?patioId=... etc.)
5. Atualizar (PUT)
6. Deletar Tag → Moto → Pátio

Payloads:
```json
// Patio Create
{ "nome":"Pátio Central","cidade":"São Paulo","estado":"SP","pais":"BR","areaM2":1500 }

// Moto Create
{ "patioId": "<GUID_PATIO>", "placa":"ABC1D23", "modelo":"Yamaha MT-07", "status":0 }

// Tag Create
{ "motoId": "<GUID_MOTO>", "serial":"TAG-0001", "tipo":0, "bateriaPct":100, "lastSeenAt": null }
```

## 9. Padrão de Paginação
```json
{
  "items": [],
  "total": 0,
  "page": 1,
  "pageSize": 10,
  "links": { "self": "...", "next": null, "prev": null }
}
```

## 10. Erros (ProblemDetails)
Exemplo conflito:
```json
{ "title":"Conflict", "status":409, "detail":"Serial já cadastrado" }
```
Concorrência (rowVersion incorreta) → 412.

## 11. Health & Observabilidade
- `/health` simples JSON.
- Logs de startup: `[Startup][Diag]` e `[Startup][Migrate]` (ver Log Stream).
- Futuro: integrar Application Insights / Managed Identity.

## 12. Testes
```bash
dotnet test tests/App.Tests/App.Tests.csproj -v minimal
```

## 13. Troubleshooting Rápido
| Sintoma | Causa | Ação |
|---------|-------|------|
| 403.14 (Forbidden) | Deploy sem publish | Refazer `dotnet publish` + zip deploy |
| 404 em /swagger | App não iniciou | Ver logs / connection string |
| 409 | Duplicidade (placa / serial) | Alterar valor |
| 412 | Concurrency / rowVersion | Recarregar e reenviar |
| 500 inicial | Connection string inválida | Ajustar config e restart |

## 14. Infra & Banco de Dados
Infraestrutura lógica:
- Persistência: EF Core + SQL Server. Contexto `AppDbContext` expõe `Patios`, `Motos`, `Tags`.
- Registro: `AddAppPersistence` configura DbContext com connection string `DefaultConnection` e registra `IRepository<>` genérico (`EfRepository`).
- Migrations: executadas no startup via `db.Database.Migrate()` (migração atual: `Initial`).

Modelo relacional (tabelas):
- Patios (Id PK, Nome, Cidade, Estado, Pais, AreaM2)
- Motos (Id PK, PatioId FK restrito, Placa UNIQUE, Modelo, Status com constraint de valores)
- Tags (Id PK, MotoId FK opcional UNIQUE filtrado, Serial UNIQUE, Tipo, BateriaPct, LastSeenAt)

Relacionamentos e regras:
- 1 Patio -> N Motos (`Restrict` ao deletar para evitar cascatas múltiplas)
- 1 Moto -> 0..1 Tag (FK em Tag com `SetNull` no delete)
- Unique indexes: Placa (Moto), Serial (Tag), MotoId (Tag) com filtro `[MotoId] IS NOT NULL`.
- Check constraints: `CK_Moto_Status` (Status ∈ {0,1,2,3}); `CK_Tag_BateriaPct` (0–100) via config.

Camada de acesso:
- Repositório genérico encapsula CRUD básico; serviços de Application layer orquestram validação e regras antes de persistir.

Considerações de evolução:
- Possível adicionar soft delete (coluna IsDeleted) sem quebrar desenho atual.
- Para escala de leitura: adicionar caching (ex: Redis) na camada Application sem tocar domínio.
- Para auditoria: incluir CreatedAt/UpdatedAt via interceptors EF.

Desempenho & Índices:
- Índices nas colunas de busca principais: Nome (Patio), Placa (Moto), Serial (Tag), PatioId+Status (Moto), MotoId filtrado (Tag).
- Paginação feita via query com filtros opcionais (nenhuma projeção pesada no DbContext).

Integridade:
- Delete restrito evita remoção acidental em cascata de Motos ao remover Pátio.
- SetNull em Tag preserva histórico de Tag mesmo sem Moto.

Segurança de conexão:
- Recomenda-se usar Azure SQL com Encrypt=True e Managed Identity futura (atual: string com credenciais).

