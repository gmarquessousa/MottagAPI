# Mottag API

API em .NET 9 demonstrando padrões REST (CRUD, paginação, HATEOAS leve, validação, ProblemDetails) sobre um domínio simples (Pátio, Moto, Tag).

## Infraestrutura na Nuvem (Azure)
Ambiente pensado de forma simples para demonstrar separação de responsabilidades:
- 1 Azure App Service (Web App) para hospedar a API (.NET 9)
- 1 Azure App Service Plan (Camada B1) – SKU básica de baixo custo
- 1 Azure SQL Server (servidor lógico) + 1 Banco Azure SQL (Edition Basic)

Fluxo geral:
Cliente -> Web App (Kestrel/ASP.NET) -> EF Core -> Azure SQL

Boas práticas (aplicar gradualmente):
- Usar Managed Identity (em vez de usuário/senha) para acesso ao banco (requer configurar AAD admin e GRANT)
- Ativar Logging estruturado (Serilog) integrado ao Application Insights (futuro)
- Restringir firewall do SQL a range específico / usar Private Endpoint em produção
- TLS mínimo 1.2 (já aplicado no comando de update do server)
- Connection string via Configuração do App Service (não commitada)

## Banco de Dados (Modelo Físico)
Criado via EF Core Migrations (migração inicial `Initial`). Tabelas:
- Patios (Id, Nome, Cidade, Estado, Pais, AreaM2)
- Motos (Id, PatioId (FK), Placa (Unique), Modelo, Status)
- Tags (Id, MotoId (FK nullável com SetNull), Serial (Unique), Tipo, BateriaPct, LastSeenAt)

Índices / Restrições:
- UNIQUE em Motos.Placa
- UNIQUE em Tags.Serial
- UNIQUE em Tags.MotoId (uma Tag por Moto no máximo)
- FK Motos -> Patios (Restrict delete)
- FK Tags -> Motos (SetNull delete)

Migração automática: Em `Program.cs` há `db.Database.Migrate()` na inicialização — garante que o schema suba/atualize no start. Em produção, pode-se trocar por pipeline de migração controlada.

## Esquema Conceitual
```
Patio (1) ──< Moto (N) ──(0..1) Tag
```

## Implantação (Deploy) via VS Code (Extensão Azure)
Pré-requisitos:
1. Extensão "Azure Tools" (ou pelo menos "Azure App Service") instalada
2. Login: use o botão "Sign in to Azure" na aba Azure ou `Azure: Sign In`
3. .NET SDK 9 instalado localmente
4. App Service e SQL já criados (ou criar conforme comandos abaixo)

Passos:
1. Build local para validar:
	 ```bash
	 dotnet build mottag-api.sln
	 dotnet test tests/App.Tests/App.Tests.csproj -v minimal
	 ```
2. Ajustar connection string local (user-secrets ou `appsettings.Development.json`)
3. Garantir que a migration inicial existe (já existe `Initial`)
4. Na aba Azure (ícone de nuvem no VS Code), expandir "App Service"
5. Botão direito no Web App (`mottag-webapp-api`) → "Deploy to Web App"
6. Selecionar a pasta `src/App.Api`
7. Confirmar prompt de substituição (deploy zip) – isso publica build Release
8. Após publicar: copiar URL exibida no output/log
9. Configurar Connection String no portal ou via CLI (ver seção abaixo)
10. Reiniciar Web App se alterar configurações

Connection String no Azure (exemplo – usar senha segura):
No portal: Web App > Configuration > Connection strings > Add
	- Name: DefaultConnection
	- Value: `Server=tcp:fiapadmin.database.windows.net,1433;Initial Catalog=dbsql-mottag;Persist Security Info=False;User ID=fiapadmin;Password=<SENHA>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
	- Type: SQLAzure

Depois do deploy: acessar `https://mottag-webapp-api.azurewebsites.net/swagger` (se ambiente não for configurado como Development, habilitar Swagger também em produção ou usar feature flag).

### Alternativa: Deploy por CLI
```bash
az webapp deploy --resource-group mottag --name mottag-webapp-api --type zip --src-path <arquivo-zip-ou-pasta>
```
Pode gerar o zip com:
```bash
dotnet publish src/App.Api -c Release -o publish
cd publish
Compress-Archive * ../app.zip
```

## Comandos Azure CLI Utilizados (Histórico)
Criação do servidor SQL:
```bash
az sql server create --resource-group mottag --name fiapadmin --location brazilsouth \
	--admin-user fiapadmin --admin-password "<SENHA_FORTE>"
```
Criar banco:
```bash
az sql db create -g mottag -s fiapadmin -n dbsql-mottag --edition Basic \
	--service-objective Basic --collation SQL_Latin1_General_CP1_CI_AS \
	--backup-storage-redundancy Geo
```
Hardening inicial:
```bash
az sql server update -g mottag -n fiapadmin --minimal-tls-version 1.2 --enable-public-network true
```
Liberar Azure Services (firewall):
```bash
az sql server firewall-rule create -g mottag -s fiapadmin -n AllowAzureServices \
	--start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```
App Service Plan:
```bash
az appservice plan create -g mottag -n plan-mottag-windows --sku B1
```
Web App:
```bash
az webapp create -g mottag -p plan-mottag-windows -n mottag-webapp-api
```

