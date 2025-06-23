using System.IO;
using System.Linq;

namespace RPGMakerDecrypter.MVMZ
{
    public abstract class ProjectReconstructor
    {
        // Directories that should exist in the project directory
        private readonly string[] _directories = {
            "audio",
            "css",
            "data",
            "dataex",
            "effects",
            "fonts",
            "icon",
            "img",
            "js",
            "movies"
        };

        // Files that should exist in the project directory
        private readonly string[] _files =
        {
            "index.html",
            "package.json"
        };

        protected abstract void CreateProjectFile(string outputPath);
        
        public virtual void Reconstruct(string deploymentPath, string outputPath)
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
                Directory.CreateDirectory(outputPath);
            }

            // Get all top-level subdirectories and create lowercase-to-original mapping
            var sourceDirectories = Directory.Exists(deploymentPath)
                ? Directory.GetDirectories(deploymentPath, "*", SearchOption.TopDirectoryOnly)
                : new string[0];
            var sourceDirectoryMap = sourceDirectories.ToDictionary(
                directoryPath => Path.GetFileName(directoryPath).ToLowerInvariant(),
                directoryPath => Path.GetFileName(directoryPath));

            foreach (var directory in _directories)
            {
                // Find actual directory name (case-insensitive)
                if (sourceDirectoryMap.TryGetValue(directory.ToLowerInvariant(), out var actualDirectoryName))
                {
                    CopyDirectory(
                        Path.Combine(deploymentPath, actualDirectoryName),
                        Path.Combine(outputPath, actualDirectoryName));
                }
            }
            
            foreach (var file in _files)
            {
                var sourceFile = Path.Combine(deploymentPath, file);
                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, Path.Combine(outputPath, file));
                }
            }
            
            CreateProjectFile(outputPath);
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                return;
            }
            
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFilePath = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFilePath);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var destDirectoryPath = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destDirectoryPath);
            }
        }
    }
}
