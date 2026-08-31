# TreasureChamber 珍宝展厅 · 产品管理系统

面向灯具展厅 / 产品展示场所的**纯本地离线**产品管理工具：批量导入产品目录 → 按系列/分类浏览搜索 → 生成打印二维码 → 登记客户意向单，全程不联网。

## 技术栈

| 层 | 技术 |
|---|---|
| 后端 | .NET 10 · ASP.NET Core（JSON API）· SQLite (EF Core 10) · 防伪令牌 |
| 前端 | **Vue 3 + PrimeVue**（DataTable / Galleria / Dialog / Toast / Tree / FileUpload…）· PrimeIcons · Vite 构建 |
| 部署 | Docker 多阶段构建（node → sdk → aspnet 运行时）· Docker Compose |
| 特性 | 全部静态资源本地化（无 CDN）、Bootstrap 已移除、纯离线可运行 |

## 功能一览

| 模块 | 功能 | 主要 PrimeVue 组件 |
|---|---|---|
| 产品浏览 | 首页按系列卡片 / 分类树一层层浏览，型号/名称搜索，分页 | Tree、Card、InputText、Paginator |
| 产品详情 | 图片轮播、产品介绍、规格参数表、单产品二维码下载 | Galleria、Tag、Button |
| 产品管理 | 表格列表、新建/编辑（规格动态行、多图上传删除）、删除确认、状态停用/在售 | DataTable、Dialog、FileUpload、ConfirmDialog |
| 批量导入 | Excel(.xlsx)/CSV 上传 → 预览校验（新增/更新/错误标记）→ 确认入库；型号已存在自动更新；系列与多级分类（`灯具/吊灯`）自动创建；GBK/UTF-8 自动识别；批量图片按「文件名=型号」匹配 | FileUpload、DataTable、Message |
| 二维码中心 | 表格多选（含全选本页）→ A4 打印页（二维码+型号+名称，自动打印）/ 打包下载全部 PNG (zip) | DataTable（多选）、Checkbox |
| 意向单 | 客户信息 + 产品搜索选择（Dialog）+ 动态明细行；编号 YX 开头；列表状态筛选（新建/跟进中/已成交/已放弃）；状态流转；Excel 一键导出 | SelectButton、Dialog、InputNumber、Tag |
| 系统设置 | 配置二维码访问地址（局域网 IP:端口）、展示本机可用地址与访问基线 | InputText、Message |

## 快速开始（本地运行）

```bash
# 前端已构建产物在 wwwroot，直接运行后端即可（默认监听 http://0.0.0.0:5000）
dotnet run --project src\TreasureChamber.WebApp

# 或发布为独立运行
dotnet publish src\TreasureChamber.WebApp -c Release -o publish
publish\TreasureChamber.WebApp.exe
```

浏览器打开 `http://localhost:5000`（本机）或 `http://<电脑局域网IP>:5000`（局域网设备 / 手机）。首次启动自动建库（`App_Data/treasure.db`）并写入示例产品数据。

## 前端开发

```bash
cd src\TreasureChamber.WebApp\ClientApp
npm install          # 首次
npm run dev          # 开发模式：http://localhost:5173，/api 代理到 5000，热更新
npm run build        # 生产构建，产物输出到 ../wwwroot（Vite outDir）
```

前端结构（`ClientApp/src/`）：

```
main.js            应用入口（PrimeVue 全局注册、主题 lara-light-blue）
router.js          前端路由（/ /products /product/:id /manage /import /qr /intent-orders /settings）
api.js             axios 实例（自动携带 XSRF-TOKEN 头）+ 文件下载工具
layout/AppLayout.vue   顶部导航 + 内容区
views/             13 个页面组件（BrowseHome / ProductList / ProductDetail / ProductManage /
                   ProductEditDialog / ImportCenter / QrCenter / QrPrint / IntentOrder* / Settings）
assets/app.css     全局样式 + 二维码打印页 CSS
```

> 前端修改后必须 `npm run build` 重新打包，后端启动时才会加载新界面。

## Docker 部署

```bash
# 方式一：直接构建并运行（多阶段：node 构建前端 → sdk 发布后端 → aspnet 运行时）
docker build -t treasurechamber:latest .
docker run -d --name treasurechamber -p 5000:5000 -v treasurechamber-data:/app/App_Data treasurechamber:latest

# 方式二：docker compose 一键部署（推荐）
docker compose up -d
```

