# BarberX Enterprise - Self-Hosted Installation Guide

## 🎯 Overview

This guide will help you deploy BarberX Enterprise on your own infrastructure using Docker.

**Requirements:**
- Docker & Docker Compose installed
- Valid BarberX Enterprise license key
- 4+ CPU cores, 16GB+ RAM
- 500GB+ storage
- Ubuntu 20.04+ or similar Linux distribution

---

## 🚀 Quick Start (15 minutes)

### **Step 1: Obtain License Key**

Contact BarberX Enterprise team:
- **Email:** enterprise@barberx.info
- **Sales:** +1 (XXX) XXX-XXXX

You'll receive:
- License key: `BX-XXXX-XXXX-XXXX-XXXX`
- Installation support
- Onboarding call

### **Step 2: Download Installation Package**

```bash
# Download from BarberX customer portal or receive via email
wget https://releases.barberx.info/enterprise/barberx-enterprise-latest.tar.gz

# Or clone from private repository (credentials provided)
git clone https://github.com/barberx/enterprise-deploy.git
cd enterprise-deploy
```

### **Step 3: Configure Environment**

```bash
# Copy environment template
cp .env.enterprise.template .env

# Edit configuration
nano .env
```

**Required settings:**
```bash
BARBERX_LICENSE_KEY=BX-XXXX-XXXX-XXXX-XXXX  # Your license key
SECRET_KEY=<generate-64-char-hex>
DB_PASSWORD=<strong-password>
OPENAI_API_KEY=sk-your-key  # Your OpenAI key
DOMAIN=barberx.yourdomain.com
```

**Generate secret key:**
```bash
python3 -c "import secrets; print(secrets.token_hex(32))"
```

### **Step 4: Launch Services**

```bash
# Start all services
docker-compose -f docker-compose.enterprise.yml up -d

# Check status
docker-compose -f docker-compose.enterprise.yml ps

# View logs
docker-compose -f docker-compose.enterprise.yml logs -f barberx-app
```

### **Step 5: Verify Installation**

```bash
# Check license validation
curl http://localhost:5000/health

# Should return:
# {"status": "healthy", "license": "valid"}
```

**Access the application:**
```
http://your-server-ip:5000
```

---

## 🔒 License Activation

### **How Licensing Works**

1. **Phone Home:** Application contacts `license.barberx.info` daily to validate license
2. **Machine Registration:** Your server is registered to your license (max machines enforced)
3. **Grace Period:** Can run offline for 72 hours before requiring re-validation
4. **Features:** License controls which features are enabled

### **Validate License Manually**

```bash
# Enter the container
docker exec -it barberx-app bash

# Run license check
python3 -c "
from license_client import LicenseClient
client = LicenseClient()
if client.is_valid():
    print('✅ License is valid')
    print(client.get_license_info())
else:
    print('❌ License validation failed')
"
```

### **Troubleshooting License Issues**

**Error: "Invalid license key"**
```bash
# Check environment variable
docker exec barberx-app env | grep LICENSE

# Verify license key is correct (no typos)
# Contact enterprise@barberx.info to verify key
```

**Error: "Machine limit exceeded"**
```bash
# Your license is installed on too many servers
# Contact enterprise@barberx.info to:
#   - Increase machine limit
#   - Unregister old machines
#   - Upgrade license tier
```

**Error: "License expired"**
```bash
# Renew your license
# Contact enterprise@barberx.info for renewal
# Payment link will be sent via email
```

**Error: "Cannot validate - offline"**
```bash
# Check internet connectivity
ping license.barberx.info

# Check firewall rules (allow HTTPS outbound to license.barberx.info)

# If server must be air-gapped:
#   - Contact enterprise@barberx.info
#   - Special air-gap license available ($4,999/year)
#   - Manual activation via USB key
```

---

## 🔧 Configuration

### **Database Setup**

**PostgreSQL is included in Docker Compose.**

To use external PostgreSQL:
```bash
# In .env
DATABASE_URL=postgresql://user:password@postgres-server:5432/barberx

# Remove postgres service from docker-compose.yml
```

### **SSL/HTTPS Setup**

**Option A: Let's Encrypt (Recommended)**

```bash
# Install certbot
sudo apt install certbot

# Generate certificate
sudo certbot certonly --standalone -d barberx.yourdomain.com

# Copy certs to nginx
sudo cp /etc/letsencrypt/live/barberx.yourdomain.com/fullchain.pem ssl/
sudo cp /etc/letsencrypt/live/barberx.yourdomain.com/privkey.pem ssl/

# Restart nginx
docker-compose -f docker-compose.enterprise.yml restart nginx
```

**Option B: Corporate Certificate**

```bash
# Copy your certificates
cp your-cert.pem ssl/fullchain.pem
cp your-key.pem ssl/privkey.pem

# Update permissions
chmod 644 ssl/fullchain.pem
chmod 600 ssl/privkey.pem
```

### **AI Model Configuration**

**Use Local Whisper (Saves API costs):**

```bash
# In docker-compose.yml, add to barberx-app environment:
- USE_LOCAL_WHISPER=true
- WHISPER_MODEL_SIZE=base  # tiny, base, small, medium, large
```

**For GPU acceleration:**

```yaml
# Add to barberx-app service:
deploy:
  resources:
    reservations:
      devices:
        - driver: nvidia
          count: 1
          capabilities: [gpu]
```

### **Storage Configuration**

**Local Storage (Default):**
```bash
# Data stored in Docker volumes
docker volume ls | grep barberx

# Backup volumes
docker run --rm -v barberx-data:/data -v $(pwd):/backup alpine tar czf /backup/barberx-backup.tar.gz /data
```

