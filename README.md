# MoeSoft - 动漫管理助手

<div align="center">

![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![UI](https://img.shields.io/badge/UI-Wpf.Ui-fluent.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

**一款现代化的番剧追踪与管理工具**

[English](#english) | [日本語](#日本語) | [한국어](#한국어) | [中文](#中文)

</div>

---

## 🌟 功能演示 / Feature Showcase

### 📱 现代化界面设计
```
┌─────────────────────────────────────────────────────────┐
│  🎬 MoeSoft                              ─  □  ×       │
├─────────────────────────────────────────────────────────┤
│                                                         │
│   ┌─────────────┐                                      │
│   │  🏠 首页    │    ╔═══════════════════════════╗     │
│   │  🔍 搜索    │    ║  【本季新番】              ║     │
│   │  ℹ️ 关于    │    ║                           ║     │
│   └─────────────┘    ║  ┌───┐  ┌───┐  ┌───┐      ║     │
│                      ║  │░░░│  │░░░│  │░░░│      ║     │
│                      ║  └───┘  └───┘  └───┘      ║     │
│                      ║  番剧封面  番剧封面  ...   ║     │
│                      ╚═══════════════════════════╝     │
│                                                         │
│         ╭─────────────────────────────────╮            │
│         │ ✓ 添加完成，返回首页查看吧！    │            │
│         ╰─────────────────────────────────╯            │
└─────────────────────────────────────────────────────────┘
```

### 🔍 番剧搜索功能

**演示流程：**
1. 在搜索框输入动画名称（如：「葬送的芙莉莲」）
2. 按下 Enter 键开始搜索
3. 系统调用 BangumiScraper 获取搜索结果
4. 显示包含封面、标题的搜索结果列表
5. 点击「添加」按钮即可收藏该番剧

```
搜索前:                          搜索后:
┌──────────────────┐            ┌──────────────────────────┐
│ 🔍 输入动画名称...│            │ 🔍 葬送的芙莉莲          │
└──────────────────┘            ├──────────────────────────┤
                                │ ┌────┐ 葬送的芙莉莲      │
                                │ │IMG │ [添加] [打开链接] │
                                │ └────┘                  │
                                │ ┌────┐ 葬送のフリーレン  │
                                │ │IMG │ [添加] [打开链接] │
                                │ └────┘                  │
                                └──────────────────────────┘
```

### 📺 番剧详情页

**展示内容：**
- 🖼️ 高清封面图
- 📝 中日文标题
- 📅 开播日期与更新信息
- 👥 制作人员（导演、编剧、制作公司）
- 📊 总集数与当前更新集数
- 🎬 简介剧情概要

**本地视频关联：**
```
┌─────────────────────────────────────────┐
│  📁 关联本地视频文件夹                  │
│  ┌─────────────────────────────────┐    │
│  │ D:\Anime\Frieren\               │    │
│  │                                 │    │
│  │  ✅ EP01.mp4  (最近观看)        │    │
│  │  ⬜ EP02.mp4                    │    │
│  │  ⬜ EP03.mp4                    │    │
│  │  ...                            │    │
│  └─────────────────────────────────┘    │
│                                         │
│  [🔄 刷新数据]  [📂 更改文件夹]         │
└─────────────────────────────────────────┘
```

### 🎨 UI/UX 特性

| 特性 | 说明 |
|------|------|
| 🌙 **深色主题** | 默认采用深色模式，保护视力 |
| ✨ **亚克力效果** | Windows 11 Mica/Acrylic 材质背景 |
| 🔔 **Snackbar 通知** | 优雅的弹出式消息提示 |
| 💫 **流畅动画** | 页面切换、加载动画平滑过渡 |
| 🎯 **自定义标题栏** | 集成最小化/最大化/关闭按钮 |
| 📦 **内容对话框** | 现代化的模态对话框组件 |

---

## 🛠️ 技术栈 / Tech Stack

- **框架**: .NET 8.0 + WPF
- **UI 库**: [Wpf.Ui](https://github.com/lepoco/wpfui) (Fluent Design)
- **数据解析**: Newtonsoft.Json
- **网络请求**: HttpClient
- **爬虫**: 独立 EXE (BangumiScraper, BangumiSearchScrap)
- **API**: Bangumi.tv

---

## 📦 安装与使用 / Installation

### 前置要求
- Windows 10/11
- .NET 8.0 Runtime

### 快速开始
```bash
# 1. 克隆仓库
git clone https://github.com/your-repo/MoeSoft.git

# 2. 编译项目
cd MoeSoft
dotnet build

# 3. 运行应用
dotnet run --project NewScarAnime/MoeSoft.csproj
```

### 目录结构
```
MoeSoft/
├── NewScarAnime/           # 主应用程序
│   ├── MainWindow.xaml     # 主窗口
│   ├── HomePage.xaml       # 首页（番剧列表）
│   ├── BangumiSearch.xaml  # 搜索页面
│   ├── AnimeInfoPage.xaml  # 详情页面
│   ├── AboutPage.xaml      # 关于页面
│   └── Scrap/              # 爬虫程序
│       ├── BangumiScraper.exe
│       └── BangumiSearchScrap.exe
└── Fonts/                  # 字体资源
```

---

## 📸 更多截图 / Screenshots

### 首页 - 本季新番优先显示
```
╔════════════════════════════════════════════╗
║  【本季新番】置顶显示                      ║
║  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐      ║
║  │    │ │    │ │    │ │    │ │    │      ║
║  └────┘ └────┘ └────┘ └────┘ └────┘      ║
║  番剧 A  番剧 B  番剧 C  番剧 D  番剧 E    ║
╠════════════════════════════════════════════╣
║  【历史番剧】                              ║
║  ┌────┐ ┌────┐ ┌────┐                     ║
║  │    │ │    │ │    │                     ║
║  └────┘ └────┘ └────┘                     ║
╚════════════════════════════════════════════╝
```

### 通知系统示例
```
成功添加:
╭─────────────────────────────────────────╮
│ ℹ️ System:                              │
│ 添加完成，返回首页查看吧！q(≧▽≦q)       │
╰─────────────────────────────────────────╯

错误提示:
╭─────────────────────────────────────────╮
│ ⚠️ 查找封面异常:                        │
│ 封面图片未找到: C:\...\cover.jpg        │
╰─────────────────────────────────────────╯
```

---

## 🔧 配置说明 / Configuration

### 数据存储位置
应用数据存储在本地 AppData 目录：
```
%LOCALAPPDATA%\ScarAnime\
├── AnimeInfo/          # 番剧元数据 (JSON)
├── AnimeCover/         # 封面图片
├── AnimeLocalLink/     # 本地视频路径映射
└── Settings.json       # 用户设置
```

### JSON 数据结构示例
```json
{
  "bangumi_url": "https://bangumi.tv/subject/390555",
  "name": "葬送のフリーレン",
  "name_chinese": "葬送的芙莉莲",
  "image_url": "https://lain.bgm.tv/pic/cover/xxx.jpg",
  "summary": "勇者一行人打倒魔王之后...",
  "total_episodes": 28,
  "start_date": "2023 年 9 月 29 日",
  "air_weekday": "星期五",
  "director": "斋藤圭一郎",
  "writer": "铃木智寻",
  "studio": "MADHOUSE",
  "current_episode": 28
}
```

---

## 🤝 贡献指南 / Contributing

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

---

## 📄 许可证 / License

MIT License - 详见 [LICENSE](LICENSE) 文件

---

## 🙏 致谢 / Acknowledgments

- [Bangumi.tv](https://bangumi.tv/) - 番剧数据源
- [Wpf.Ui](https://github.com/lepoco/wpfui) - Fluent Design UI 库
- [Noto Sans](https://fonts.google.com/noto) - 字体支持

---

<div align="center">

**Made with ❤️ for Anime Fans**

⭐ 如果这个项目对你有帮助，请给个 Star 支持一下！

</div>

---

<a name="english"></a>
# MoeSoft - Anime Management Assistant

<div align="center">

**A Modern Anime Tracking & Management Tool**

</div>

## 🌟 Feature Showcase

### 📱 Modern Interface Design
- Fluent Design UI powered by Wpf.Ui
- Dark theme with Acrylic/Mica effects
- Custom titlebar with window controls
- Smooth animations and transitions

### 🔍 Bangumi Search
1. Enter anime title in search box
2. Press Enter to search
3. View results with covers and titles
4. Click "Add" to favorite the anime

### 📺 Anime Details Page
- HD cover images
- Japanese & Chinese titles
- Air date and update schedule
- Staff information (Director, Writer, Studio)
- Episode count and current progress
- Synopsis

### 🎨 Local Video Integration
- Link local video folders to anime entries
- Track watched episodes
- Auto-detect video files (.mp4, .mkv, .avi, etc.)
- Mark last watched episode

### 🔔 Notification System
- Snackbar notifications for actions
- Content dialogs for confirmations
- Error handling with user-friendly messages

---

<a name="日本語"></a>
# MoeSoft - アニメ管理アシスタント

<div align="center">

**モダンなアニメ追跡・管理ツール**

</div>

## 🌟 機能デモ

### 📱 モダンなインターフェース
- Wpf.Ui による Fluent Design UI
- アクリル/マイカ効果付きダークテーマ
- カスタムタイトルバー
- スムーズなアニメーション

### 🔍 バングミ検索
1. 検索ボックスにアニメタイトルを入力
2. Enter キーで検索実行
3. サムネイルとタイトル付きの結果を表示
4. 「追加」ボタンでお気に入りに登録

### 📺 アニメ詳細ページ
- ハイビジョンカバー画像
- 日本語・中国語タイトル
- 放送日・更新情報
- スタッフ情報（監督、脚本、制作会社）
- 話数情報
- あらすじ

### 🎨 ローカル動画連携
- アニメエントリにローカル動画フォルダをリンク
- 視聴済みエピソードを追跡
- 動画ファイルを自動検出
- 最終視聴エピソードをマーク

### 🔔 通知システム
- スナックバー通知
- 確認用コンテンツダイアログ
- エラーハンドリング

---

<a name="한국어"></a>
# MoeSoft - 애니메이션 관리 도우미

<div align="center">

**현대적인 애니메이션 추적 및 관리 도구**

</div>

## 🌟 기능 시연

### 📱 현대적인 인터페이스
- Wpf.Ui 의 Fluent Design UI
- 아크릴/마이카 효과가 있는 다크 테마
- 커스텀 제목 표시줄
- 부드러운 애니메이션

### 🔍 방구미 검색
1. 검색창에 애니메이션 제목 입력
2. Enter 키로 검색 실행
3. 썸네일과 제목이 포함된 결과 확인
4. "추가" 버튼으로 즐겨찾기 등록

### 📺 애니메이션 상세 페이지
- 고화질 커버 이미지
- 일본어·중국어 제목
- 방송일 및 업데이트 정보
- 스태프 정보 (감독, 각본, 제작사)
- 에피소드 수 및 진행 상황
- 줄거리

### 🎨 로컬 동영상 연동
- 애니메이션 항목에 로컬 동영상 폴더 링크
- 시청 완료 에피소드 추적
- 동영상 파일 자동 감지
- 마지막 시청 에피소드 표시

### 🔔 알림 시스템
- 스낵바 알림
- 확인용 콘텐츠 다이얼로그
- 오류 처리

---

<a name="中文"></a>
# MoeSoft - 动漫管理助手

<div align="center">

**一款现代化的番剧追踪与管理工具**

</div>

## 🌟 核心功能

### 📱 现代化界面
- 基于 Wpf.Ui 的 Fluent Design 设计
- 深色主题 + 亚克力/云母效果
- 自定义标题栏
- 流畅的过渡动画

### 🔍 番剧搜索
1. 在搜索框输入动画名称
2. 按 Enter 键搜索
3. 浏览带封面的搜索结果
4. 点击"添加"收藏番剧

### 📺 番剧详情
- 高清封面图
- 中日双语标题
- 开播日期与更新日程
- 制作团队信息
- 集数统计
- 剧情简介

### 🎨 本地视频管理
- 关联本地视频文件夹
- 追踪已观看集数
- 自动识别视频格式
- 标记最后观看记录

### 🔔 通知系统
- 优雅的消息提示
- 确认对话框
- 友好的错误处理

---

<div align="center">

[返回顶部](#moeSoft---动漫管理助手)

</div>
