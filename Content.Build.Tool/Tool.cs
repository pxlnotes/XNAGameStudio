using Content.Build;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Build
{
    public static class Tool
    {
        private static Dictionary<string, AssetRule> assetRules;
        private static List<ContentCompileItem> contentCompileItems;

        public static int Run(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string contentRoot = Path.Combine(baseDir, "Content");

            // setup the content builder
            string platform = ConfigurationManager.AppSettings["XnaPlatform"];
            string profile = ConfigurationManager.AppSettings["XnaProfile"];
            string config = ConfigurationManager.AppSettings["Configuration"];
            var contentBuilderProps = new BuildProperties(
                XnaPlatform.Windows,
                profile,
                "v4.0",
                BuildConfiguration.Release
            );

            //load content build properties from .contentproj if exists
            bool hasContentProj = LoadContentProj(baseDir);

            //load asset build config with default rules
            LoadAssetRules();

            using (var builder = new ContentBuilder(contentBuilderProps))
            {
                if (hasContentProj && contentCompileItems.Count > 0)
                    AddAssetsFromContentProj(builder, contentRoot);
                else
                    AddAllAssets(builder, contentRoot);

                string err = builder.Build();
                if (err != null)
                {                    
                    ErrorLogger.WriteErrorFile(baseDir, $"BUILD FAILED: {err}");
                    return 2;
                }

                CopyXnb(builder.OutputDirectory, contentRoot);
            }

            return 0;
        }

        private static void AddAllAssets(ContentBuilder builder, string contentRoot)
        {
            foreach (var file in Directory.GetFiles(contentRoot, "*.*", SearchOption.AllDirectories))
            {
                AddAsset(builder, file);
            }
        }

        private static void AddAssetsFromContentProj(ContentBuilder builder, string contentRoot)
        {
            // Build lookup once
            var fileLookup = Directory
                .EnumerateFiles(contentRoot, "*.*", SearchOption.AllDirectories)
                .ToLookup(
                    f => Path.GetFileName(f),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var item in contentCompileItems)
            {
                string itemSourceFile = Path.GetFileName(item.SourceFile);
                var matches = fileLookup[itemSourceFile];

                if (!matches.Any())
                {
                    Console.WriteLine($"[WARN] Asset not found: {item.SourceFile}");
                    continue;
                }

                foreach (var fullPath in matches)
                {
                    AddAsset(builder, fullPath, item);
                }
            }
        }

        private static bool LoadContentProj(string buildDir)
        {
            contentCompileItems = ContentProjectConsumer.ConsumeContentProjects(buildDir);
            if (contentCompileItems == null)
                return false;

            return true;
        }

        private static void LoadAssetRules()
        {
            assetRules = ConfigurationManager
                .AppSettings
                .AllKeys
                .Where(k => k.StartsWith("extension.", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    key =>
                    {
                        return "." + key.Substring("extension.".Length);
                    },
                    key =>
                    {
                        var value = ConfigurationManager.AppSettings[key];
                        var parts = value.Split(';');

                        if (parts.Length != 2)
                            throw new ConfigurationErrorsException(
                                $"Invalid asset rule '{key}'. Expected 'Importer;Processor'.");

                        return new AssetRule
                        {
                            Extension = "." + key.Substring("extension.".Length),
                            Importer = parts[0],
                            Processor = parts[1]
                        };
                    },
                    StringComparer.OrdinalIgnoreCase
                );
        }

        private static void CopyDirectoryRecursive(
            string sourceDir,
            string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string targetFile =
                    Path.Combine(
                        targetDir,
                        Path.GetFileName(file));

                File.Copy(file, targetFile, overwrite: true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir =
                    Path.Combine(
                        targetDir,
                        Path.GetFileName(dir));

                CopyDirectoryRecursive(dir, targetSubDir);
            }
        }

        static void AddAsset(ContentBuilder builder, string file, ContentCompileItem contentCompileItem = null)
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();

            if (ext == ".xnb")
                return;

            string name = Path.ChangeExtension(file, null);
            name = Path.GetFileNameWithoutExtension(file);

            if (contentCompileItem != null)
            {
                //copy to output as-is
                var outputDir = Path.Combine(builder.BuildDirectory, Path.GetDirectoryName(contentCompileItem.SourceFile) ?? string.Empty);
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                var targetFile = Path.Combine(builder.BuildDirectory, contentCompileItem.SourceFile);
                File.Copy(file, targetFile, overwrite: true);

                if (contentCompileItem.Action == BuildAction.None)
                {                    
                    //copy to output bin as well
                    var outputBinDir = Path.Combine(builder.OutputDirectory, Path.GetDirectoryName(contentCompileItem.SourceFile) ?? string.Empty);
                    var targetBinFile = Path.Combine(builder.OutputDirectory, contentCompileItem.SourceFile);
                    
                    if (!Directory.Exists(outputBinDir))
                        Directory.CreateDirectory(outputBinDir);

                    File.Copy(file, targetBinFile, overwrite: true);
                }
                else if (contentCompileItem.Action == BuildAction.Compile)
                {
                    builder.Add(file, name, contentCompileItem.Importer, contentCompileItem.Processor, contentCompileItem);
                }
            }
            else if (assetRules.TryGetValue(ext, out var rule))
            {
                builder.Add(file, name, rule.Importer, rule.Processor, null);
            }
            else
            {
                Console.WriteLine(
                    $"No content rule defined for {ext}");
            }
        }

        static void CopyXnb(string src, string dst)
        {
            //clean up content
            Directory.Delete(dst, true);

            foreach (var file in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
            {
                // path relative to src
                string relativePath = file.Substring(src.Length)
                                          .TrimStart(Path.DirectorySeparatorChar);

                string destinationPath = Path.Combine(dst, relativePath);

                // ensure directory exists
                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                File.Copy(file, destinationPath, true);
            }
        }
    }
}
