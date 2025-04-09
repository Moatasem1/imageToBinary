using ImagesToBinary.Models;

namespace ImagesToBinary.Services;

class ImageSettingsValidator
{
    private List<string> _errors;
    private ImageSettings _imageSettings;

    public ImageSettingsValidator(ImageSettings settings)
    {
        _errors = new List<string>();
        _imageSettings = settings;
    }

    public List<string> ValidateImageSettings()
    {
        ValidateRootPath(_imageSettings.RootPath);
        ValidateTables(_imageSettings.Tables);


        return _errors;
    }

    private void ValidateRootPath(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(_imageSettings.RootPath))
        {
            _errors.Add($"RootPath '{_imageSettings.RootPath}' can't be null or empty");
            return;
        }

        if(!Directory.Exists(_imageSettings.RootPath))
        {
            _errors.Add($"RootPath '{_imageSettings.RootPath}' does not exist.");
            return;
        }
    }

    private void ValidateTables(List<TargetTable> tables)
    {
        if (tables == null || tables.Count == 0)
        {
            _errors.Add("No tables found in settings.");
            return;
        }

        foreach (var table in tables)
            ValidateTableInfo(table);
    }

    private void ValidateTableInfo(TargetTable table)
    {
        ValidateTableName(table.TableName);
        ValidateImagePathColumn(table.ImagePathColumn);
        ValidateBinaryColumn(table.BinaryColumn);
        var imagesFolderPath = Path.Combine(_imageSettings.RootPath, table.ImagesFolderPath);
        ValidateImagesFolderPath(imagesFolderPath);
    }

    private void ValidateTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            _errors.Add("TableName can't be null or empty");
            return;
        }
    }

    private void ValidateImagePathColumn(string imagePathColumn)
    {
        if (string.IsNullOrWhiteSpace(imagePathColumn))
        {
            _errors.Add("ImagePathColumn can't be null or empty");
            return;
        }
    }

    private void ValidateBinaryColumn(string binaryColumn)
    {
        if (string.IsNullOrWhiteSpace(binaryColumn))
        {
            _errors.Add("BinaryColumn can't be null or empty");
            return;
        }
    }

    private void ValidateImagesFolderPath(string imagesFolderPath)
    {
        
        if (!Directory.Exists(imagesFolderPath))
        {
            _errors.Add($"ImagesFolderPath '{imagesFolderPath}' does not exist.");
            return;
        }
    }
}
