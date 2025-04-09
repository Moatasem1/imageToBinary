using ImagesToBinary.Common;
using ImagesToBinary.Models;
using ImagesToBinary.Services;
using Microsoft.Extensions.Configuration;

namespace ImagesToBinary;

internal class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();

        
        var imageSettings = config.GetSection("ImageSettings").Get<ImageSettings>();

        if (imageSettings == null)
        {
            Console.WriteLine("ImageSettings section not found in appsettings.json");
            return;
        }

        var imageSettingsValidator = new ImageSettingsValidator(imageSettings);
        var errors = imageSettingsValidator.ValidateImageSettings();

        if (errors.Count > 0)
        {
            Console.WriteLine("Validation Faild, With Errors:");
            Logger.LogErrors(errors);
            return;
        }

        var imagesPathToBinaryService = new ImagesPathToBinaryService(imageSettings);

        await Task.Run(() => imagesPathToBinaryService.ConvertImagesToBinaryInParallel());
    }
}