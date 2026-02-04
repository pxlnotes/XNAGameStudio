using Content.Build;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

public static class ContentProjectConsumer
{
    public static List<ContentCompileItem> ConsumeContentProjects(string buildOutputPath)
    {
        var contentCompileResult = new List<ContentCompileItem>();        
        var contentProjectReferencesResult = new List<ContentProjectReference>();
        var contentReferencesResult = new List<ContentReference>();

        // Content folder inside build output
        string contentDir = Path.Combine(buildOutputPath, "Content");
        if (!Directory.Exists(contentDir))
            return null;

        string[] contentProjFiles = Directory.GetFiles(contentDir, "*.contentproj", SearchOption.AllDirectories);
        if (contentProjFiles.Length == 0)
            return null;

        foreach (var file in contentProjFiles)
        {
            ConsumeSingleContentProj(file, contentCompileResult, contentReferencesResult, contentProjectReferencesResult);
        }

        //Refresh content references if needed
        RefreshContentReferences(buildOutputPath, contentReferencesResult, contentProjectReferencesResult);

        return contentCompileResult;
    }

    private static void RefreshContentReferences(string buildOutputPath, List<ContentReference> contentReferencesResult, List<ContentProjectReference> contentProjectReferencesResult)
    {
        if (contentReferencesResult == null && contentProjectReferencesResult == null)
            return;

        List<string> pipelineAssemblies = new List<string>();

        if (contentReferencesResult != null)
        {
            foreach (var reference in contentReferencesResult)
            {
                pipelineAssemblies.Add(reference.ReferenceName);
            }
        }

        if (contentProjectReferencesResult != null)
        {
            foreach (var projectReference in contentProjectReferencesResult)
            {
                pipelineAssemblies.Add(buildOutputPath + "" + projectReference.Name + ".dll");
            }
        }

        ContentBuilder.UpdatePipelineAssemblies(pipelineAssemblies.ToArray());
    }

    private static void ConsumeSingleContentProj(string filePath, List<ContentCompileItem> contentCompile, 
        List<ContentReference> contentReferences, List<ContentProjectReference> contentProjectReferences)
    {
        XDocument doc = XDocument.Load(filePath);

        // Ignore XML namespaces (important!)
        XNamespace ns = doc.Root.Name.Namespace;

        // Find ALL ItemGroup elements at ANY depth
        var itemGroups = doc
            .Descendants(ns + "ItemGroup");

        foreach (var itemGroup in itemGroups)
        {
            //ItemGroup - Compile           
            var compileNodes = itemGroup
                .Descendants(ns + "Compile");

            foreach (var compile in compileNodes)
            {
                var item = new ContentCompileItem();
                item.Action = BuildAction.Compile;

                // Compile Include attribute
                item.SourceFile = (string)compile.Attribute("Include");

                // Read known metadata
                item.Importer = compile.Element(ns + "Importer")?.Value;
                item.Processor = compile.Element(ns + "Processor")?.Value;
                item.Name = compile.Element(ns + "Name")?.Value;

                // Capture ANY additional metadata automatically
                foreach (var element in compile.Elements())
                {
                    string key = element.Name.LocalName;
                    if (key == "Importer" || key == "Processor" || key == "Name")
                        continue;

                    item.ExtraMetadata[key] = element.Value;
                }

                contentCompile.Add(item);
            }

            //ItemGroup - None           
            var noneNodes = itemGroup
                .Descendants(ns + "None");

            foreach (var none in noneNodes)
            {
                var item = new ContentCompileItem();                
                item.Action = BuildAction.None;

                // Compile Include attribute
                item.SourceFile = (string)none.Attribute("Include");

                // Read known metadata
                item.Importer = none.Element(ns + "Importer")?.Value;
                item.Processor = none.Element(ns + "Processor")?.Value;
                item.Name = none.Element(ns + "Name")?.Value;

                // Capture ANY additional metadata automatically
                foreach (var element in none.Elements())
                {
                    string key = element.Name.LocalName;
                    if (key == "Importer" || key == "Processor" || key == "Name")
                        continue;

                    item.ExtraMetadata[key] = element.Value;
                }

                contentCompile.Add(item);
            }

            //ItemGroup - ProjectReference            
            var projectReferenceNodes = itemGroup
                .Descendants(ns + "ProjectReference");

            foreach (var compile in projectReferenceNodes)
            {
                var item = new ContentProjectReference();

                // ProjectReference Include attribute
                item.Project = (string)compile.Attribute("Include");
                item.Name = compile.Element(ns + "Name")?.Value;

                contentProjectReferences.Add(item);
            }

            //ItemGroup - Reference            
            var referenceNodes = itemGroup
            .Descendants(ns + "Reference");

            foreach (var compile in referenceNodes)
            {
                var item = new ContentReference();

                // Reference Include attribute
                item.ReferenceName = (string)compile.Attribute("Include");

                contentReferences.Add(item);
            }
        }
    }
}
