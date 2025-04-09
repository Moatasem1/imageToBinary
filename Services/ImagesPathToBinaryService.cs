using ImagesToBinary.Data;
using ImagesToBinary.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sky.NetCore;
using System.Data;

class ImagesPathToBinaryService
{
    private readonly List<TargetTable> _tables;
    private readonly string _imageRootPath;
    private List<string> _errorLog = new List<string>();
    private readonly DocumentCompressor _compressor;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public ImagesPathToBinaryService(ImageSettings imageSettings)
    {
        _tables = imageSettings.Tables;
        _imageRootPath = imageSettings.RootPath;
        _compressor = new DocumentCompressor();
        _contentTypeProvider = new FileExtensionContentTypeProvider();
    }

    public void ConvertImagesToBinary()
    {
        var dbContext = new AppDB();
        var startTime = System.DateTime.Now;
        try
        {
            foreach (var table in _tables)
                ConverteTableImagesToBinary(table, dbContext);

            // After processing all tables, output all errors
            if (_errorLog.Count > 0)
            {
                Console.WriteLine("\nErrors encountered during processing:");
                foreach (var error in _errorLog)
                {
                    Console.WriteLine(error);
                }
                Console.WriteLine($"Total errors: {_errorLog.Count}");
            }

            var endTime = System.DateTime.Now;
            Console.WriteLine($"Image conversion completed in {endTime - startTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error converting images to binary: {ex.Message}");
        }
    }

    public void ConvertImagesToBinaryInParallel()
    {
        var startTime = System.DateTime.Now;
        try
        {
            var tasks = new List<Task>();

            foreach (var table in _tables)
            {
                var tableCopy = table; // Ensure thread-safe capturing of table
                var task = Task.Run(() =>
                {
                    using (var context = new AppDB()) // Create a new DbContext instance per thread
                    {
                        ConverteTableImagesToBinary(tableCopy, context);
                    }
                });
                tasks.Add(task);
            }

            // Wait for all tasks to complete
            Task.WhenAll(tasks).Wait();

            // After processing all tables, output all errors
            if (_errorLog.Count > 0)
            {
                Console.WriteLine("\nErrors encountered during processing:");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var error in _errorLog)
                {
                    Console.WriteLine(error);
                }
                Console.ResetColor();
                Console.WriteLine($"Total errors: {_errorLog.Count}");
            }

            var endTime = System.DateTime.Now;
            Console.WriteLine($"Image conversion completed in {endTime - startTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error converting images to binary: {ex.Message}");
        }
    }

    private void ConverteTableImagesToBinary(TargetTable table, AppDB context)
    {
        Console.WriteLine($"Processing table: {table.TableName}");
        var imagePaths = GetImagePathsFromDatabase(table.TableName, table.ImagePathColumn, context);
        int processedCount = 0;
        int errorCount = 0;

        foreach (var imagePath in imagePaths)
        {
            var fullImagePath = System.IO.Path.Combine(_imageRootPath, table.ImagesFolderPath, imagePath);

            // Validate file exists before attempting to convert
            if (!System.IO.File.Exists(fullImagePath))
            {
                LogError($"Image not found: {fullImagePath} in {table.TableName} table");
                errorCount++;
                continue;
            }

            try
            {
                var imageBinary = ConvertImageToBinary(fullImagePath);
                if(imageBinary == null)
                {
                    LogError($"Error converting image {imagePath} to binary");
                    errorCount++;
                    continue;
                }
                SaveImageBinaryToDatabase(table.TableName, table.ImagePathColumn, table.BinaryColumn, imagePath, imageBinary, context);
                processedCount++;
            }
            catch (Exception ex)
            {
                LogError($"Error processing image {imagePath}: {ex.Message}");
                errorCount++;
            }
        }

        Console.WriteLine($"Table {table.TableName} completed: {processedCount} images processed, {errorCount} errors");
    }

    private List<string> GetImagePathsFromDatabase(string tableName, string imagePathColumn, AppDB context)
    {
        var imagePaths = new List<string>();
        var query = $"SELECT {imagePathColumn} FROM {tableName} WHERE {imagePathColumn} IS NOT NULL";

        using (var command = context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;
            context.Database.OpenConnection();

            using (var result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    imagePaths.Add(result.GetString(0));
                }
            }
        }

        return imagePaths;
    }

    private void SaveImageBinaryToDatabase(string tableName, string imagePathColumn, string binaryColumn, string imagePath, byte[] imageBinary, AppDB context)
    {
        var query = $"UPDATE {tableName} SET {binaryColumn} = @binaryData WHERE {imagePathColumn} = @imagePath";

        using (var command = context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;
            command.Parameters.Add(new SqlParameter("@binaryData", SqlDbType.VarBinary) { Value = imageBinary ?? (object)DBNull.Value });
            command.Parameters.Add(new SqlParameter("@imagePath", SqlDbType.NVarChar) { Value = imagePath });

            context.Database.OpenConnection();
            command.ExecuteNonQuery();
        }
    }

    private byte[]? ConvertImageToBinary(string imagePath)
    {
        var imageBinary = File.ReadAllBytes(imagePath);

        return _compressor.GetCompressedDocumentBytes(imageBinary, GetImageMIMEType(imagePath));
    }

    private string GetImageMIMEType(string imagePath)
    {
        if (!_contentTypeProvider.TryGetContentType(imagePath, out string contentType))
        {
            contentType = "application/octet-stream";
        }

        return contentType;
    }

    private void LogError(string message)
    {
        _errorLog.Add(message);
    }
}
