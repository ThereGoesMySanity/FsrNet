using System;
using System.Linq;
using ImageMagick;
using Microsoft.Extensions.FileProviders;

namespace FsrNet.Services;

public class ImageStore
{
    private readonly IWebHostEnvironment env;

    private readonly string imageRootPath = "images";

    // Do not touch default images
    private readonly string[] defaultImages = ["default.gif", "alternate.gif"];

    public ImageStore(IWebHostEnvironment env)
    {
        this.env = env;
        foreach (var i in GetImages())
        {
            GeneratePreview(i.Name).Wait();
        }
    }

    public IEnumerable<IFileInfo> GetImages()
    {
        return env.WebRootFileProvider.GetDirectoryContents(imageRootPath).Where(i => Path.GetExtension(i.Name) == ".gif");
    }

    public string[] GetImageNames() => GetImages().OrderBy(i => i.LastModified).Select(i => i.Name).ToArray();

    public IFileInfo GetImageInfo(string image)
    {
        return env.WebRootFileProvider.GetFileInfo(Path.Combine(imageRootPath, image));
    }

    public void RemoveImage(string image)
    {
        if (defaultImages.Contains(image)) return;

        var img = GetImageInfo(image);
        if(img.Exists) File.Delete(img.PhysicalPath!);
        var prev = GetPreview(image);
        if (prev.Exists) File.Delete(prev.PhysicalPath!);
    }

    public IFileInfo GetPreview(string image) => env.WebRootFileProvider.GetFileInfo(Path.Combine(imageRootPath, "previews", image));
    public async Task GeneratePreview(string file)
    {
        using var image = new MagickImageCollection(GetImageInfo(file).CreateReadStream());
        using var newImage = new MagickImageCollection();

        image.Coalesce();

        foreach (var i in image)
        {
            var newi = new MagickImage(MagickColors.Silver, 64 * 3, 64 * 3);
            if (image[0].Width == 64)
            {
                foreach (var (x, y) in new (int x, int y)[] {
                    new (64, 0),
                    new (128, 64),
                    new (64, 128),
                    new (0, 64),
                })
                {
                    newi.CopyPixels(i, new MagickGeometry(64), x, y);
                    i.Rotate(90);
                }
            }
            else if (image[0].Width == 256)
            {
                foreach (var (x, y, off) in new (int x, int y, int off)[] {
                    new (0, 64, 0),
                    new (64, 128, 64),
                    new (64, 0, 128),
                    new (128, 64, 192),
                })
                {
                    newi.CopyPixels(i, new MagickGeometry(off, 0, 64, 64), x, y);
                }
            }
            newi.AnimationDelay = i.AnimationDelay;
            newImage.Add(newi);
        }
        await newImage.WriteAsync(GetPreview(file).PhysicalPath!);
    }

    public async Task<string?> ConvertAndSave(string filename, Stream inFile)
    {
        if (defaultImages.Contains(filename)) return null;

        using var image = new MagickImageCollection();
        if (Path.GetExtension(filename) == ".gif") await image.ReadAsync(inFile);
        else image.Add(new MagickImage(inFile));

        image.Coalesce();
        foreach(var i in image) i.BackgroundColor = MagickColors.Black;

        if (image.Any(i => i.Height != 64 || new uint[]{64, 128, 256}.Contains(i.Width)))
        {
            foreach(var i in image)
            {
                i.Resize(64, 64);
                i.Extent(64, 64, Gravity.Center);
            }
        }
        foreach(var i in image)
        {
            i.BackgroundColor = MagickColors.Black;
            i.Alpha(AlphaOption.Remove);
        }
        if (image.Count > 8)
        {
            var delays = image.Select((img, idx) => img.AnimationDelay).ToArray();
            var seq_new = gifDownsample(delays, 8);
            foreach (var (index, delay) in seq_new)
            {
                image[index].AnimationDelay = delay;
            }
            foreach (var i in Enumerable.Range(0, image.Count).Except(seq_new.Select(i => i.index)).OrderDescending())
            {
                image.RemoveAt(i);
            }
        }

        var newFile = Path.GetFileNameWithoutExtension(filename) + ".gif";
        await image.WriteAsync(GetImageInfo(newFile).PhysicalPath!);
        await GeneratePreview(newFile);

        if (GetImageInfo(newFile).Exists) return newFile;
        return null;
    }

    private List<(int index, uint delay)> gifDownsample(uint[] delays, int count)
    {
        if (delays.Length <= count) return delays.Select((d, i) => (i, d)).ToList();
        var delays_acc = new uint[delays.Length + 1];
        uint sum = 0;
        for (int i = 0; i < delays_acc.Length; i++)
        {
            delays_acc[i] = sum;
            if (i < delays.Length) sum += delays[i];
        }

        return gifDownsample(delays, delays_acc, 0, delays.Length, count).ToList();
    }
    private IEnumerable<(int index, uint delay)> gifDownsample(uint[] delays, uint[] delays_acc, int start, int end, int count)
    {
        if (start == end) return [];
        var groups = Enumerable.Range(0, count)
                .Select(x => Array.BinarySearch(delays_acc, start, end - start,  delays_acc[start] + (delays_acc[end] - delays_acc[start]) * x / count))
                .Select(idx => idx >= 0? idx : -idx - 1)
                .GroupBy(x => x)
                .Select(x => (x.Key, x.Count())).ToArray();

        int[] g2;
        if (groups.Length == count)
        {
            g2 = groups.Select(g => g.Key).ToArray();
            return g2.Zip(g2.Skip(1).Append(end)).Select(i => (i.First, delays_acc[i.Second] - delays_acc[i.First]));
        }
        g2 = groups.Where(g => g.Item2 > 1).Select(g => g.Key).ToArray();
        var remaining = count - g2.Length;
        var remainingTotal = end - start - g2.Length;

        return g2.Prepend(start - 1).Zip(g2.Append(end))
                .Aggregate((Enumerable.Empty<(int, uint)>(), remaining, remainingTotal), (acc, i) => 
                    {
                        var subtotal = (i.Second - i.First - 1);
                        if (subtotal == 0) return acc;
                        var subcount = subtotal * acc.remaining / acc.remainingTotal;
                        return (acc.Item1.Concat(gifDownsample(delays, delays_acc, i.First + 1, i.Second, subcount)), acc.remaining - subcount, acc.remainingTotal - subtotal);
                    })
                .Item1
                .Concat(g2.Select(i => (i, delays[i])))
                .OrderBy(i => i.Item1);
    }
}