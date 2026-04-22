# Guia de Deploy - Backend AquaMonitor

## 🚀 Preparação para Deploy

### Pré-requisitos
- MySQL 8.0+ (ou MariaDB 10.3+)
- .NET 10 Runtime
- Domínio com HTTPS
- Servidor com acesso a banco de dados

## 📦 Build e Publicação

### 1. Build Local

```bash
cd Backend
dotnet build --configuration Release
```

### 2. Publicar para Produção

```bash
dotnet publish --configuration Release --output ./publish
```

Isso criará pasta `publish/` com todos os arquivos necessários.

## 💾 Configuração do Banco de Dados

### 1. Criar Bancos de Dados

```sql
-- Produção
CREATE DATABASE aquamonitor 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Backup
CREATE DATABASE aquamonitor_backup 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;
```

### 2. Criar Usuário MySQL

```sql
-- Criar usuário com permissões limitadas
CREATE USER 'aquamonitor'@'localhost' IDENTIFIED BY 'SenhaForte123!@#';

-- Dar permissões apenas no banco de produção
GRANT ALL PRIVILEGES ON aquamonitor.* TO 'aquamonitor'@'localhost';

-- Se for usar de outro servidor:
CREATE USER 'aquamonitor'@'%' IDENTIFIED BY 'SenhaForte123!@#';
GRANT ALL PRIVILEGES ON aquamonitor.* TO 'aquamonitor'@'%';

FLUSH PRIVILEGES;
```

## 🔧 Configuração de Produção

### Atualizar `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=seu.servidor.com;Port=3306;Database=aquamonitor;User=aquamonitor;Password=SenhaForte123!@#;SSL Mode=Required;"
  },
  "ChaveJwt": "UmaChaveMuitoSecretaEComplicada123456789!@#$%^&*(somenteNumero62caracteres)",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "seu.dominio.com"
}
```

### HTTPS

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:80"
      },
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/path/to/cert.pfx",
          "Password": "seu-password-de-cert"
        }
      }
    }
  }
}
```

## 🐳 Docker (Opcional)

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src
COPY ["Backend/Backend.csproj", "Backend/"]
RUN dotnet restore "Backend/Backend.csproj"
COPY . .
WORKDIR "/src/Backend"
RUN dotnet build "Backend.csproj" -c Release -o /app/build

FROM builder AS publish
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish

