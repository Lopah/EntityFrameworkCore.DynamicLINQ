using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;


public partial class Build
{
    AbsolutePath TestResultDirectory => ArtifactsDirectory / "test-results";

    IEnumerable<Project> TestProjects => Partition.GetCurrent(Solution.GetProjects("*.Tests"));

    Target Test => _ => _
        .DependsOn(Compile)
        .Produces(TestResultDirectory / "*.trx")
        .Produces(TestResultDirectory / "*.xml")
        .Executes(() =>
        {
            try
            {
                DotNetTest(_ => _
                        .Apply(TestSettings)
                        .CombineWith(TestProjects, (_, v) => _
                            .Apply(TestProjectSettings, v)
                            .Apply(TestProjectSettings, v)),
                    completeOnFailure: true);
            }
            finally
            {
                ReportTestCount();
            }
        });

    Configure<DotNetTestSettings> TestSettings => _ => _
        .SetConfiguration(Configuration)
        .SetNoBuild(SucceededTargets.Contains(Compile))
        .ResetVerbosity()
        .SetResultsDirectory(TestResultDirectory)
        .When(IsServerBuild, _ => _
            .EnableCollectCoverage()
            .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
            .SetExcludeByFile("*.Generated.cs")
            .When(IsServerBuild, _ => _
                .EnableUseSourceLink()));

    Configure<DotNetTestSettings, Project> TestProjectSettings => (_, v) => _
        .SetProjectFile(v)
        .SetLoggers($"trx;LogFileName={v.Name}.trx")
        .When(IsServerBuild, _ => _
            .SetCoverletOutput(TestResultDirectory / $"{v.Name}.xml"));

    void ReportTestCount()
    {
        IEnumerable<string> GetOutcomes(AbsolutePath file)
        {
            return XmlTasks.XmlPeek(
                file,
                "/xn:TestRun/xn:Results/xn:UnitTestResult/@outcome",
                ("xn", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"));
        }

        var resultFiles = TestResultDirectory.GlobFiles("*.trx");
        var outcomes = resultFiles.SelectMany(GetOutcomes).ToList();
        var passedTests = outcomes.Count(x => x == "Passed");
        var failedTests = outcomes.Count(x => x == "Failed");
        var skippedTests = outcomes.Count(x => x == "NotExecuted");

        ReportSummary(_ => _
            .When(failedTests > 0, _ => _
                .AddPair("Failed", failedTests.ToString()))
            .AddPair("Passed", passedTests.ToString())
            .When(skippedTests > 0, _ => _
                .AddPair("Skipped", skippedTests.ToString())));
    }
}