- 容器监听 `http://0.0.0.0:5000`，宿主机 `5000` 端口对外。
- 数据（SQLite 库 + 数据保护密钥）持久化在命名卷 `treasurechamber-data`，重建容器不丢失。
- 升级：`docker compose up -d --build`；日志：`docker logs -f treasurechamber`；停止：`docker compose down`。
- 容器以非 root 运行（entrypoint 修正卷属主后降权为 `app` 用户）。

## API 接口一览

统一前缀 `/api`，JSON；写操作需携带 `X-XSRF-TOKEN` 请求头（前端 axios 自动处理）。

| 方法 | 路径 | 说明 |
|---|---|---|
| GET | `/api/catalog` | 首页数据：系列+数量、分类树+数量、最新产品 |
| GET | `/api/products` | 产品分页列表（seriesId/categoryId/keyword/page/pageSize/includeInactive） |
| GET | `/api/products/picker?keyword=` | 意向单产品选择器 |
| GET | `/api/products/{id}` | 产品详情（含图片、规格） |
| GET | `/api/products/{id}/qr` | 单产品二维码 PNG |
| POST/PUT/DELETE | `/api/products[/{id}]` | 新建 / 更新 / 删除产品 |
| POST/DELETE | `/api/products/{id}/images[/{imageId}]` | 上传 / 删除产品图片 |
| POST | `/api/import/preview` | 上传目录文件并预览（多部分表单 `file`） |
| POST | `/api/import/commit` | 确认入库（沿用预览会话） |
| POST | `/api/import/images` | 按文件名匹配型号批量上传图片 |
| GET | `/api/import/template` | 下载导入模板 xlsx |
| GET | `/api/qr/print?ids=1,2` | 打印标签数据 |
| GET | `/api/qr/zip?ids=1,2` | 打包下载二维码 PNG (zip) |
| GET/POST | `/api/intent-orders[/{id}]` | 意向单列表 / 创建 / 详情 |
| PUT | `/api/intent-orders/{id}/status?status=` | 状态流转 |
| GET | `/api/intent-orders/export` | 导出全部意向单 Excel |
| GET/PUT | `/api/settings` | 读取 / 保存二维码访问地址 |

## 二维码使用说明（重要）

1. 启动后在「系统设置」填写**展厅电脑的局域网地址**，例如 `http://192.168.1.100:5000`（页面会列出本机所有 IPv4 供参考，选手机能访问的那个）。
2. 「二维码中心」勾选产品 → 打印标签 / 下载 PNG → 打印出来贴在实物旁。
3. 客户手机连**同一 WiFi**，扫描即打开该产品详情页。

> Windows 首次监听 5000 端口会弹出防火墙提示，需允许「专用网络」访问，否则手机无法连接。

## 数据导入格式

表头（`型号`为必填）：`型号, 名称, 系列, 分类, 描述, 规格参数`

- `分类`支持多级，用 `/` 分隔，如 `灯具/吊灯`，不存在会自动创建。
- `规格参数`格式：`名称=值;名称=值`，如 `功率=36W;色温=3000K`。
- CSV 兼容 GBK / UTF-8（自动识别）；Excel 请下载「导入模板」参考填写。

## 数据备份

直接复制整个 `App_Data` 文件夹即可（SQLite 数据库 + 数据保护密钥）；Docker 部署则备份命名卷 `treasurechamber-data`。

## 解决方案结构

```
TreasureChamber.slnx
src/
├── TreasureChamber.Core/            实体（Product/Category/Series/IntentOrder...）+ 枚举
├── TreasureChamber.Data/            AppDbContext、Repo 仓储、DbInitializer（建库+种子）
├── TreasureChamber.Application/     产品导入、二维码、意向单、Excel 导出服务
└── TreasureChamber.WebApp/
    ├── Controllers/Api/             JSON API 控制器（8 组）
    ├── Models/                      API DTO + 实体映射
    ├── ClientApp/                   Vue3 + PrimeVue 前端工程（Vite）
    ├── wwwroot/                     SPA 构建产物 + 上传图片（uploads/）
    └── Program.cs                   服务注册、防伪令牌、SPA 托管
```

## 离线还原说明

仓库根目录 `nuget.config` 已清空远端源（本机依赖在全局 NuGet 缓存中），保证离线构建；Docker 构建使用 `.docker/nuget.config`（nuget.org）在容器内还原。
