using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Utilities.Collections;
using Nuke.GitHub;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.GitHub.GitHubTasks;
using static Nuke.Common.IO.PathConstruction;

[GitHubActions("release-main", GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "master", "main" },
    InvokedTargets = new[] { nameof(Push), nameof(PublishGithubRelease) },
    PublishArtifacts = true)]
public partial class Build
{
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";

    Target Pack => _ => _
        .DependsOn(Test)
        .Produces(PackagesDirectory / "*.nupkg")
        .Executes(() =>
        {
            DotNetPack(_ => _
                .Apply(PackSettings));

            ReportSummary(_ => _
                .AddPair("Packages", PackagesDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });

    Target Push => _ => _
        .DependsOn(Pack)
        .OnlyWhenStatic(() => IsServerBuild)
        .Executes(() =>
        {
            Log.Information("Running push to packages directory.");
            
            GlobFiles(PackagesDirectory, "*.nupkg")
                .Where(x => !x.EndsWith("symbols.nupkg"))
                .ForEach(x =>
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSource(GitRepository.HttpsUrl)
                    );
                });

        });

    Target PublishGithubRelease => _ => _
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch())
        .Executes<Task>(async () =>
        {
            Log.Information("Started creating release.");
            var releaseTag = $"v{GitVersion.MajorMinorPatch}";

            var githubOwner = GitRepository.GetGitHubOwner();
            var repositoryName = GitRepository.GetGitHubName();

            var nugetPackages = PackagesDirectory.GlobFiles("*.nupkg")
                .Select(x => x.ToString()).ToArray();
            
            Assert.NotEmpty(nugetPackages);

            await PublishRelease(conf => conf
                .SetArtifactPaths(nugetPackages)
                .SetCommitSha(GitVersion.Sha)
                .SetRepositoryName(repositoryName)
                .SetRepositoryOwner(githubOwner)
                .SetTag(releaseTag)
                .DisablePrerelease());
        });


    private Configure<DotNetPackSettings> PackSettings => _ => _
        .SetProject(Solution)
        .SetConfiguration(Configuration)
        .SetNoBuild(SucceededTargets.Contains(Compile))
        .SetOutputDirectory(PackagesDirectory)
        .When(GitRepository.HttpsUrl != null, (_) => _.SetRepositoryUrl(GitRepository.HttpsUrl));
}
