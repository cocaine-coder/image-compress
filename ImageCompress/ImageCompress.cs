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
        InitImagePaths(param.Input, param.Limit, param.Recurse);

        foreach (var filePath in pathMapImageType[ImageFormat.Jpeg])
        {
            using var stream = new MemoryStream(File.ReadAllBytes(filePath));
            using var image = Image.FromStream(stream);

            var output = string.IsNullOrWhiteSpace(param.Output) ? Path.GetDirectoryName(filePath) : param.Output;
            var destPath = GetDestPath(
                output,
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
            using var stream = new MemoryStream(File.ReadAllBytes(filePath));
            using var bitmap = new Bitmap(stream);
            var quantizer = new nQuant.WuQuantizer();
            using var quantized = quantizer.QuantizeImage(bitmap, 10, 70);

            var output = string.IsNullOrWhiteSpace(param.Output) ? Path.GetDirectoryName(filePath) : param.Output;
            var destPath = GetDestPath(
                output,
                Path.GetFileNameWithoutExtension(filePath),
                Path.GetExtension(filePath),
                param.Convert);

            quantized.Save(destPath, ImageFormat.Png);
        }
    }

    private void InitImagePaths(string inputPath, int limit, bool recurse)
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

        if (recurse)
        {
            foreach (var dirInfo in directoryInfo.GetDirectories())
            {
                InitImagePaths(dirInfo.FullName, limit, recurse);
            }
        }

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
        if (fileInfo.Length < limit * 1024) return null;

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
        while (files.Contains(filePath))
        {
            count += 1;
            filePath = Path.Combine(output, $"{filename}({count}){extension}");
        }

        return filePath;
    }
}