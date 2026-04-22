# Backend AquaMonitor - Guia de Implementação

## 📋 Visão Geral

Este é um backend completo em C# .NET 10 implementado com os seguintes requisitos:
- ✅ Banco de dados MySQL
- ✅ Arquitetura Multitenant
- ✅ Estrutura MVC
- ✅ Código 100% em português
- ✅ API RESTful completa

## 🏗️ Arquitetura

### Estrutura de Pastas
```
Backend/
├── Controllers/          # AutenticacaoController, RegistrosAguaController, NotificacoesController, DashboardController
├── Models/               # Entidades: Tenant, Usuario, RegistroAqua, Notificacao
├── DTOs/                 # Data Transfer Objects para requisições/respostas
├── Data/                 # AppDbContext e interfaces de Tenant
├── Services/             # ProvedorTenantService
├── Program.cs            # Configuração e inicialização
├── appsettings.json      # Configurações em produção
└── appsettings.Development.json  # Configurações em desenvolvimento
```

### Fluxo Multitenant

1. **Tenant**: Representa uma organização/cliente
2. **Usuario**: Usuário pertencente a um tenant específico
3. **RegistroAqua**: Registro de consumo associado ao usuário e tenant
4. **Notificacao**: Notificação do sistema associada ao usuário e tenant

Cada operação verifica o `TenantId` do JWT para garantir isolamento de dados.

## 🔧 Instalação e Configuração

### Pré-requisitos
- .NET 10
- MySQL 8.0+
- Visual Studio Code ou Visual Studio

### 1. Instalar dependências

```bash
cd Backend
dotnet restore
```

### 2. Configurar banco de dados

Editar `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=seu_servidor;Port=3306;Database=aquamonitor_dev;User=seu_usuario;Password=sua_senha;"
  },
  "ChaveJwt": "SuaChaveSecretaMuitoComplicada123456789"
}
```

### 3. Criar banco de dados MySQL

```sql
CREATE DATABASE aquamonitor_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE aquamonitor CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 4. Executar a aplicação

```bash
dotnet run
```

A aplicação criará as tabelas automaticamente na primeira execução.

## 📡 Endpoints da API

### Autenticação (`/api/autenticacao`)

#### Registrar Novo Usuário
```
POST /api/autenticacao/registrar
Content-Type: application/json

{
  "nome": "João da Silva",
  "email": "joao@example.com",
  "senha": "senha123",
  "identificadorTenant": "padrao"
}

Resposta (200):
{
  "mensagem": "Usuário registrado com sucesso"
}
```

#### Fazer Login
```
POST /api/autenticacao/entrar
Content-Type: application/json

{
  "email": "joao@example.com",
  "senha": "senha123",
  "identificadorTenant": "padrao"
}

Resposta (200):
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "nome": "João da Silva",
  "email": "joao@example.com",
  "idUsuario": 1,
  "idTenant": 1
}
```

### Registros de Água (`/api/registros-agua`) [Requer Authorization]

#### Listar Registros
```
GET /api/registros-agua?pagina=1&tamanho=10
Authorization: Bearer {token}

Resposta (200):
[
  {
    "id": 1,
    "idUsuario": 1,
    "consumoLitros": 250.5,
    "data": "2025-04-22T10:30:00Z",
    "observacoes": "Consumo durante chuveiro",
    "dataCriacao": "2025-04-22T10:30:00Z"
  }
]
```

#### Criar Registro
```
POST /api/registros-agua
Authorization: Bearer {token}
Content-Type: application/json

{
  "consumoLitros": 250.5,
  "data": "2025-04-22T10:30:00Z",
  "observacoes": "Consumo durante chuveiro"
}

Resposta (201):
{
  "id": 1,
  "idUsuario": 1,
  "consumoLitros": 250.5,
  "data": "2025-04-22T10:30:00Z",
  "observacoes": "Consumo durante chuveiro",
  "dataCriacao": "2025-04-22T10:30:00Z"
}
```

#### Atualizar Registro
```
PUT /api/registros-agua/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "consumoLitros": 280.0,
  "observacoes": "Consumo atualizado"
}

