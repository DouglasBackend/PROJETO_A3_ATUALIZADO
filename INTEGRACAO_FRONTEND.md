# Guia de Integração Frontend - Backend AquaMonitor

## 📋 Resumo das Mudanças

O backend foi completamente reimplementado com:
- ✅ MySQL em vez de SQLite
- ✅ Arquitetura Multitenant
- ✅ Controllers estruturados em MVC
- ✅ Código 100% em português
- ✅ Endpoints completamente novos

## 🔄 Mudanças nos Endpoints

### Autenticação

#### Antes (Minimal API):
```javascript
POST /api/auth/register
POST /api/auth/login
```

#### Agora (AutenticacaoController):
```javascript
POST /api/autenticacao/registrar
POST /api/autenticacao/entrar
```

**Novo parâmetro necessário:** `identificadorTenant`

```javascript
// Exemplo de register anterior
const response = await fetchApi('/auth/register', {
  method: 'POST',
  body: JSON.stringify({ name, email, password })
});

// Exemplo de register novo
const response = await fetchApi('/autenticacao/registrar', {
  method: 'POST',
  body: JSON.stringify({ 
    nome: name, 
    email: email, 
    senha: password,
    identificadorTenant: 'padrao' // NOVO!
  })
});
```

### Dashboard

#### Antes (Mock data):
```javascript
GET /api/dashboard/summary
```

#### Agora (DashboardController com dados reais):
```javascript
GET /api/dashboard/resumo
GET /api/dashboard/consumo-diario?mes=X&ano=Y
GET /api/dashboard/consumo-semanal
GET /api/dashboard/consumo-mensal
GET /api/dashboard/estatisticas
```

### Notificações

#### Antes (Minimal API):
```javascript
GET /api/notifications
POST /api/notifications/read/{id}
```

#### Agora (NotificacoesController):
```javascript
GET /api/notificacoes
GET /api/notificacoes/{id}
POST /api/notificacoes/{id}/marcar-como-lida
POST /api/notificacoes/marcar-todas-como-lidas
GET /api/notificacoes/nao-lidas/contar
DELETE /api/notificacoes/{id}
```

### Novos Endpoints (Registros de Água)

Agora há endpoints completos para gerenciar registros:

```javascript
GET /api/registros-agua
GET /api/registros-agua/{id}
POST /api/registros-agua
PUT /api/registros-agua/{id}
DELETE /api/registros-agua/{id}
```

## 📝 Atualizações Necessárias no Frontend

### 1. Atualizar `lib/api.ts`

```typescript
const API_URL = "http://localhost:5093/api";

// Adicionar gerenciamento de tenant
export function obterTenantLocal(): string {
  return localStorage.getItem("tenant") || "padrao";
}

export function salvarTenantLocal(tenant: string) {
  localStorage.setItem("tenant", tenant);
}
```

### 2. Atualizar LoginPage.tsx

```typescript
// ANTES
await fetchApi('/auth/register', {
  method: 'POST',
  body: JSON.stringify({ name, email, password })
});

// DEPOIS - Adicionar identificadorTenant
const tenant = obterTenantLocal();
await fetchApi('/autenticacao/registrar', {
  method: 'POST',
  body: JSON.stringify({ 
    nome: name, 
    email: email, 
    senha: password,
    identificadorTenant: tenant
  })
});
```

### 3. Atualizar JSON de Resposta

O token agora inclui mais informações:

```typescript
// ANTES
const data = await response.json();
localStorage.setItem("token", data.token);
localStorage.setItem("userName", data.name);

// DEPOIS
const data = await response.json();
localStorage.setItem("token", data.token);
localStorage.setItem("userName", data.nome); // Mudou de 'name'
localStorage.setItem("userEmail", data.email); // NOVO
localStorage.setItem("userId", data.idUsuario.toString()); // NOVO
localStorage.setItem("tenantId", data.idTenant.toString()); // NOVO
```

### 4. Atualizar Dashboard.tsx

```typescript
// Ao invés de 1 endpoint que retorna tudo:
const data = await fetchApi('/dashboard/summary');

// Agora são vários endpoints mais específicos:
const resumo = await fetchApi('/dashboard/resumo');
const consumoDiario = await fetchApi('/dashboard/consumo-diario');
const consumoSemanal = await fetchApi('/dashboard/consumo-semanal');
const consumoMensal = await fetchApi('/dashboard/consumo-mensal');
const estatisticas = await fetchApi('/dashboard/estatisticas');

// Para gráficos, usar os dados diferentes:
// resumo tiene: totalRegistros, consumoTotal, consumoMedio, consumoDiaAtual, consumoMesAtual
// consumoSemanal é um array com data e consumoTotal
// consumoMensal é um array com mes, ano e consumoTotal
```

## 🔐 Token JWT Atualizado

O token JWT agora contém:
```
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "João Silva",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "joao@example.com",
  "TenantId": "1",
  "TenantIdentificador": "padrao"
}
```

O frontend pode extrair esses dados após decode do JWT se necessário.

## 📡 Novas Rotas Recomendadas

Sugestões de rotas para o frontend aproveitar a nova API:

```typescript
// Já existe
GET /dashboard
GET /data-entry
GET /tips

// Novos sugeridos
GET /configuracoes (usuário)
GET /relatorios
GET /historico
```

## 🧪 Teste de Integração

Passos para testar:

1. **Backend rodando**:
   ```bash
   cd Backend
   dotnet run
   ```

2. **Frontend rodando**:
   ```bash
   cd frontend
   npm run dev
   ```

3. **Testar fluxo**:
   - Registrar novo usuário (usa tenant 'padrao' automaticamente)
   - Fazer login
   - Criar alguns registros de água
   - Visualizar dashboard com dados reais

4. **Verificar console**:
   - Não deve haver erros de CORS
   - Token deve estar no header Authorization

## 🚨 Erros Comuns e Soluções

### CORS Error
```
Solution: Verificar se backend está rodando em http://localhost:5093
E se frontend está em http://localhost:5173 ou localhost:3000
```

### 401 Unauthorized
```
Solution: Verificar se token está no localStorage e sendo enviado no header
```

### 404 Endpoint not found
```
Solution: Verificar se endpoint agora é /api/autenticacao em vez de /api/auth
```

### Erro ao registrar
```
Solution: Verificar se identificadorTenant é 'padrao' ou válido no BD
```

## 📱 Estrutura de Dados Frontend

Recomendado adicionar tipos TypeScript para os novos DTOs:

```typescript
interface RegistroAgua {
  id: number;
  idUsuario: number;
  consumoLitros: number;
  data: string;
  observacoes?: string;
  dataCriacao: string;
}

interface Notificacao {
  id: number;
  titulo: string;
  mensagem: string;
  lida: boolean;
  tipo?: string;
  dataCriacao: string;
  dataLeitura?: string;
}

interface RespostaAutenticacao {
  token: string;
  nome: string;
  email: string;
  idUsuario: number;
  idTenant: number;
}

interface Resumo {
  totalRegistros: number;
  consumoTotal: number;
  consumoMedio: number;
  consumoDiaAtual: number;
  consumoMesAtual: number;
  registrosRecentes: RegistroAgua[];
}
```

## 📚 Documentação Completa

Ver `Backend/README.md` para documentação completa de todos os endpoints.

## ✅ Checklist de Integração

- [ ] Backend rodando com MySQL
- [ ] Frontend consegue fazer login
- [ ] Token sendo armazenado corretamente
- [ ] Dashboard exibindo dados reais
- [ ] Registros de água sendo criados
- [ ] Notificações sendo listadas
- [ ] Testes de autorização funcionando
