using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TaskProcessing.Functions.Functions;

public class BlobTriggerFunction(ILogger<BlobTriggerFunction> logger)
{
    // Fires whenever a blob is created or updated in the trigger-uploads container
    [Function("ProcessUploadedBlob")]
    public void Run(
        [BlobTrigger("trigger-uploads/{name}", Connection = "AzureWebJobsStorage")] Stream blob,
        string name)
    {
        logger.LogInformation("Blob trigger fired: {Name} ({Size} bytes)", name, blob.Length);

        // In a real app: resize image, extract text from PDF, run virus scan, etc.
        var extension = Path.GetExtension(name).ToLowerInvariant();
        var action = extension switch
        {
            ".jpg" or ".png" or ".gif" => "image resize queued",
            ".pdf" => "text extraction queued",
            ".csv" => "import processing queued",
            _ => "generic processing queued"
        };

        logger.LogInformation("File {Name}: {Action}", name, action);
    }
}