**S3/R2 Storage:**
```bash
# In .env
AWS_ACCESS_KEY_ID=your-key
AWS_SECRET_ACCESS_KEY=your-secret
AWS_S3_BUCKET=barberx-uploads
AWS_REGION=us-west-2
CLOUD_STORAGE_ENABLED=true
```

---

## 📊 Monitoring

### **Health Checks**

```bash
# Application health
curl http://localhost:5000/health

# Database health
docker exec barberx-postgres pg_isready

# Redis health
docker exec barberx-redis redis-cli ping
```

### **View Logs**

```bash
# All services
docker-compose -f docker-compose.enterprise.yml logs -f

# Specific service
docker-compose -f docker-compose.enterprise.yml logs -f barberx-app

# Last 100 lines
docker-compose -f docker-compose.enterprise.yml logs --tail=100 barberx-app
```

### **Usage Statistics**

```bash
# View license usage
docker exec -it barberx-app python3 -c "
from license_client import LicenseClient
client = LicenseClient()
# Reports videos processed, users active, etc.
"
```

---

## 🔄 Updates

### **Updating BarberX Enterprise**

```bash
# Pull latest image
docker pull barberx/enterprise:latest

# Or pull specific version
docker pull barberx/enterprise:2.5.0

# Restart with new image
docker-compose -f docker-compose.enterprise.yml down
docker-compose -f docker-compose.enterprise.yml up -d

# Verify new version
docker exec barberx-app python3 -c "import os; print(os.getenv('BARBERX_VERSION'))"
```

### **Database Migrations**

```bash
# Run migrations after update
docker exec -it barberx-app flask db upgrade
```

---

## 💾 Backup & Restore

### **Backup**

```bash
#!/bin/bash
# backup-barberx.sh

BACKUP_DIR="/backups/barberx/$(date +%Y-%m-%d)"
mkdir -p $BACKUP_DIR

# Backup database
docker exec barberx-postgres pg_dump -U barberx barberx > $BACKUP_DIR/database.sql

# Backup data volumes
docker run --rm -v barberx-data:/data -v $BACKUP_DIR:/backup alpine tar czf /backup/data.tar.gz /data

# Backup configuration
cp .env $BACKUP_DIR/
cp docker-compose.enterprise.yml $BACKUP_DIR/

echo "Backup complete: $BACKUP_DIR"
```

### **Restore**

```bash
#!/bin/bash
# restore-barberx.sh

BACKUP_DIR=$1

# Restore database
cat $BACKUP_DIR/database.sql | docker exec -i barberx-postgres psql -U barberx barberx

# Restore data
docker run --rm -v barberx-data:/data -v $BACKUP_DIR:/backup alpine tar xzf /backup/data.tar.gz -C /

# Restart services
docker-compose -f docker-compose.enterprise.yml restart

echo "Restore complete"
```

---

## 🔐 Security Best Practices

### **1. Firewall Configuration**

```bash
# Only expose necessary ports
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable

# Block direct access to application port
sudo ufw deny 5000/tcp
```

### **2. Regular Updates**

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Update Docker images monthly
docker pull barberx/enterprise:latest
```

### **3. Access Control**

```bash
# Limit SSH access
# /etc/ssh/sshd_config
PermitRootLogin no
PasswordAuthentication no  # Use SSH keys only
```

### **4. Secrets Management**

```bash
# Never commit .env to version control
echo ".env" >> .gitignore

# Use Docker secrets for sensitive data
docker secret create db_password /path/to/password/file
```

---

## 📞 Support

### **Enterprise Support SLA**

- **Response Time:** 4 hours (business hours)
- **Critical Issues:** 1 hour (24/7)
- **Dedicated Slack Channel:** Provided during onboarding
- **Quarterly Business Reviews:** Included

### **Contact Information**

- **Email:** enterprise@barberx.info
- **Phone:** +1 (XXX) XXX-XXXX (24/7 for P1 issues)
- **Portal:** https://support.barberx.info
- **Status Page:** https://status.barberx.info

### **Common Support Requests**

1. **Increase License Limits**
   - Email with current usage stats
   - Upgrade quote sent within 4 hours

2. **Add More Servers**
   - Provide machine IDs to register
   - License updated within 1 business day

3. **Custom Features**
   - Schedule consultation call
   - Custom development quotes available

---

## 🎓 Training & Onboarding

### **Included Training**

1. **Admin Training (2 hours)**
   - Installation & configuration
   - User management
   - Troubleshooting

2. **User Training (1 hour)**
   - Platform walkthrough
   - BWC analysis workflow
   - Best practices

3. **Documentation**
   - Admin guide
   - User guide
   - API documentation

### **Schedule Training**

Email enterprise@barberx.info with:
- Preferred dates/times
- Number of attendees
- Specific topics of interest

---

## 📋 Checklist

### **Pre-Launch:**
- [ ] License key obtained
- [ ] Server provisioned (4+ cores, 16GB+ RAM)
- [ ] Docker & Docker Compose installed
- [ ] .env configured
- [ ] SSL certificates generated
- [ ] Firewall configured
- [ ] Backup strategy planned

### **Post-Launch:**
- [ ] License validated successfully
- [ ] Admin account created
- [ ] Test user accounts created
- [ ] Sample BWC video analyzed
- [ ] SSL working (HTTPS)
- [ ] Backups scheduled
- [ ] Monitoring alerts configured
- [ ] Team training scheduled

---

## 🚀 Next Steps

1. **Complete installation** following steps above
2. **Create admin account** at `/admin/setup`
3. **Configure organization settings** at `/admin/settings`
4. **Schedule training** with BarberX team
5. **Import user accounts** (bulk import available)
6. **Start analyzing cases!**

---

**Questions? Contact enterprise@barberx.info anytime.**

**Last Updated:** January 26, 2026  
**Version:** 1.0.0
