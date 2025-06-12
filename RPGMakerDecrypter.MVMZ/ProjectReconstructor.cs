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

            // 获取源目录下所有一级子目录，建立小写->原始名映射
            var sourceDirs = Directory.Exists(deploymentPath)
                ? Directory.GetDirectories(deploymentPath, "*", SearchOption.TopDirectoryOnly)
                : new string[0];
            var sourceDirMap = sourceDirs.ToDictionary(
                d => Path.GetFileName(d).ToLowerInvariant(),
                d => Path.GetFileName(d));

            foreach (var directory in _directories)
            {
                // 查找实际存在的目录名（忽略大小写）
                if (sourceDirMap.TryGetValue(directory.ToLowerInvariant(), out var realDirName))
                {
                    CopyDirectory(
                        Path.Combine(deploymentPath, realDirName),
                        Path.Combine(outputPath, realDirName));
                }
            }
            
            foreach (var file in _files)
            {
                var srcFile = Path.Combine(deploymentPath, file);
                if (File.Exists(srcFile))
                {
                    File.Copy(srcFile, Path.Combine(outputPath, file));
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
