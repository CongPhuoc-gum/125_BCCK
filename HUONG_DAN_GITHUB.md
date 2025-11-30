# Hướng dẫn đẩy code lên GitHub

## 📋 Checklist trước khi commit

### ✅ Đã làm:
- [x] Tạo file `.gitignore` (đã có sẵn)
- [x] Tạo file `Web.config.template` (thay thế Web.config)
- [x] Tạo file `README.md`
- [x] Ignore folder `images/appointments/*` (chỉ giữ .gitkeep)

### ⚠️ Cần kiểm tra:

1. **Web.config** - File này sẽ KHÔNG được commit (đã có trong .gitignore)
   - Đảm bảo `Web.config.template` đã được tạo
   - Người khác sẽ copy template này thành Web.config

2. **TestController & DebugController** - Tùy chọn:
   - Nếu muốn giữ để team test: Commit bình thường
   - Nếu không muốn: Uncomment các dòng trong .gitignore

3. **Images/appointments** - Folder này sẽ trống (chỉ có .gitkeep)

## 🚀 Các bước đẩy code lên GitHub

### Bước 1: Tạo repository trên GitHub

1. Truy cập https://github.com
2. Click nút "New repository"
3. Đặt tên: `PetCare-Management` (hoặc tên khác)
4. Chọn **Private** nếu không muốn public
5. **KHÔNG** chọn "Initialize with README" (vì đã có sẵn)
6. Click "Create repository"

### Bước 2: Mở Git Bash hoặc Terminal trong thư mục project

```bash
cd D:\Visual_Studio\BCCK\Repos\125_BCCK
```

### Bước 3: Khởi tạo Git (nếu chưa có)

```bash
git init
```

### Bước 4: Kiểm tra các file sẽ được commit

```bash
git status
```

**Kiểm tra kỹ:**
- ✅ Web.config KHÔNG nên xuất hiện (màu đỏ)
- ✅ Web.config.template NÊN xuất hiện (màu đỏ)
- ✅ bin/, obj/, packages/ KHÔNG nên xuất hiện

Nếu Web.config xuất hiện, chạy:
```bash
git rm --cached Web.config
```

### Bước 5: Add tất cả files

```bash
git add .
```

### Bước 6: Commit

```bash
git commit -m "Initial commit: PetCare Management System with Staff Module"
```

### Bước 7: Thêm remote repository

Thay `YOUR_USERNAME` và `YOUR_REPO_NAME`:

```bash
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
```

Ví dụ:
```bash
git remote add origin https://github.com/nguyenvana/PetCare-Management.git
```

### Bước 8: Đẩy code lên GitHub

```bash
git branch -M main
git push -u origin main
```

Nếu yêu cầu đăng nhập:
- Username: Tên GitHub của bạn
- Password: **Personal Access Token** (KHÔNG phải password GitHub)

#### Tạo Personal Access Token:
1. GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Generate new token (classic)
3. Chọn scope: `repo` (full control)
4. Copy token và dùng làm password

## 🔄 Cập nhật code sau này

### Khi có thay đổi mới:

```bash
# Kiểm tra thay đổi
git status

# Add files
git add .

# Commit với message mô tả
git commit -m "Add feature: Upload image in Complete Appointment"

# Push lên GitHub
git push
```

## 👥 Làm việc nhóm

### Clone project về máy:

```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
cd YOUR_REPO_NAME
```

### Setup sau khi clone:

1. **Tạo Web.config từ template:**
   ```bash
   copy Web.config.template Web.config
   ```

2. **Sửa connection string** trong Web.config

3. **Chạy script database:** `DVthucung.sql`

4. **Restore NuGet packages** trong Visual Studio

5. **Build và chạy**

### Pull code mới nhất:

```bash
git pull origin main
```

### Tránh conflict:

```bash
# Trước khi làm việc
git pull

# Sau khi làm xong
git add .
git commit -m "Your message"
git push
```

## 🚨 Lưu ý quan trọng

### ❌ KHÔNG BAO GIỜ commit:
- `Web.config` - Chứa password database
- `bin/`, `obj/` - Build output
- `packages/` - NuGet packages (sẽ restore lại)
- `.vs/` - Visual Studio cache
- `*.user` - User settings

### ✅ NÊN commit:
- Source code (.cs, .cshtml, .css, .js)
- `Web.config.template`
- `DVthucung.sql`
- `README.md`
- `.gitignore`
- Static files (images, fonts) - KHÔNG phải user uploads

## 🔍 Kiểm tra trước khi push

```bash
# Xem các file sẽ được commit
git status

# Xem chi tiết thay đổi
git diff

# Xem lịch sử commit
git log --oneline
```

## 🆘 Xử lý sự cố

### Đã commit nhầm Web.config:

```bash
# Xóa khỏi Git nhưng giữ file local
git rm --cached Web.config

# Commit lại
git commit -m "Remove Web.config from tracking"
git push
```

### Đã push nhầm password lên GitHub:

1. **Thay đổi password database NGAY LẬP TỨC**
2. Xóa file khỏi Git history (phức tạp, nên tạo repo mới)
3. Hoặc làm repo private

### Conflict khi pull:

```bash
# Xem file bị conflict
git status

# Sửa file thủ công, sau đó:
git add .
git commit -m "Resolve conflict"
git push
```

## 📚 Tài liệu tham khảo

- [Git Documentation](https://git-scm.com/doc)
- [GitHub Guides](https://guides.github.com/)
- [Gitignore Templates](https://github.com/github/gitignore)