Resposta (200):
{
  "mensagem": "Registro atualizado com sucesso"
}
```

#### Deletar Registro
```
DELETE /api/registros-agua/{id}
Authorization: Bearer {token}

Resposta (200):
{
  "mensagem": "Registro deletado com sucesso"
}
```

### Notificações (`/api/notificacoes`) [Requer Authorization]

#### Listar Notificações
```
GET /api/notificacoes?pagina=1&tamanho=10
Authorization: Bearer {token}

Resposta (200):
[
  {
    "id": 1,
    "titulo": "Bem-vindo ao AquaMonitor!",
    "mensagem": "Obrigado por se cadastrar...",
    "lida": false,
    "tipo": "INFO",
    "dataCriacao": "2025-04-22T10:00:00Z",
    "dataLeitura": null
  }
]
```

#### Marcar Notificação como Lida
```
POST /api/notificacoes/{id}/marcar-como-lida
Authorization: Bearer {token}

Resposta (200):
{
  "mensagem": "Notificação marcada como lida"
}
```

#### Marcar Todas Notificações como Lidas
```
POST /api/notificacoes/marcar-todas-como-lidas
Authorization: Bearer {token}

Resposta (200):
{
  "mensagem": "Todas as notificações marcadas como lidas"
}
```

#### Contar Não Lidas
```
GET /api/notificacoes/nao-lidas/contar
Authorization: Bearer {token}

Resposta (200):
{
  "contar": 3
}
```

#### Deletar Notificação
```
DELETE /api/notificacoes/{id}
Authorization: Bearer {token}

Resposta (200):
{
  "mensagem": "Notificação deletada com sucesso"
}
```

### Dashboard (`/api/dashboard`) [Requer Authorization]

#### Resumo Completo
```
GET /api/dashboard/resumo
Authorization: Bearer {token}

Resposta (200):
{
  "totalRegistros": 42,
  "consumoTotal": 10500.5,
  "consumoMedio": 250.0,
  "consumoDiaAtual": 320.0,
  "consumoMesAtual": 7850.0,
  "registrosRecentes": [...]
}
```

#### Consumo Diário
```
GET /api/dashboard/consumo-diario?mes=4&ano=2025
Authorization: Bearer {token}

Resposta (200):
[
  {
    "data": "2025-04-01",
    "consumoTotal": 245.5
  },
  {
    "data": "2025-04-02",
    "consumoTotal": 198.0
  }
]
```

#### Consumo Semanal
```
GET /api/dashboard/consumo-semanal
Authorization: Bearer {token}

Resposta (200):
[
  {
    "data": "2025-04-16",
    "consumoTotal": 245.5
  },
  {
    "data": "2025-04-17",
    "consumoTotal": 312.0
  }
]
```

#### Consumo Mensal (Últimos 12 meses)
```
GET /api/dashboard/consumo-mensal
Authorization: Bearer {token}

Resposta (200):
[
  {
    "mes": 4,
    "ano": 2024,
    "consumoTotal": 6200.0
  }
]
```

#### Estatísticas Gerais
```
GET /api/dashboard/estatisticas
Authorization: Bearer {token}

