var target = Argument("target", "Default");
var solution = GetFiles("*.sln").First();
var configuration = "Release";
var publishFolder = "artifacts";
var projects = GetFiles("./**/*.csproj").Select(f => (
        File: f,
        Directory: f.GetDirectory(),
        Name: f.GetFilename().FullPath.Split(new string[] { ".csproj" }, StringSplitOptions.None)[0],
        Framework: XmlPeek(f,"Project/PropertyGroup/TargetFramework/text()")
    )
).ToList();

Task("Clean Artifacts")
    .ContinueOnError()
    .Does(() => {
        const string bin = "bin";
        const string obj = "obj";
        
        var settings =  new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        };
        foreach(var projectDirectory in projects.Select(p => p.Directory.FullPath)) {
            Information($"Removing {bin} & {obj} Directories in  '{projectDirectory}'");
            DeleteDirectory($"{projectDirectory}/{bin}", settings);
            DeleteDirectory($"{projectDirectory}/{obj}", settings);
        }
    });

Task("Restore Projects")
    .IsDependentOn("Clean Artifacts")
    .Does(() => {
        foreach(var path in projects.Select(p => p.File.FullPath)) DotNetCoreRestore(path);
    });

Task("Build Projects")
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
    .IsDependentOn("Build Projects")
    .Does(() => {
        foreach(var project in projects) {
            Information(project);
            DotNetCorePublish(
                project.Directory.FullPath, 
                new DotNetCorePublishSettings {
                    OutputDirectory =  Directory(publishFolder) + Directory(project.Name),
                    Configuration = configuration
                }
            );
        }
    });

RunTarget("Publish Projects");