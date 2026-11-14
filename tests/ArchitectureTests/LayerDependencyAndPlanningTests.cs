using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Domain.Entities;
using Xunit;

namespace Dockyra.ArchitectureTests;

public sealed class LayerDependencyAndPlanningTests
{
    private static readonly string RepoRoot = FindRepositoryRoot();

    [Fact]
    public void Domain_ShouldHaveZeroProjectAndPackageReferences()
    {
        XDocument csproj = LoadCsproj("src/Dockyra.Domain/Dockyra.Domain.csproj");
        Assert.Empty(GetProjectReferences(csproj));
        Assert.Empty(GetPackageReferences(csproj));
    }

    [Fact]
    public void Contracts_ShouldHaveZeroProjectAndPackageReferences()
    {
        XDocument csproj = LoadCsproj("src/Dockyra.Contracts/Dockyra.Contracts.csproj");
        Assert.Empty(GetProjectReferences(csproj));
        Assert.Empty(GetPackageReferences(csproj));
    }

    [Fact]
    public void Application_ShouldOnlyReferenceDomainAndContracts()
    {
        XDocument csproj = LoadCsproj("src/Dockyra.Application/Dockyra.Application.csproj");
        IReadOnlyList<string> refs = GetProjectReferences(csproj);

        Assert.All(refs, r =>
            Assert.True(
                r.EndsWith("Dockyra.Domain.csproj", StringComparison.OrdinalIgnoreCase) ||
                r.EndsWith("Dockyra.Contracts.csproj", StringComparison.OrdinalIgnoreCase),
                $"Forbidden reference in Dockyra.Application: {r}"));
    }

