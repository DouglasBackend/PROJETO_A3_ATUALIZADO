# AquaMonitor — Backend

API REST em **ASP.NET Core 10** para monitoramento de consumo de água. Gerencia autenticação, registros de consumo, contas de água, notificações e dashboard analítico.

---

## Tecnologias

| Tecnologia | Uso |
|---|---|
| ASP.NET Core 10 | Framework web |
| Entity Framework Core 8 | ORM / acesso ao banco |
| SQLite | Banco de dados (arquivo local) |
| BCrypt.Net | Hash de senhas |
| Cookie Authentication | Autenticação de sessão |
| Swagger / OpenAPI | Documentação interativa da API |

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

## Instalação e execução

```bash
# 1. Entrar na pasta do backend
cd Backend

# 2. Restaurar pacotes
dotnet restore

# 3. Executar
dotnet run
```

A API estará disponível em `http://localhost:5093`. O banco de dados SQLite é criado automaticamente na primeira execução, sem nenhuma configuração adicional.

---

## Documentação interativa

Com o servidor rodando, acesse o Swagger para explorar e testar todos os endpoints:

```
http://localhost:5093/swagger
```

---

## Estrutura do projeto

```
Backend/
├── Controllers/
│   ├── AutenticacaoController.cs   # Login, cadastro, logout
│   ├── ContasAguaController.cs     # Faturas mensais de água
│   ├── DashboardController.cs      # Resumos e gráficos
│   ├── NotificacoesController.cs   # Notificações do sistema
│   ├── RegistrosAguaController.cs  # Leituras de consumo
│   └── SistemaController.cs        # Verificação de setup
├── Data/
│   └── AppDbContext.cs             # Contexto do banco
├── DTOs/                           # Objetos de transferência de dados
├── Models/                         # Entidades do banco
└── Program.cs                      # Configuração da aplicação
```