### (Opcional) Connection String via CLI
```bash
az webapp config connection-string set -g mottag -n mottag-webapp-api \
	-t SQLAzure --settings DefaultConnection="Server=tcp:fiapadmin.database.windows.net,1433;Initial Catalog=dbsql-mottag;User ID=fiapadmin;Password=<SENHA>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### (Futuro) Migração para Managed Identity
1. Ativar Managed Identity no Web App (Identity > System Assigned = On)
2. No Azure SQL: adicionar AAD admin e dar permissões: `CREATE USER [mottag-webapp-api] FROM EXTERNAL PROVIDER; ALTER ROLE db_datareader ADD MEMBER ...;` etc.
3. Ajustar connection string para usar `Authentication=Active Directory Default;` (sem senha)

### Observações de Produção
- Desabilitar `db.Database.Migrate()` em runtime e mover migrações para pipeline controlado
- Habilitar Application Insights
- Configurar autoscale (SKU > B1 não tem; considerar S1 ou Premium)
- Private Endpoint / VNet Integration
- Backup automatizado (já incluso no Azure SQL; definir retenção)

## Equipe
Gabriel Marques de Lima Sousa - RM554889  
Leonardo Menezes Parpinelli Ribas - RM557908  
Leonardo Matheus Teixeira - RM556629

## Domínio
Pátio (1) -> Motos (N) -> Tag (0..1). Tag e Placa são únicas.
```
Patio ──< Moto ──(0..1) Tag
```

## Arquitetura
Domain | Infrastructure (EF Core) | Application (DTOs, Services, Validation, Mapping, Query Extensions, HATEOAS) | Api (Controllers, Swagger, Middleware) | Tests.

## Principais Features
- CRUD (3 entidades)
- Paginação (page, pageSize, total, links)
- Filtros + ordenação (search / ids / status / serial / sortBy / sortDir)
- HATEOAS básico em coleções
- Validação FluentValidation
- Erros padronizados (ProblemDetails)
- Unicidade: placa (Moto), serial (Tag)
- Swagger com exemplos

## Executar
```bash
dotnet build mottag-api.sln
dotnet run --project src/App.Api
```
Swagger: http://localhost:<porta>/swagger

## Banco de Dados & Migrations
Definir `ConnectionStrings:DefaultConnection` via user-secrets ou variável de ambiente.
Gerar nova migration (se modelo mudar):
```bash
dotnet ef migrations add Nome -p src/App.Infrastructure -s src/App.Api
dotnet ef database update -p src/App.Infrastructure -s src/App.Api
```
Migração atual: `Initial`.

## Paginação (Contrato)
```json
{ "items": [], "total": 0, "page": 1, "pageSize": 10, "links": { "self": "...", "next": null, "prev": null } }
```

## Erros (ProblemDetails)
Exemplo 409:
```json
{ "title": "Conflict", "status": 409, "detail": "Serial já cadastrado" }
```

## Testes
```bash
dotnet test tests/App.Tests/App.Tests.csproj -v minimal
```

## Script Opcional
`scripts/demo_flow.py` executa fluxo rápido (criação + conflitos) para smoke test

## Tutorial Rápido de Uso (cURL)
Base URL assumida: `http://localhost:5000` (ajuste porta conforme execução).

1. Criar Pátio
```bash
curl -s -X POST http://localhost:5000/api/v1/patios \
	-H "Content-Type: application/json" \
	-d '{"nome":"Central","cidade":"São Paulo","estado":"SP","pais":"BR","areaM2":1200}' | jq
```
Copie o `id` retornado (ex: `PATIO_ID`).

2. Listar Pátios (com paginação + busca parcial)
```bash
curl -s "http://localhost:5000/api/v1/patios?page=1&pageSize=5&search=Cent" | jq
```

3. Criar Moto vinculada ao pátio
```bash
curl -s -X POST http://localhost:5000/api/v1/motos \
	-H "Content-Type: application/json" \
	-d '{"patioId":"PATIO_ID","placa":"ABC1D23","modelo":"CG 160","status":"Ativa"}' | jq
```
Copie o `id` da moto (ex: `MOTO_ID`).

4. Criar Tag (associada à moto)
```bash
curl -s -X POST http://localhost:5000/api/v1/tags \
	-H "Content-Type: application/json" \
	-d '{"motoId":"MOTO_ID","serial":"TAG-0001","tipo":"V1","bateriaPct":90}' | jq
```

5. Paginar Motos filtrando por pátio
```bash
curl -s "http://localhost:5000/api/v1/motos?patioId=PATIO_ID&page=1&pageSize=10&sortBy=placa&sortDir=asc" | jq
```

6. Ver Tag por serial (filtro)
```bash
curl -s "http://localhost:5000/api/v1/tags?serial=TAG-0001" | jq
```

7. Testar conflito (placa duplicada) – espera 409
```bash
curl -i -X POST http://localhost:5000/api/v1/motos \
	-H "Content-Type: application/json" \
	-d '{"patioId":"PATIO_ID","placa":"ABC1D23","modelo":"Outro","status":"Ativa"}'
```

8. Navegação HATEOAS (links self/next/prev) já aparece nas respostas paginadas; ver campo `links`.

Observações:
- Substitua PATIO_ID e MOTO_ID pelos GUIDs reais.
- Remover `| jq` se não tiver jq instalado.
- Ajustar porta conforme log de execução.
