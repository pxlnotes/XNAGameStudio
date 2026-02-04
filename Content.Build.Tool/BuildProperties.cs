using Microsoft.Build.Evaluation;

namespace Content.Build
{
    public sealed class AssetRule
    {
        public string Extension { get; set; }
        public string Importer { get; set; }
        public string Processor { get; set; }
    }

    public enum XnaPlatform
    {
        Windows,
        Xbox360
    }

    public enum XnaProfile
    {
        Reach,
        HiDef
    }

    public enum BuildConfiguration
    {
        Debug,
        Release
    }

    public readonly struct BuildProperties
    {
        public XnaPlatform Platform { get; }
        public string Profile { get; }
        public string FrameworkVersion { get; }
        public BuildConfiguration Configuration { get; }

        public BuildProperties(
            XnaPlatform platform,
            string profile,
            string frameworkVersion,
            BuildConfiguration configuration)
        {
            Platform = platform;
            Profile = profile;
            FrameworkVersion = frameworkVersion;
            Configuration = configuration;
        }

        public void ApplyTo(Project project)
        {
            project.SetProperty("XnaPlatform", Platform.ToString());
            project.SetProperty("XnaProfile", Profile.ToString());
            project.SetProperty("XnaFrameworkVersion", FrameworkVersion);
            project.SetProperty("Configuration", Configuration.ToString());
        }
    }
}
