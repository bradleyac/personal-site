using System.Web;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;

public class PhotosModel : PageModel
{
    private readonly ILogger<PhotosModel> _logger;
    private readonly IAmazonS3 _s3Client;

    public PhotosModel(IAmazonS3 s3Client, ILogger<PhotosModel> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public List<string> ImagesToDisplay { get; private set; } = new List<string>();

    public async Task OnGetAsync()
    {
        var response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = "bradley-ac-photos-resized",
        });

        foreach (var obj in response.S3Objects.Where(obj => obj.Key.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)))
        {
            ImagesToDisplay.Add(GetObjectUrl(obj));
        }
    }

    public string GetObjectUrl(S3Object obj)
    {
        return $"https://{obj.BucketName}.s3.us-east-1.amazonaws.com/{HttpUtility.UrlEncode(obj.Key)}";
    }
}