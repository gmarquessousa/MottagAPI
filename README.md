# Mottag API

API em .NET 9 demonstrando padrões REST (CRUD, paginação, HATEOAS leve, validação, ProblemDetails) sobre um domínio simples (Pátio, Moto, Tag).

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