FROM runtime
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80 443
ENV ASPNETCORE_URLS=http://+:80;https://+:443
ENTRYPOINT ["dotnet", "Backend.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root123
      MYSQL_DATABASE: aquamonitor
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build: .
    environment:
      ConnectionStrings__DefaultConnection: "Server=mysql;Port=3306;Database=aquamonitor;User=root;Password=root123;"
      ChaveJwt: "ChaveDesenvolvimentoTemporaria123456789"
    ports:
      - "5093:80"
    depends_on:
      mysql:
        condition: service_healthy
    restart: unless-stopped

volumes:
  mysql_data:
```

Rodar: `docker-compose up -d`

## 🖥️ Deployment em Linux (Ubuntu/Debian)

### 1. Instalar .NET Runtime

```bash
wget https://dot.net/dotnet-release-metadata.json
sudo apt-get update
sudo apt-get install -y dotnet-runtime-10.0
```

### 2. Criar Serviço Systemd

```bash
sudo nano /etc/systemd/system/aquamonitor-backend.service
```

```ini
[Unit]
Description=AquaMonitor Backend
After=network.target mysql.service

[Service]
Type=notify
User=www-data
WorkingDirectory=/var/www/aquamonitor-backend
ExecStart=/usr/bin/dotnet /var/www/aquamonitor-backend/Backend.dll
Restart=on-failure
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

### 3. Ativar Serviço

```bash
sudo systemctl daemon-reload
sudo systemctl start aquamonitor-backend
sudo systemctl enable aquamonitor-backend
sudo systemctl status aquamonitor-backend
```

### 4. Nginx como Reverse Proxy

```bash
sudo nano /etc/nginx/sites-available/aquamonitor
```

```nginx
server {
    listen 80;
    server_name seu.dominio.com;

    # Redirecionar para HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name seu.dominio.com;

    ssl_certificate /etc/letsencrypt/live/seu.dominio.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/seu.dominio.com/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:5093;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 5. Ativar Nginx

```bash
sudo ln -s /etc/nginx/sites-available/aquamonitor /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### 6. SSL com Let's Encrypt

```bash
sudo apt-get install certbot python3-certbot-nginx
sudo certbot certonly --nginx -d seu.dominio.com
```

## 📊 Backup e Restore MySQL

### Backup Automático

```bash
# backup.sh
#!/bin/bash
BACKUP_DIR="/backups/aquamonitor"
DATE=$(date +%Y%m%d_%H%M%S)
mysqldump -u aquamonitor -p "SenhaForte123!@#" aquamonitor > "$BACKUP_DIR/backup_$DATE.sql"
# Manter apenas últimos 7 dias
find "$BACKUP_DIR" -name "backup_*.sql" -mtime +7 -delete
```

### Agendar com Cron

```bash
crontab -e
# Executar backup diariamente às 2 da manhã
0 2 * * * /home/admin/backup.sh
```

### Restore

```bash
mysql -u aquamonitor -p aquamonitor < backup_20250422_020000.sql
```

## 🔒 Segurança

### Checklist de Segurança

- [ ] HTTPS ativado com certificado válido
- [ ] Chave JWT com +60 caracteres aleatórios
- [ ] Banco MySQL com usuário não-root
- [ ] Firewall configurado (porta 443 aberta, 3306 fechada para internet)
- [ ] Backups automáticos configurados
- [ ] Logs sendo monitorados
- [ ] Senhas em variáveis de ambiente (nunca no código)
- [ ] CORS restrito apenas ao domínio do frontend

### Variáveis de Ambiente

```bash
export ConnectionStrings__DefaultConnection="Server=...;Password=..."
export ChaveJwt="..."
export ASPNETCORE_ENVIRONMENT=Production
```

## 📈 Monitoramento

### Logs

```bash
# Systemd
sudo journalctl -u aquamonitor-backend -f

# Arquivo
sudo tail -f /var/log/aquamonitor/backend.log
```

### Health Check

```bash
curl https://seu.dominio.com/openapi/v1.json
```

## 🆘 Troubleshooting

### Backend não inicia
```bash
# Verificar logs
sudo journalctl -u aquamonitor-backend -n 50

# Verificar erro de banco
mysql -u aquamonitor -p -e "SELECT 1"
```

### Erro de conexão MySQL
```sql
-- Verificar permissões
SHOW GRANTS FOR 'aquamonitor'@'localhost';

-- Resetar senha se necessário
ALTER USER 'aquamonitor'@'localhost' IDENTIFIED BY 'NovaSenha123!@#';
FLUSH PRIVILEGES;
```

### CORS Error
- Verificar `AllowedHosts` em appsettings.json
- Verificar origem do frontend

## 📋 Checklist Final

- [ ] Banco de dados criado
- [ ] Usuário MySQL criado com permissões
- [ ] appsettings.json atualizado
- [ ] HTTPS configurado
- [ ] Backup automático configurado
- [ ] Logs configurados
- [ ] Frontend configurado para novo URL
- [ ] Teste de login funcionando
- [ ] Dashboard com dados reais
- [ ] Monitoramento ativo

## 📞 Suporte

Caso encontre problemas:
1. Verificar logs: `sudo journalctl -u aquamonitor-backend -f`
2. Verificar banco: `mysql -u aquamonitor -p aquamonitor -e "SHOW TABLES;"`
3. Testar conectividade: `curl http://localhost:5093/openapi/v1.json`
