# Корпоративный портал на Blazor

Проект создан с нуля как серверное Blazor-приложение с авторизацией по LDAP через GitLab и встроенной моделью пользователей/ролей.

## Основные возможности

- **LDAP-аутентификация через GitLab.** Проверка учетных данных происходит по подключению к каталогу GitLab LDAP с использованием библиотеки `Novell.Directory.Ldap`.
- **Управление ролями.** После успешного входа пользователь синхронизируется с локальной базой данных и получает роли, соответствующие группам из LDAP.
- **UI на Blazor Server.** Приложение построено на Blazor Server и использует cookie-аутентификацию ASP.NET Core.
- **Административный раздел.** Пользователи с ролью `Admin` могут просматривать всех авторизованных сотрудников и управлять их ролями.

## Настройка LDAP подключения

Параметры авторизации задаются в конфигурации (`appsettings.json`, переменные окружения и т.д.) в секции `Authentication:GitLabLdap`.

```json
"Authentication": {
  "GitLabLdap": {
    "Host": "gitlab.example.com",
    "Port": 389,
    "UseSsl": true,
    "BindDn": "cn=readonly,dc=example,dc=com",
    "BindCredentials": "readonly-password",
    "UserBaseDn": "ou=users,dc=example,dc=com",
    "UserFilter": "(uid={0})",
    "DisplayNameAttribute": "cn",
    "EmailAttribute": "mail",
    "RoleAttribute": "memberOf",
    "RoleMappings": {
      "cn=platform-admins,ou=groups,dc=example,dc=com": "Admin",
      "cn=portal-users,ou=groups,dc=example,dc=com": "User"
    }
  }
}
```

- `Host`, `Port`, `UseSsl` — адрес и параметры подключения к LDAP, который обслуживает GitLab.
- `BindDn`, `BindCredentials` — учетная запись для поиска пользователей (при необходимости).
- `UserBaseDn`, `UserFilter` — местоположение пользователей и фильтр поиска.
- `DisplayNameAttribute`, `EmailAttribute` — атрибуты каталога, используемые для отображения данных.
- `RoleAttribute` и `RoleMappings` — атрибут, содержащий членство в группах, и соответствие групп ролям приложения.

## Роли по умолчанию

Приложение предсоздает две роли: `Admin` и `User`. Если у пользователя нет сопоставленных ролей в LDAP, ему автоматически назначается роль `User`.

## Структура проекта

- `Program.cs` — настройка сервисов, аутентификации и конечных точек входа/выхода.
- `Data/` — определение контекста базы данных и сущностей пользователей/ролей.
- `Services/` — логика работы с LDAP и синхронизации пользователей.
- `Pages/` — Razor-компоненты (Blazor) для страниц входа, домашней страницы и управления пользователями.
- `Shared/` — общие компоненты макета и меню.
- `wwwroot/` — статические ресурсы и стили.

## Запуск проекта

```bash
dotnet restore
dotnet run
```

По умолчанию используется база данных EF Core InMemory. Для боевого использования рекомендуется заменить её на постоянное хранилище (например, PostgreSQL или SQL Server) и включить HTTPS.
