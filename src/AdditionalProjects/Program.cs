using System;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Hoho_PhotoVideoOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            //string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string rootPath = @"D:\temp\Boxing2";

            MoveFilesToRoot(rootPath);
            CreateFolders(rootPath);

            string photoPath = Path.Combine(rootPath, "Photo");
            string videoPath = Path.Combine(rootPath, "Video");

            MoveFilesToTypeFolders(rootPath, photoPath, videoPath);
            OrganizePhotos(photoPath);
            OrganizeVideos(videoPath);

            Console.WriteLine("Organization complete.");
        }

        static void MoveFilesToRoot(string rootPath)
        {
            var directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

            foreach (var dir in directories)
            {
                var files = Directory.GetFiles(dir);

                foreach (var file in files)
                {
                    var destFile = Path.Combine(rootPath, Path.GetFileName(file));
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(file, destFile);
                }
            }

            foreach (var dir in directories.Reverse())
            {
                Directory.Delete(dir, true);
            }
        }

        static void CreateFolders(string rootPath)
        {
            Directory.CreateDirectory(Path.Combine(rootPath, "Photo"));
            Directory.CreateDirectory(Path.Combine(rootPath, "Video"));
        }

        static void MoveFilesToTypeFolders(string rootPath, string photoPath, string videoPath)
        {
            var files = Directory.GetFiles(rootPath);

            foreach (var file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                string destFile = null;

                if (IsPhoto(extension))
                {
                    destFile = Path.Combine(photoPath, Path.GetFileName(file));
                }
                else if (IsVideo(extension))
                {
                    destFile = Path.Combine(videoPath, Path.GetFileName(file));
                }

                if (destFile != null)
                {
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(file, destFile);
                }
            }
        }

        static void OrganizePhotos(string photoPath)
        {
            Directory.CreateDirectory(Path.Combine(photoPath, "Horizontal"));
            Directory.CreateDirectory(Path.Combine(photoPath, "Vertical"));

            var files = Directory.GetFiles(photoPath);

            foreach (var file in files)
            {
                var orientation = GetImageOrientation(file);

                if (orientation.HasValue)
                {
                    string targetFolder = orientation.Value == Orientation.Horizontal
                        ? "Horizontal"
                        : "Vertical";

                    var destFile = Path.Combine(photoPath, targetFolder, Path.GetFileName(file));
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(file, destFile);
                }
            }
        }

        static void OrganizeVideos(string videoPath)
        {
            Directory.CreateDirectory(Path.Combine(videoPath, "Horizontal"));
            Directory.CreateDirectory(Path.Combine(videoPath, "Vertical"));

            var files = Directory.GetFiles(videoPath);

            foreach (var file in files)
            {
                var orientation = GetVideoOrientation(file);

                if (orientation.HasValue)
                {
                    string targetFolder = orientation.Value == Orientation.Horizontal
                        ? "Horizontal"
                        : "Vertical";

                    var destFile = Path.Combine(videoPath, targetFolder, Path.GetFileName(file));
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(file, destFile);
                }
            }
        }

        static bool IsPhoto(string extension)
        {
            string[] photoExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
            return photoExtensions.Contains(extension);
        }

        static bool IsVideo(string extension)
        {
            string[] videoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv" };
            return videoExtensions.Contains(extension);
        }

        enum Orientation
        {
            Horizontal,
            Vertical
        }

        static Orientation? GetImageOrientation(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                if (subIfdDirectory != null && subIfdDirectory.TryGetInt32(ExifDirectoryBase.TagOrientation, out int orientation))
                {
                    // Assuming orientation 1 or 3 means horizontal and 6 or 8 means vertical.
                    return (orientation == 6 || orientation == 8) ? Orientation.Vertical : Orientation.Horizontal;
                }
            }
            catch (Exception)
            {
                // Log or handle exceptions if needed
            }

            return null;
        }

        static Orientation? GetVideoOrientation(string filePath)
        {
            // Implement video orientation detection logic here if possible
            // As a placeholder, assuming all videos are horizontal.
            return Orientation.Horizontal;
        }
    }
}
