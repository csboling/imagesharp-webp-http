using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// simple example with just WebP
app.MapGet(
    "/download.webp",
    async ctx =>
    {
        using (Image<Rgba32> image = new(1, 1))
        {
            ctx.Response.ContentType = "image/webp";
            await image.SaveAsWebpAsync(ctx.Response.Body);
        }
    });

// route for comparing any format with the same image written to disk
app.MapGet(
    "/format.{extension}",
    async ctx =>
    {
        var extension = (string)ctx.GetRouteValue("extension")!;
        if (!Configuration.Default.ImageFormatsManager.TryFindFormatByFileExtension(extension, out var format))
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.CompleteAsync();
            return;
        }

        ctx.Response.ContentType = format.DefaultMimeType;
        var encoder = Configuration.Default.ImageFormatsManager.GetEncoder(format);

        using (Image<Rgba32> image = new(1, 1))
        {
            await encoder.EncodeAsync(image, ctx.Response.Body, ctx.RequestAborted);
            await using (var fs = File.OpenWrite($"file.{extension}"))
            {
                await encoder.EncodeAsync(image, fs, ctx.RequestAborted);
            }
        }
    });

app.Run();
