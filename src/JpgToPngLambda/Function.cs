using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.S3.Model;
using Imageflow.Fluent;

namespace JpgToPngLambda;

public class Function
{
    /// <summary>
    /// The main entry point for the custom runtime.
    /// </summary>
    /// <param name="args"></param>
    private static async Task Main(string[] args)
    {
        Func<S3Event, ILambdaContext, Task> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
            .Build()
            .RunAsync();
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        IAmazonS3 amazonS3 = new AmazonS3Client();
        context.Logger.LogInformation($"evnt.Records == null ? {evnt.Records == null}");
        var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();
        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;
            if (s3Event == null)
            {
                continue;
            }

            try
            {
                var response = await amazonS3.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = s3Event.Bucket.Name,
                    Key = s3Event.Object.Key,
                });

                Stream jpgStream = response.ResponseStream;
                MemoryStream encodedStream = new MemoryStream();

                using (ImageJob job = new ImageJob())
                {
                    var result = await job.Decode(jpgStream, false).ConstrainWithin(1000, 1000).Encode(new StreamDestination(encodedStream, false), new PngQuantEncoder()).Finish().InProcessAsync();
                }

                var putObjectResponse = await amazonS3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = s3Event.Bucket.Name,
                    Key = Path.ChangeExtension(s3Event.Object.Key, ".png"),
                    InputStream = encodedStream,
                });

                context.Logger.LogInformation($"Uploaded with response {putObjectResponse.HttpStatusCode}");
            }
            catch (Exception e)
            {
                context.Logger.LogError(e.Message);
                context.Logger.LogError(e.StackTrace);
                throw;
            }
        }
    }
}