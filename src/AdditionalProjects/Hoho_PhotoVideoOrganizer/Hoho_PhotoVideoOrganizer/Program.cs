using System;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Mpeg;
using MetadataExtractor.Formats.QuickTime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using ImageMagick;


namespace Hoho_PhotoVideoOrganizer
{
    class Program
    {
        private static readonly string SourceFolder = "Sources";
        private static readonly string PublishFolder = "To Publish";

        private static readonly string Publish_Site_Folder = "WebSite";
        private static readonly string Publish_Site_Template_Folder = "[ProductName]";
        private static readonly string Publish_Site_Template_Original_Folder = "Original";
        private static readonly string Publish_Site_Template_Compressed_Folder = "Compressed";

        private static readonly string Publish_FB_Folder = "Facebook";
        private static readonly string Publish_In_Folder = "Instagram";
        private static readonly string Publish_Tk_Folder = "Tiktok";
        private static readonly string Publish_Yb_Folder = "Youtube";
        private static readonly string Publish_Pin_Folder = "Pinterest";

        private static readonly string PhotoFolderName = "Photo";
        private static readonly string VideoFolderName = "Video";

        static async Task Main(string[] args)
        {
            //string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            //string rootPath = @"D:\temp\Boxing2";
            var rootPath = GetRootPath();

            MoveFilesToRoot(rootPath);

            string photoPath = Path.Combine(rootPath, SourceFolder, PhotoFolderName);
            string videoPath = Path.Combine(rootPath, SourceFolder, VideoFolderName);

            MoveFilesToTypeFolders(rootPath, photoPath, videoPath);
            OrganizePhotos(photoPath);
            OrganizeVideos(videoPath);

            Console.WriteLine("Organization complete.");

            Console.WriteLine("Creating additional folders...");
            CreatePublishingDirectory(Path.Combine(rootPath, PublishFolder, Publish_Site_Folder, Publish_Site_Template_Folder, Publish_Site_Template_Original_Folder));
            CreatePublishingDirectory(Path.Combine(rootPath, PublishFolder, Publish_Site_Folder, Publish_Site_Template_Folder, Publish_Site_Template_Compressed_Folder));

            CreatePublishingDirectory(rootPath, Publish_FB_Folder);
            CreatePublishingDirectory(rootPath, Publish_In_Folder);
            CreatePublishingDirectory(rootPath, Publish_Tk_Folder);
            CreatePublishingDirectory(rootPath, Publish_Yb_Folder);
            CreatePublishingDirectory(rootPath, Publish_Pin_Folder);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Congratulation!!! The process is done! Press any key to finish!");
            Console.ReadKey();
        }

        private static string GetRootPath()
        {
            
            string rootPath;

            while (true)
            {
                Console.Write("Enter path to folder with media:");
                rootPath = Console.ReadLine();

                if (System.IO.Directory.Exists(rootPath))
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("ATTENTION!!! There is a risk to loose folders structure or some files!!!!");
                    Console.WriteLine("Double check that path is correct and enter 'y' or 'yes' to continue");
                    var answer = Console.ReadLine().ToLower();
                    if (answer == "y" || answer == "yes")
                    {
                        Console.Clear();
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect path. Try again...");
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            return rootPath;
        }

        private static void CreatePublishingDirectory(string rootPath, string folderName)
        {
            var path = Path.Combine(rootPath, PublishFolder, folderName);
            CreatePublishingDirectory(path);
        }

        private static void CreatePublishingDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }


        static void MoveFilesToRoot(string rootPath)
        {
            Console.WriteLine("Moving all files to root folder...");
            var directories = System.IO.Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

            foreach (var dir in directories)
            {
                
                string folderName = dir.Replace(rootPath, "").Replace("\\","_");
                var files = System.IO.Directory.GetFiles(dir);

                foreach (var file in files)
                {
                    // Create the new file name with folder name prefix
                    string newFileName = $"{folderName}_{Path.GetFileName(file)}";
                    string destFile = Path.Combine(rootPath, newFileName);
                    //var destFile = Path.Combine(rootPath, Path.GetFileName(file));
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(file, destFile);
                }
            }

            foreach (var dir in directories.Reverse())
            {
                System.IO.Directory.Delete(dir, true);
            }
        }

        static void MoveFilesToTypeFolders(string rootPath, string photoPath, string videoPath)
        {
            Console.WriteLine("Sorting files to Photo and Video...");
            var files = System.IO.Directory.GetFiles(rootPath);

            foreach (var file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                string destFile = null;

                if (IsPhoto(extension))
                {
                    if (!System.IO.Directory.Exists(photoPath))
                        System.IO.Directory.CreateDirectory(photoPath);
                    destFile = Path.Combine(photoPath, Path.GetFileName(file));
                }
                else if (IsVideo(extension))
                {
                    if(!System.IO.Directory.Exists(videoPath))
                        System.IO.Directory.CreateDirectory(videoPath);

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
            Console.WriteLine();
            Console.WriteLine("Organizing photos...");
            System.IO.Directory.CreateDirectory(Path.Combine(photoPath, "Horizontal"));
            System.IO.Directory.CreateDirectory(Path.Combine(photoPath, "Vertical"));

            var files = System.IO.Directory.GetFiles(photoPath);

            Console.WriteLine($"Processing {files.Count()} files...");
            var counter = 0;
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

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Processed {++counter} out of {files.Count()} files");
            }
            Console.WriteLine();
            Console.WriteLine("Done");
            Console.WriteLine();
        }

        static void OrganizeVideos(string videoPath)
        {
            Console.WriteLine();
            Console.WriteLine("Organizing videos...");
            System.IO.Directory.CreateDirectory(Path.Combine(videoPath, "Horizontal"));
            System.IO.Directory.CreateDirectory(Path.Combine(videoPath, "Vertical"));

            var files = System.IO.Directory.GetFiles(videoPath);

            Console.WriteLine($"Processing {files.Count()} files...");
            var counter = 0;
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
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Processed {++counter} out of {files.Count()} files");
            }
            Console.WriteLine();
            Console.WriteLine("Done");
            Console.WriteLine();
        }

        static bool IsPhoto(string extension)
        {
            extension = extension.ToLower();
            string[] photoExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".heic" };
            return photoExtensions.Contains(extension);
        }

