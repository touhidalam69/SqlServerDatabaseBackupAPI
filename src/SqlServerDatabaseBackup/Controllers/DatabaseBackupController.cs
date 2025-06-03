using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.IO.Compression;

namespace SqlServerDatabaseBackup.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseBackupController : ControllerBase
    {
        private readonly string _connectionString = "Server=TOUHID-PC; Database=VATNBR_WEB_MT;Trusted_Connection=False;ConnectRetryCount=0;User Id=sa; Password=data;Encrypt=False;TrustServerCertificate=True;";
        private readonly string _backupFolder = "C:\\DatabaseBackups";
        private readonly ILogger<DatabaseBackupController> _logger;

        public DatabaseBackupController(ILogger<DatabaseBackupController> logger)
        {
            _logger = logger;
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadZippedBackup()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            string databaseName = builder.InitialCatalog;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string bakFileName = $"{databaseName}_{timestamp}.bak";
            string zipFileName = $"{databaseName}_{timestamp}.zip";
            string bakFilePath = Path.Combine(_backupFolder, bakFileName);
            string zipFilePath = Path.Combine(_backupFolder, zipFileName);

            try
            {
                _logger.LogInformation("Starting database backup process for '{DatabaseName}'", databaseName);

                if (!Directory.Exists(_backupFolder))
                {
                    Directory.CreateDirectory(_backupFolder);
                    _logger.LogInformation("Created backup folder: {BackupFolder}", _backupFolder);
                }

                // Step 1: Backup the database
                _logger.LogInformation("Backing up database to file: {BakFilePath}", bakFilePath);
                BackupDatabase(_connectionString, databaseName, bakFilePath);

                // Step 2: Create zip file
                _logger.LogInformation("Creating zip archive: {ZipFilePath}", zipFilePath);
                using (var zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(bakFilePath, bakFileName, CompressionLevel.Optimal);
                }

                // Step 3: Read zip into memory
                _logger.LogInformation("Reading zip file into memory");
                var memoryStream = new MemoryStream();
                using (var zipStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
                {
                    await zipStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;

                // Step 4: Delete temp files
                _logger.LogInformation("Deleting temporary files");
                System.IO.File.Delete(bakFilePath);
                _logger.LogInformation("Deleted .bak file: {BakFilePath}", bakFilePath);

                System.IO.File.Delete(zipFilePath);
                _logger.LogInformation("Deleted .zip file: {ZipFilePath}", zipFilePath);

                _logger.LogInformation("Database backup and zip completed successfully for '{DatabaseName}'", databaseName);
                return File(memoryStream, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database backup");

                // Cleanup on failure
                if (System.IO.File.Exists(bakFilePath))
                {
                    System.IO.File.Delete(bakFilePath);
                    _logger.LogWarning("Deleted leftover .bak file: {BakFilePath}", bakFilePath);
                }

                if (System.IO.File.Exists(zipFilePath))
                {
                    System.IO.File.Delete(zipFilePath);
                    _logger.LogWarning("Deleted leftover .zip file: {ZipFilePath}", zipFilePath);
                }

                return BadRequest($"Error: {ex.Message}");
            }
        }

        private void BackupDatabase(string connectionString, string databaseName, string backupFilePath)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = $@"BACKUP DATABASE [{databaseName}] 
                                  TO DISK = N'{backupFilePath}' 
                                  WITH FORMAT, INIT, NAME = N'{databaseName}-Full Backup'";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            _logger.LogInformation("Database '{DatabaseName}' successfully backed up to '{BackupFilePath}'", databaseName, backupFilePath);
        }
    }
}
