# 🚀 GUIA RÁPIDO - Iniciar o Backend AquaMonitor

## ⚡ Setup em 5 Minutos

### 1️⃣ Pré-requisitos
```bash
# Verificar se MySQL está instalado e rodando
mysql --version
mysql -u root -p -e "SELECT 1"

# Verificar se .NET 10 está instalado
dotnet --version
```

### 2️⃣ Criar Banco de Dados
```bash
# Abrir MySQL
mysql -u root -p

# Colar os seguintes comandos:
CREATE DATABASE aquamonitor_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE aquamonitor CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EXIT;
```

### 3️⃣ Configurar Credenciais

Editar `Backend/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=aquamonitor_dev;User=root;Password=SEU_SENHA_MYSQL;"
  },
  "ChaveJwt": "ChaveSecretaDesenvolvimento123456789",
  ...
}
```

### 4️⃣ Executar Backend

```bash
cd Backend
dotnet run
```

Você verá:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5093
```

### 5️⃣ Testar API

Abra no navegador:
```
http://localhost:5093/openapi/v1.json
```

## 📝 Primeiros Testes

### Registrar Usuário (Postman/Insomnia)

```
POST http://localhost:5093/api/autenticacao/registrar
Content-Type: application/json

{
  "nome": "Teste Usuario",
  "email": "teste@example.com",
  "senha": "senha123",
  "identificadorTenant": "padrao"
}
```

Resposta esperada (200):
```json
{
  "mensagem": "Usuário registrado com sucesso"
}
```

### Fazer Login

```
POST http://localhost:5093/api/autenticacao/entrar
Content-Type: application/json

{
  "email": "teste@example.com",
  "senha": "senha123",
  "identificadorTenant": "padrao"
}
```

Resposta esperada (200):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "nome": "Teste Usuario",
  "email": "teste@example.com",
  "idUsuario": 1,
  "idTenant": 1
}
```

### Criar Registro de Água (Com token acima)

```
POST http://localhost:5093/api/registros-agua
Authorization: Bearer {cole-o-token-aqui}
Content-Type: application/json

{
  "consumoLitros": 250.5,
  "data": "2025-04-22T10:00:00Z",
  "observacoes": "Consumo durante chuveiro"
}
```

### Visualizar Dashboard

```
GET http://localhost:5093/api/dashboard/resumo
Authorization: Bearer {token}
```

## 🔗 Integrar com Frontend

### Atualizar arquivo `frontend/src/lib/api.ts`

```typescript
// Altere para:
const API_URL = "http://localhost:5093/api";
```

### Rodar Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend estará em: `http://localhost:5173`

## 📂 Arquivos Importantes

| Arquivo | Descrição |
|---------|-----------|
| `Backend/Program.cs` | Configuração e inicialização |
| `Backend/Controllers/*` | Endpoints da API |
| `Backend/Models/*` | Entidades do banco de dados |
| `Backend/DTOs/*` | Data Transfer Objects |
| `Backend/Data/AppDbContext.cs` | Contexto do banco de dados |
| `Backend/appsettings.Development.json` | Configurações de desenvolvimento |
| `Backend/README.md` | Documentação completa |

## 🐛 Debug

### Ver logs detalhados
```bash
# No terminal onde backend está rodando, você verá logs like:
info: Microsoft.EntityFrameworkCore.Infrastructure[10403]
      Entity Framework Core initialized 'AppDbContext' using provider 'Pomelo.EntityFrameworkCore.MySql:10.0.0'
```

### Verificar banco de dados criado
```bash
mysql -u root -p -e "USE aquamonitor_dev; SHOW TABLES; DESCRIBE usuarios;"
```

### Limpar banco e recomeçar (DEV APENAS)
```bash
# Deletar e recriar banco
mysql -u root -p -e "DROP DATABASE aquamonitor_dev; CREATE DATABASE aquamonitor_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# Rodar backend novamente para recriar estrutura
dotnet run
```

## ❌ Problemas Comuns

### "Connection refused"
```
❌ Problema: MySQL não está rodando
✅ Solução: 
   - Windows: net start MySQL80
   - Linux: sudo service mysql start
   - Mac: brew services start mysql
```

### "Access denied for user 'root'@'localhost'"
```
❌ Problema: Senha MySQL incorreta
✅ Solução: Atualizar password em appsettings.Development.json
```

### "Port 5093 already in use"
```
❌ Problema: Outro processo usando porta
✅ Solução: 
   - Windows: netstat -ano | findstr :5093
   - Linux/Mac: lsof -i :5093
   - Matar processo e tentar novamente
```

### CORS Error no console do Frontend
```
❌ Problema: Backend CORS não permitindo origem
✅ Solução: Verificar se frontend está em http://localhost:5173
```

## 📚 Próximos Passos

1. ✅ Backend rodando ✓
2. ⬜ Frontend fazendo requisições (próximo)
3. ⬜ Testes automatizados
4. ⬜ Deploy em produção

## 📖 Documentação Completa

Para guias detalhados, ver:
- [Backend/README.md](Backend/README.md) - Documentação completa da API
- [INTEGRACAO_FRONTEND.md](INTEGRACAO_FRONTEND.md) - Como integrar frontend
- [DEPLOYMENT.md](DEPLOYMENT.md) - Como fazer deploy em produção

## 🎯 Estrutura da Resposta API

Todas as respostas seguem este padrão:

### Sucesso (200, 201)
```json
{
  "campo": "valor"
}
```

### Erro (400, 401, 500)
```json
{
  "mensagem": "Descrição do erro",
  "detalhes": "Detalhes opcionais"
}
```

## 🔒 JWT Token Info

Token tem validade de **7 dias**.

Após expirar, usuário precisa fazer login novamente.

## ✨ Recursos Extras

### Visualizar BD em UI
```bash
# Instalar Adminer (viewer web para MySQL)
docker run -p 8080:8080 adminer
# Acessar: http://localhost:8080
```

### Testar Endpoints com arquivo HTTP
```bash
# Usar Backend.http com REST Client extension do VS Code
# Instalar: REST Client (humao.rest-client)
# Abrir Backend/Backend.http e clicar em "Send Request"
```

## 🆘 Precisa de Ajuda?

Verifique:
1. Logs do backend: `dotnet run` (verá tudo no console)
2. Logs do banco: MySQL Workbench ou phpMyAdmin
3. Arquivo: `INTEGRACAO_FRONTEND.md` para conectar com frontend

---

**🎉 Parabéns! Seu backend está pronto para usar!**
