using System.Drawing;
using System.Drawing.Imaging;

public interface IImageCompress
{
    void Compress(CompressParameter param);
}

public class ImageCompress : IImageCompress
{
    private Dictionary<ImageFormat, List<string>> pathMapImageType =
        new Dictionary<ImageFormat, List<string>>()
        {
            [ImageFormat.Jpeg] = new List<string>(),
            [ImageFormat.Png] = new List<string>(),
        };

    public void Compress(CompressParameter param)
    {
        InitImagePaths(param.Input, param.Limit);

        foreach (var filePath in pathMapImageType[ImageFormat.Jpeg])
        {
            using var image = Image.FromFile(filePath);
            var destPath = GetDestPath(
                param.Output,
                Path.GetFileNameWithoutExtension(filePath),
                Path.GetExtension(filePath),
                param.Convert);
            var encoder = ImageCodecInfo
                .GetImageEncoders()
                .First(x => x.FormatID == image.RawFormat.Guid);

            image.Save(
                destPath,
                encoder,
                new EncoderParameters()
                {
                    Param = new[] { new EncoderParameter(Encoder.Quality, param.Quality) }
                });
        }

        foreach (var filePath in pathMapImageType[ImageFormat.Png])
        {
            using var bitmap = new Bitmap(filePath);
            var quantizer = new nQuant.WuQuantizer();
            using var quantized = quantizer.QuantizeImage(bitmap, 10, 70);
            var destPath = GetDestPath(
                param.Output,
                Path.GetFileNameWithoutExtension(filePath),
                Path.GetExtension(filePath),
                param.Convert);

            quantized.Save(destPath,ImageFormat.Png);
        }
    }

    private void InitImagePaths(string inputPath, int limit)
    {
        var files = new List<string>();
        if (File.Exists(inputPath))
        {
            var format = ValidateAndGetImageType(new FileInfo(inputPath), limit);
            if (format != null)
            {
                pathMapImageType[format].Add(inputPath);
            }

            return;
        }

        var directoryInfo = new DirectoryInfo(inputPath);
        foreach (var fileInfo in directoryInfo.GetFiles())
        {
            var format = ValidateAndGetImageType(fileInfo, limit);
            if (format != null)
            {
                pathMapImageType[format].Add(fileInfo.FullName);
            }
        }
    }

    private ImageFormat? ValidateAndGetImageType(FileInfo fileInfo, int limit)
    {
        if (fileInfo.Length < limit) return null;

        var tempPath = fileInfo.FullName.ToLower();
        if (tempPath.EndsWith(".jpg") || tempPath.EndsWith(".jpeg"))
        {
            return ImageFormat.Jpeg;
        }

        if (tempPath.EndsWith(".png"))
        {
            return ImageFormat.Png;
        }

        return null;
    }

    private string GetDestPath(string output, string filename, string extension, bool convert)
    {
        var filePath = Path.Combine(output, $"{filename}{extension}");
        if (convert)
        {
            return filePath;
        }

        var files = Directory.GetFiles(output);
        int count = 0;
        if (files.Contains(filePath))
        {
            count += 1;
            filePath = Path.Combine(output, $"{filename}({count}){extension}");
        }

        return filePath;
    }
}