# 📦 SQL Server Database Backup API

This project provides an ASP.NET Core Web API to back up a SQL Server database to a `.bak` file, compress it to a `.zip`, return it to the client for download, and then delete the temporary files. 

All steps are logged using the built-in logging system.

---

## 🔧 Features

- 🛡️ Securely backs up a SQL Server database via API.
- 📦 Compresses `.bak` file to `.zip` for faster download.
- 🔥 Deletes all temporary files after download.
- 📝 Logs every step using `ILogger`.
- 🧠 Auto-detects database name from connection string.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 6+](https://dotnet.microsoft.com/download)
- SQL Server (Local/Remote)
- Visual Studio / VS Code

---

## 🛠️ Configuration

### 1. Update Connection String

Edit `DatabaseBackupController.cs`:

```csharp
private readonly string _connectionString = "Server=YOUR_SERVER;Database=YOUR_DB;User Id=sa;Password=yourPassword;";
```

- Set your SQL Server details.
- If using Windows Auth, replace with:  
  `"Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;"`

---

### 2. Set Backup Directory

Edit this path as needed:

```csharp
private readonly string _backupFolder = "C:\DatabaseBackups";
```

Ensure the folder exists or the application can create it.

---

## 📡 API Endpoint

| Method | Route                 | Description                  |
|--------|-----------------------|------------------------------|
| GET    | `/api/databasebackup/download` | Triggers backup and downloads `.zip` |

### ✅ Example Request

```bash
GET http://localhost:5000/api/databasebackup/download
```

Returns a ZIP file containing the database backup.

---

## 📁 Output

1. Creates: `YourDB_20250603103045.bak`
2. Zips to: `YourDB_20250603103045.zip`
3. Returns file to client.
4. Deletes both `.bak` and `.zip` files after streaming.

---

## 🪵 Logging

Logs include:

- Backup started
- Folder creation
- Backup success
- Zip file creation
- Memory load
- File cleanup
- Errors with stack trace

> You can view logs in your terminal, debug window, or configure providers like Serilog, File, Seq, etc.

---

## 📚 Extending

Want to add more features?

- 🔑 Password-protect ZIP files with `SharpZipLib`
- ☁️ Upload backups to cloud (Azure Blob, AWS S3)
- 🔔 Send backup alerts via email
- 📅 Schedule backups using Hangfire or Windows Task Scheduler

---

## ⚖️ License

MIT License

---

## 🙋 Support

Feel free to open issues or pull requests. Contributions are welcome!