    [Fact]
    public void PresentationWpf_ShouldNeverReferenceWindowsIntegrationOrInfrastructure()
    {
        XDocument csproj = LoadCsproj("src/Dockyra.Presentation.Wpf/Dockyra.Presentation.Wpf.csproj");
        IReadOnlyList<string> refs = GetProjectReferences(csproj);

        Assert.DoesNotContain(refs, r => r.Contains("WindowsIntegration", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(refs, r => r.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void WindowsIntegration_ShouldNeverReferencePresentationWpf()
    {
        XDocument csproj = LoadCsproj("src/Dockyra.WindowsIntegration/Dockyra.WindowsIntegration.csproj");
        IReadOnlyList<string> refs = GetProjectReferences(csproj);

        Assert.DoesNotContain(refs, r => r.Contains("Presentation.Wpf", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(refs, r => r.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CoreLayers_ShouldNotExposeIntPtrOrWpfTypesInPublicSignatures()
    {
        Assembly[] coreAssemblies =
        [
            typeof(TaskbarLayout).Assembly,
            typeof(WindowId).Assembly,
            typeof(ITaskbarService).Assembly
        ];

        foreach (Assembly assembly in coreAssemblies)
        {
            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    Assert.NotEqual(typeof(IntPtr), prop.PropertyType);
                    Assert.DoesNotContain("System.Windows", prop.PropertyType.Namespace ?? string.Empty);
                }

                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    Assert.NotEqual(typeof(IntPtr), method.ReturnType);
                    foreach (ParameterInfo param in method.GetParameters())
                    {
                        Assert.NotEqual(typeof(IntPtr), param.ParameterType);
                    }
                }
            }
        }
    }

    [Fact]
    public void ModuleRegistry_AllSourcePathsTestsAndPlanningDocs_ShouldExistOnDisk()
    {
        string registryPath = Path.Combine(RepoRoot, "planning", "modules", "module-registry.yaml");
        Assert.True(File.Exists(registryPath), "module-registry.yaml must exist.");

        string content = File.ReadAllText(registryPath);
        MatchCollection idMatches = Regex.Matches(content, @"^\s*-\s*id:\s*(MOD-[A-Z0-9-]+)", RegexOptions.Multiline);
        Assert.True(idMatches.Count >= 9, "Expected at least 9 registered modules in module-registry.yaml.");

        HashSet<string> uniqueIds = new(StringComparer.Ordinal);
        foreach (Match match in idMatches)
        {
            string id = match.Groups[1].Value;
            Assert.True(uniqueIds.Add(id), $"Duplicate module ID '{id}' in module-registry.yaml.");
        }

        MatchCollection pathMatches = Regex.Matches(
            content,
            @"^\s*-\s*((?:src|tests|planning)/[^\r\n]+)",
            RegexOptions.Multiline);

        Assert.NotEmpty(pathMatches);
        foreach (Match match in pathMatches)
        {
            string relPath = match.Groups[1].Value.Trim();
            string fullPath = Path.Combine(RepoRoot, relPath.Replace('/', Path.DirectorySeparatorChar));
            bool exists = File.Exists(fullPath) || Directory.Exists(fullPath);
            Assert.True(exists, $"Referenced path in module-registry.yaml does not exist: {relPath}");
        }
    }

    [Fact]
    public void AgentSystem_AllAgentsRulesAndSkills_ShouldExistWithValidFrontmatter()
    {
        string[] expectedAgents =
        [
            "architect.md", "planner.md", "wpf-developer.md", "windows-specialist.md",
            "backend-developer.md", "performance-engineer.md", "qa-engineer.md",
            "security-reviewer.md", "documentation-agent.md"
        ];

        foreach (string agentFile in expectedAgents)
        {
            string path = Path.Combine(RepoRoot, ".agents", "agents", agentFile);
            Assert.True(File.Exists(path), $"Missing agent file: {agentFile}");
            string text = File.ReadAllText(path);
            Assert.StartsWith("---", text.TrimStart());
            Assert.Contains("description:", text);
        }

        string[] expectedSkills =
        [
            "project-onboarding", "implement-feature", "fix-bug", "create-module",
            "architecture-review", "performance-audit", "release-check"
        ];

        foreach (string skillDir in expectedSkills)
        {
            string path = Path.Combine(RepoRoot, ".agents", "skills", skillDir, "SKILL.md");
            Assert.True(File.Exists(path), $"Missing SKILL.md for skill: {skillDir}");
            string text = File.ReadAllText(path);
            Assert.StartsWith("---", text.TrimStart());
            Assert.Contains($"name: {skillDir}", text);
            Assert.Contains("description:", text);
        }
    }

    [Fact]
    public void PlanningDocs_AllRelativeMarkdownLinks_ShouldResolveToExistingFiles()
    {
        string planningRoot = Path.Combine(RepoRoot, "planning");
        string[] mdFiles = Directory.GetFiles(planningRoot, "*.md", SearchOption.AllDirectories);
        Assert.NotEmpty(mdFiles);

        var linkRegex = new Regex(@"\[[^\]]+\]\(([^)#]+)(?:#[^)]*)?\)", RegexOptions.Compiled);

        foreach (string mdFile in mdFiles)
        {
            string content = File.ReadAllText(mdFile);
            string baseDir = Path.GetDirectoryName(mdFile)!;

            foreach (Match match in linkRegex.Matches(content))
            {
                string target = match.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(target) ||
                    target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    target.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    target.StartsWith("file://", StringComparison.OrdinalIgnoreCase) ||
                    target.StartsWith("customization://", StringComparison.OrdinalIgnoreCase) ||
                    target.StartsWith("conversation://", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string resolved = Path.GetFullPath(Path.Combine(baseDir, target.Replace('/', Path.DirectorySeparatorChar)));
                bool exists = File.Exists(resolved) || Directory.Exists(resolved);
                Assert.True(exists, $"Broken link '{target}' in {Path.GetRelativePath(RepoRoot, mdFile)}");
            }
        }
    }

    private static XDocument LoadCsproj(string relativePath)
    {
        string fullPath = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return XDocument.Load(fullPath);
    }

    private static IReadOnlyList<string> GetProjectReferences(XDocument doc) =>
        doc.Descendants("ProjectReference")
           .Select(e => e.Attribute("Include")?.Value ?? string.Empty)
           .Where(v => !string.IsNullOrWhiteSpace(v))
           .ToList();

    private static IReadOnlyList<string> GetPackageReferences(XDocument doc) =>
        doc.Descendants("PackageReference")
           .Select(e => e.Attribute("Include")?.Value ?? string.Empty)
           .Where(v => !string.IsNullOrWhiteSpace(v))
           .ToList();

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Dockyra.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate Dockyra.sln repository root.");
    }
}

