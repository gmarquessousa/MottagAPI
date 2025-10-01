// main.bicep - Infraestrutura base para Mottag API
// Observações:
// - Usa Azure SQL, App Service, Key Vault, Application Insights
// - Recomenda-se habilitar Managed Identity e armazenar segredos no Key Vault
// - Ajuste nomes/locais conforme necessidade

@description('Prefixo único para recursos (minúsculo, sem espaços).')
param namePrefix string

@description('Localização padrão dos recursos.')
param location string = resourceGroup().location

@description('SKU do App Service Plan')
@allowed([ 'F1' 'B1' 'P0v3' 'P1v3' ])
param appServiceSku string = 'B1'

@description('SKU do Azure SQL (camada).')
param sqlSkuName string = 'GP_S_Gen5'

@description('Tamanho da instância do Azure SQL.')
param sqlSkuTier string = 'GeneralPurpose'

@description('Nome do administrador lógico (apenas provisório; depois substituir por AAD).')
param sqlAdministratorLogin string

@secure()
@description('Senha do administrador do Azure SQL (evitar uso em produção; usar AAD/Managed Identity).')
param sqlAdministratorPassword string

var appServicePlanName = '${namePrefix}-plan'
var appServiceName = '${namePrefix}-api'
var appInsightsName = '${namePrefix}-appi'
var keyVaultName = toLower('${namePrefix}kv')
var sqlServerName = toLower('${namePrefix}sqlsrv')
var sqlDbName = '${namePrefix}db'

// App Service Plan
resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServiceSku
    tier: appServiceSku == 'F1' ? 'Free' : appServiceSku == 'B1' ? 'Basic' : 'PremiumV3'
  }
  properties: {
    reserved: false
  }
}

// Application Insights
resource appi 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
  }
}

// Key Vault
resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enablePurgeProtection: true
    enableSoftDelete: true
    accessPolicies: [] // Usar RBAC + Managed Identity
    publicNetworkAccess: 'Enabled'
  }
}

// SQL Logical Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
    publicNetworkAccess: 'Enabled'
    version: '12.0'
  }
}

// SQL Database
resource sqlDb 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  name: '${sqlServer.name}/${sqlDbName}'
  location: location
  sku: {
    name: sqlSkuName
    tier: sqlSkuTier
  }
  properties: {
    readScale: 'Disabled'
    zoneRedundant: false
  }
  dependsOn: [ sqlServer ]
}

// App Service com Managed Identity
resource web 'Microsoft.Web/sites@2023-12-01' = {
  name: appServiceName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ApplicationInsights__ConnectionString'
          value: appi.properties.ConnectionString
        }
      ]
    }
  }
  tags: {
    'component': 'api'
  }
}

// Exemplo de secret (connection string) - recomenda-se inserir via pipeline / script, não embedar
resource kvSecretConn 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(sqlAdministratorLogin)) {
  name: '${kv.name}/SqlConnectionString'
  properties: {
    value: 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Database=${sqlDbName};Authentication=Active Directory Default;Encrypt=True;'
  }
  dependsOn: [ kv, sqlDb ]
}

// Permissão do App Service para ler Key Vault (Role Assignment via script externo normalmente)
output webAppName string = web.name
output sqlFqdn string = sqlServer.properties.fullyQualifiedDomainName
output keyVaultUri string = kv.properties.vaultUri
output appInsightsConnection string = appi.properties.ConnectionString
