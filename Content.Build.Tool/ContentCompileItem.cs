using System;
using System.Collections.Generic;

public enum BuildAction
{
    None = 0,
    Compile
}

public sealed class ContentCompileItem
{
    public BuildAction Action { get; set; }

    public string SourceFile { get; set; }   // Compile Include=""
    public string Importer { get; set; }
    public string Processor { get; set; }
    public string Name { get; set; }

    // Any extra tags (optional safety)
    public Dictionary<string, string> ExtraMetadata { get; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed class ContentReference
{
    public string ReferenceName { get; set; }
}

public sealed class ContentProjectReference
{
    public string Project { get; set; }   // Reference Include=""
    public string Name { get; set; }
}
