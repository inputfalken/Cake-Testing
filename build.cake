var solution = GetFiles("*.sln").First();
var configuration = "Release";
var publishDirectory = Directory("artifacts");
var projects = GetFiles("./**/*.csproj").Select(f => (
        File: f,
        Directory: f.GetDirectory(),
        Name: f.GetFilename().FullPath.Split(new string[] { f.GetExtension() }, StringSplitOptions.None)[0],
        Framework: XmlPeek(f,"Project/PropertyGroup/TargetFramework/text()")
    )
).ToList();

public void RemoveDirectory(DirectoryPath path, DeleteDirectorySettings settings) {
    if (DirectoryExists(path)) {
        Information($"Deleting '{path.FullPath}'.");
        DeleteDirectory(path, settings);
    } else {
        Information($"Skipping deletion, could not find '{path.FullPath}'.");
    }
}

Task("Clean Artifacts")
    .Does(() => {
        const string bin = "/bin";
        const string obj = "/obj";
        var settings =  new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        };
        RemoveDirectory(publishDirectory, settings);
        foreach(var projectDirectory in projects.Select(p => p.Directory)) {
            RemoveDirectory(projectDirectory + Directory(bin), settings);
            RemoveDirectory(projectDirectory + Directory(obj), settings);
        }
    });

Task("Restore Projects")
    .IsDependentOn("Clean Artifacts")
    .Does(() => {
        foreach(var path in projects.Select(p => p.File.FullPath)) DotNetCoreRestore(path);
    });

Task("Build Solution")
    .IsDependentOn("Restore Projects")
    .Does(() => {
        DotNetCoreBuild(
            solution.FullPath,
            new DotNetCoreBuildSettings {
                Configuration = configuration
            }
        );
    });

// WIP not packing project as expected
Task("Publish Projects")
    .IsDependentOn("Build Solution")
    .Does(() => {
        foreach(var project in projects) {
            Information(project);
            DotNetCorePublish(
                project.Directory.FullPath, 
                new DotNetCorePublishSettings {
                    OutputDirectory =  publishDirectory + Directory(project.Name),
                    Configuration = configuration
                }
            );
        }
    });

RunTarget("Publish Projects");