        static bool IsVideo(string extension)
        {
            extension = extension.ToLower();
            string[] videoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv" };
            return videoExtensions.Contains(extension);
        }

        enum Orientation
        {
            Horizontal,
            Vertical
        }

        //static Orientation? GetImageOrientation(string filePath)
        //{
        //    try
        //    {
        //        var directories = ImageMetadataReader.ReadMetadata(filePath);
        //        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

        //        if (subIfdDirectory != null && subIfdDirectory.TryGetInt32(ExifDirectoryBase.TagOrientation, out int orientation))
        //        {
        //            // Assuming orientation 1 or 3 means horizontal and 6 or 8 means vertical.
        //            return (orientation == 6 || orientation == 8) ? Orientation.Vertical : Orientation.Horizontal;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // Log or handle exceptions if needed
        //    }

        //    return null;
        //}

        static Orientation? GetImageOrientation(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".heic")
            {
                return GetHeicImageOrientation(filePath);
            }
            else
            {
                try
                {
                    using (var image = Image.Load(filePath))
                    {
                        var exifProfile = image.Metadata.ExifProfile;

                        if (exifProfile != null)
                        {
                            // The Exif tag for orientation is 0x0112 (274)
                            exifProfile.TryGetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Orientation, out var orientationValue);

                            if (orientationValue != null)
                            {
                                // Orientation values (1 to 8) correspond to the following orientations:
                                // 1: Horizontal (normal)
                                // 2: Mirror horizontal
                                // 3: Rotate 180
                                // 4: Mirror vertical
                                // 5: Mirror horizontal and rotate 270 CW
                                // 6: Rotate 90 CW
                                // 7: Mirror horizontal and rotate 90 CW
                                // 8: Rotate 270 CW

                                switch (orientationValue.Value)
                                {
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                        return Orientation.Vertical;
                                    case 5:
                                    case 6:
                                    case 7:
                                    case 8:
                                        return Orientation.Horizontal;
                                }
                            }
                        }

                        // If no orientation property is found, assume horizontal
                        return Orientation.Horizontal;
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., file not found, not an image, etc.)
                    Console.WriteLine($"Error reading image orientation: {ex.Message}");
                    return null;
                }
            }
        }

        static Orientation? GetHeicImageOrientation(string filePath)
        {
            try
            {
                using (var image = new MagickImage(filePath))
                {
                    var orientation = image.Orientation;

                    // Assuming TopLeft and BottomRight means horizontal, RightTop and LeftBottom means vertical
                    //return (orientation == OrientationType.RightTop || orientation == OrientationType.LeftBottom) ? Orientation.Vertical : Orientation.Horizontal;

                    var result = image.Width < image.Height? Orientation.Vertical : Orientation.Horizontal;
                    return result;
                }
            }
            catch (Exception)
            {
                // Log or handle exceptions if needed
            }

            return null;
        }

        private static Orientation? GetVideoOrientation(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // Try to get rotation from MP4 metadata
                var movDirectory = directories.OfType<QuickTimeTrackHeaderDirectory>().FirstOrDefault();
                if (movDirectory != null)
                {
                    int width = movDirectory.GetInt32(QuickTimeTrackHeaderDirectory.TagWidth);
                    int height = movDirectory.GetInt32(QuickTimeTrackHeaderDirectory.TagHeight);
                    var matrix = movDirectory.GetString(QuickTimeTrackHeaderDirectory.TagMatrix);

                    // Check for rotation information
                    if (movDirectory.TryGetInt32(QuickTimeTrackHeaderDirectory.TagRotation, out int rotation))
                    {
                        if(width > height) //horizontal, but...
                        {
                            if(rotation == 90 || rotation == 270 || rotation == -90 || rotation == -270)
                                return Orientation.Vertical;
                            return Orientation.Horizontal;
                        }
                        else //vertical, but
                        {
                            if (rotation == 90 || rotation == 270 || rotation == -90 || rotation == -270)
                                return Orientation.Horizontal;
                            return Orientation.Vertical;
                        }
                    }

                    // Fallback if no rotation information is found
                    return width > height ? Orientation.Horizontal : Orientation.Vertical;
                }

                // If MP4 metadata is not available or does not provide required information, fallback
                Console.WriteLine($"Rotation information not available for {filePath}. Using width and height as fallback.");
                var fallbackDirectory = directories.FirstOrDefault(d => d.Tags.Any(t => t.Type == 1)); // Generic fallback
                if (fallbackDirectory != null)
                {
                    int width = fallbackDirectory.GetInt32(1); // Try to get width
                    int height = fallbackDirectory.GetInt32(2); // Try to get height

                    return width > height ? Orientation.Horizontal : Orientation.Vertical;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading video file {filePath}: {ex.Message}");
            }

            return null;
        }
    }
}
