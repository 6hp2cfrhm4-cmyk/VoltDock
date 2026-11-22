# VoltDock

<div align="center">
  <img src="docs/images/logo.png" alt="VoltDock Logo" width="128" height="128">
  
  **Красивый, лёгкий док для отображения заряда устройств в Windows**
  
  [![Build Status](https://github.com/yourusername/VoltDock/workflows/Build%20and%20Test/badge.svg)](https://github.com/yourusername/VoltDock/actions)
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
  [![Windows 10/11](https://img.shields.io/badge/Windows-10%2F11-blue.svg)](https://www.microsoft.com/windows)
  [![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com)
</div>

---

## Обзор

VoltDock — современное нативное Windows-приложение, которое показывает уровень заряда подключённых устройств в красивом, настраиваемом плавающем доке. Работает без фирменного ПО производителей (Logitech G HUB, Razer Synapse, SteelSeries GG и др.) — использует стандартные API Windows.

### Основные возможности

- **🎯 Универсальная поддержка устройств** — Bluetooth-мыши, клавиатуры, наушники, TWS-наушники, геймпады, USB-приёмники 2.4 ГГц, батарея ноутбука
- **🎨 Красивый док** — Плавающая панель с стеклянным эффектом, размытием, плавными анимациями и темами
- **⚡ Обновления в реальном времени** — Событийная модель + адаптивный поллинг, минимальное потребление CPU/памяти
- **🔔 Умные уведомления** — Настраиваемые пороги, учёт режима «Не беспокоить», никакого спама
- **🔧 Полная настройка** — Позиция, размер, прозрачность, анимации, индивидуальные настройки устройств
- **🛡️ Приватность превыше всего** — 100% локально, никакой телеметрии, никаких сетевых запросов, никаких аккаунтов
- **📦 Портативный и лёгкий** — ~5 МБ, установка не требуется, запускается из любой папки

---

## Поддерживаемые устройства

| Категория | Примеры | Источник батареи |
|-----------|---------|------------------|
| **Bluetooth-мыши** | Logitech MX Master, Microsoft Surface Mouse, Keychron M3 | GATT Battery Service (0x180F) |
| **Bluetooth-клавиатуры** | Keychron K8, Logitech MX Keys, NuPhy Air75 | GATT Battery Service |
| **Наушники/Гарнитуры** | Sony WH-1000XM5, Bose QC45, JBL Tour One | GATT / Свойства устройства |
| **TWS-наушники** | AirPods Pro, Galaxy Buds, MCHOSE V9 Pro | GATT (мульти-батарея: Л/П/Кейс) |
| **Геймпады** | Xbox Wireless, DualSense, 8BitDo | XInput / GATT |
| **Приёмники 2.4 ГГц** | Logitech Lightspeed, Razer HyperSpeed, Attack Shark R2 | HID Battery Report / Протокол производителя |
| **Батарея ноутбука** | Встроенная батарея | Windows Power API |

> **Важно:** Чтение заряда зависит от того, что устройство/прошивка сообщает Windows. Если устройство не сообщает заряд через стандартные протоколы, будет показано «Неизвестно», а не выдуманные данные.

---

## Скриншоты

<div align="center">
  <img src="docs/images/dock-darkglass.png" alt="Dark Glass Dock" width="600">
  <p><em>Тема Dark Glass на Windows 11</em></p>
  
  <img src="docs/images/settings.png" alt="Settings Window" width="600">
  <p><em>Окно настроек с живым предпросмотром дока</em></p>
</div>

---

## Установка

### Портативная версия (рекомендуется)
1. Скачайте `VoltDock-x64.zip` из [Releases](https://github.com/yourusername/VoltDock/releases)
2. Распакуйте в любую папку (например, `C:\Tools\VoltDock`)
3. Запустите `VoltDock.exe`
4. (Опционально) Создайте ярлык в `shell:startup` для автозапуска

### Установщик
1. Скачайте `VoltDock-Setup.exe` из Releases
2. Запустите установщик
3. Следуйте инструкциям мастера

### Требования
- Windows 10 19041+ (20H1) или Windows 11
- .NET 9 Runtime (входит в self-contained сборки)
- Процессор x64 или ARM64

---

## Сборка из исходников

### Предварительные требования
- Visual Studio 2022 17.9+ или .NET 9 SDK
- Windows 10/11 SDK (10.0.19041+)
- Git

### Шаги сборки
```bash
# Клонирование репозитория
git clone https://github.com/yourusername/VoltDock.git
cd VoltDock

# Восстановление зависимостей
dotnet restore VoltDock.sln

# Сборка
dotnet build VoltDock.sln --configuration Release --platform x64

# Запуск тестов
dotnet test VoltDock.sln --configuration Release --platform x64

# Публикация
dotnet publish src/VoltDock.App/VoltDock.App.csproj --configuration Release --platform x64 --self-contained false -o ./publish
```

---

## Архитектура

```
VoltDock/
├── src/
│   ├── VoltDock.Core/          # Основная логика, обнаружение устройств, провайдеры батареи
│   ├── VoltDock.UI/            # WPF-контролы, темы, конвертеры
│   └── VoltDock.App/           # Точка входа, окна, ViewModel'и
├── tests/
│   ├── VoltDock.Core.Tests/    # Юнит-тесты ядра
│   └── VoltDock.UI.Tests/      # UI-тесты
├── docs/                       # Документация
├── installer/                  # Проекты установщиков (Wix/MSIX)
└── .github/workflows/          # CI/CD пайплайны
```

### Основные компоненты

| Компонент | Ответственность |
|-----------|-----------------|
| `IDeviceDiscoveryProvider` | Обнаружение устройств (Bluetooth LE, HID, XInput, Windows Power) |
| `IBatteryProvider` | Чтение заряда из источников (GATT, HID, XInput, Производитель) |
| `IDeviceIdentityResolver` | Дедупликация устройств через несколько интерфейсов |
| `IBatteryReadingAggregator` | Агрегация показаний от нескольких провайдеров |
| `VoltDockService` | Оркестрация всех компонентов, управление жизненным циклом |

### Провайдеры батареи (по приоритету)
1. **Производительские** (MCHOSE V9 Pro, Attack Shark R2, Logitech, Razer) — Высший
2. **Bluetooth GATT** (0x180F Battery Service) — Стандарт Bluetooth LE
3. **HID Battery Report** (Usage Page 0x000C) — USB/2.4 ГГц устройства
4. **XInput** — Игровые контроллеры
5. **Windows Power API** — Встроенная батарея ноутбука
6. **Свойства устройств** — Резерв через `System.Devices.BatteryLevel`

---

## Конфигурация

Настройки хранятся в `%LOCALAPPDATA%\VoltDock\settings.json`.

### Настройки дока
```json
{
  "Dock": {
    "Position": "Bottom",           // Bottom, Top, Left, Right
    "DisplayMode": "IconsWithPercent",
    "AutoHide": "Never",
    "EdgeMargin": 20,
    "IconSize": 32,
    "CornerRadius": 16,
    "Opacity": 0.85,
    "EnableBlur": true,
    "EnableAnimations": true,
    "HideOnFullscreen": true
  }
}
```

### Пороги уведомлений
```json
{
  "Notifications": {
    "Enabled": true,
    "GlobalWarningPercent": 20,
    "GlobalCriticalPercent": 10,
    "Thresholds": [
      { "DeviceId": "mouse-xyz", "WarningPercent": 15, "CriticalPercent": 5 }
    ],
    "RespectFocusAssist": true,
    "MinNotificationInterval": 30
  }
}
```

---

## Приватность

**VoltDock не собирает никаких данных.**

- Никакой телеметрии
- Никакой аналитики
- Никакого crash reporting (кроме ручного экспорта диагностики)
- Никаких сетевых соединений вообще
- Все данные остаются на вашем компьютере
- Открытый исходный код — проверьте сами

Подробнее в [PRIVACY.md](PRIVACY.md).

---

## Матрица совместимости

| Устройство | Bluetooth | 2.4 ГГц USB | USB Проводной | Примечания |
|------------|-----------|-------------|---------------|------------|
| MCHOSE V9 Pro | ✅ GATT | 🔄 Исследование | ❓ | Мульти-батарея (Л/П/Кейс) |
| Attack Shark R2 | ✅ GATT | 🔄 Исследование | ❓ | Игровая мышь |
| Logitech G Pro Wireless | ✅ | ✅ HID/Logi | ❓ | Через приёмник Lightspeed |
| Razer Viper Ultimate | ✅ | ✅ HID/Razer | ❓ | Через приёмник HyperSpeed |
| Keychron K8/K3 | ✅ GATT | ❌ | ✅ HID | Проводной показывает заряд |
| Xbox Series Controller | ✅ GATT | ❌ | ✅ XInput | Bluetooth + USB |
| DualSense (PS5) | ✅ GATT | ❌ | ✅ HID | |
| AirPods Pro | ✅ GATT | ❌ | ❌ | Мульти-батарея |
| Generic HID Mouse | ❌ | ✅ HID | ✅ HID | Если прошивка поддерживает |

**Легенда:** ✅ Проверено работает | 🔄 В исследовании | ❓ Неизвестно | ❌ Не поддерживается

---

## Решение проблем

### Устройство показывает «Неизвестно»
1. Убедитесь, что устройство сопряжено/подключено в Параметрах Windows
2. Проверьте, поддерживает ли прошивка устройства отчёт о батарее
3. Попробуйте переподключить устройство
4. Посмотрите `Дополнительно → Диагностика` на ошибки провайдеров

### Док не появляется
1. Проверьте, включён ли док в Настройки → Док
2. Проверьте конфигурацию мониторов (мульти-мониторные настройки)
3. Проверьте настройку `Скрывать в полноэкранном режиме` если играете

### Высокое потребление CPU
1. Включите `Адаптивный поллинг` в Дополнительных настройках
2. Увеличьте `Интервал поллинга`
3. Отключите неиспользуемые провайдеры в Дополнительных настройках

### Уведомления не показываются
1. Проверьте Windows Focus Assist / Не беспокоить
2. Убедитесь, что `Уведомления → Включены` в настройках
3. Проверьте индивидуальные настройки уведомлений для устройства

---

## Участие в разработке

Мы приветствуем вклад! См. [CONTRIBUTING.md](CONTRIBUTING.md) для руководства.

### Настройка разработки
1. Сделайте форк репозитория
2. Создайте ветку для фичи
3. Внесите изменения
4. Запустите тесты: `dotnet test`
5. Проверьте форматирование: `dotnet format --verify-no-changes`
6. Отправьте PR

---

## Лицензия

MIT License — см. [LICENSE](LICENSE) для деталей.

---

## Благодарности

- Команде Windows SDK за отличные API
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) за системный трей
- [Microsoft.Xaml.Behaviors.Wpf](https://github.com/microsoft/XamlBehaviorsWpf) за behaviors
- Всем open-source проектам по реверс-инженерингу протоколов производителей

---

## Ссылки

- [Releases](https://github.com/yourusername/VoltDock/releases)
- [Issues](https://github.com/yourusername/VoltDock/issues)
- [Discussions](https://github.com/yourusername/VoltDock/discussions)
- [Changelog](CHANGELOG.md)
- [Security Policy](SECURITY.md)

---

<div align="center">
  <sub>Создано с ❤️ для пользователей Windows, любящих чистое, функциональное ПО.</sub>
</div>