Resposta (200):
{
  "maiorConsumo": 520.5,
  "menorConsumo": 120.3,
  "mediaConsumoDia": 250.0,
  "desviaoPadrao": 84.5
}
```

## 🔐 Autenticação JWT

1. Fazer login e obter token
2. Incluir no header: `Authorization: Bearer {token}`
3. Token válido por 7 dias
4. Claims incluem: UserId, TenantId, Name, Email

## 📊 Modelo de Dados

### Tenants
```sql
CREATE TABLE tenants (
  id INT PRIMARY KEY AUTO_INCREMENT,
  identificador VARCHAR(100) UNIQUE NOT NULL,
  nome VARCHAR(100) NOT NULL,
  descricao VARCHAR(500),
  conexao_banco_dados VARCHAR(255) NOT NULL,
  ativo BOOLEAN DEFAULT true,
  data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,
  data_atualizacao DATETIME
);
```

### Usuarios
```sql
CREATE TABLE usuarios (
  id INT PRIMARY KEY AUTO_INCREMENT,
  id_tenant INT NOT NULL,
  nome VARCHAR(100) NOT NULL,
  email VARCHAR(100) NOT NULL,
  senha_hash VARCHAR(255) NOT NULL,
  telefone VARCHAR(20),
  ativo BOOLEAN DEFAULT true,
  data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,
  data_atualizacao DATETIME,
  UNIQUE KEY unique_email_tenant (id_tenant, email),
  FOREIGN KEY (id_tenant) REFERENCES tenants(id) ON DELETE CASCADE
);
```

### RegistrosAgua
```sql
CREATE TABLE registros_agua (
  id INT PRIMARY KEY AUTO_INCREMENT,
  id_usuario INT NOT NULL,
  id_tenant INT NOT NULL,
  consumo_litros DOUBLE NOT NULL,
  data DATETIME NOT NULL,
  observacoes VARCHAR(500),
  data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON DELETE CASCADE,
  FOREIGN KEY (id_tenant) REFERENCES tenants(id) ON DELETE CASCADE
);
```

### Notificacoes
```sql
CREATE TABLE notificacoes (
  id INT PRIMARY KEY AUTO_INCREMENT,
  id_usuario INT NOT NULL,
  id_tenant INT NOT NULL,
  titulo VARCHAR(200) NOT NULL,
  mensagem VARCHAR(1000) NOT NULL,
  lida BOOLEAN DEFAULT false,
  tipo VARCHAR(50),
  data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,
  data_leitura DATETIME,
  FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON DELETE CASCADE,
  FOREIGN KEY (id_tenant) REFERENCES tenants(id) ON DELETE CASCADE
);
```

## 🧪 Testando a API

Use Postman, Insomnia, ou similar para testar:

1. Executar backend: `dotnet run`
2. A API estará disponível em `http://localhost:5093`
3. OpenAPI disponível em `http://localhost:5093/openapi/v1.json`

## 📝 Variáveis de Ambiente

Configurar em `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=aquamonitor_dev;User=root;Password=root;"
  },
  "ChaveJwt": "ChaveDesenvolvimentoTemporariaQueDeveSerMudadaEmProducao123456",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

## 🚀 Deploy em Produção

1. Atualizar `appsettings.json` com credenciais reais
2. Usar chave JWT secura e forte
3. Usar HTTPS
4. Configurar CORS adequadamente
5. Fazer backup regular do banco MySQL

## 📚 Tecnologias Utilizadas

- **Framework**: ASP.NET Core 10
- **ORM**: Entity Framework Core 10
- **Banco de Dados**: MySQL 8.0+
- **Autenticação**: JWT (JSON Web Token)
- **Validação**: Data Annotations
- **Criptografia**: BCrypt.Net

## 🐛 Troubleshooting

### Erro de conexão MySQL
- Verificar se MySQL está rodando
- Validar credenciais em appsettings
- Verificar se banco existe

### Erro ao rodar migrations
- Executar: `dotnet ef database update`
- Se necessário resetar: `dotnet ef database drop --force`

### Erro de autenticação
- Verificar se token é válido
- Verificar se ChaveJwt é a mesma em Program.cs
- Token tem validade de 7 dias

## 📞 Suporte

Para questões sobre implementação, consultar:
- Documentação oficial: https://docs.microsoft.com/dotnet
- Entity Framework: https://docs.microsoft.com/ef
- MySQL Connector: https://dev.mysql.com/doc/
