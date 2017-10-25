var target = Argument("target", "Default");
var settings = GetFiles("./**/*.csproj").Select(f => (
        File: f,
        ProjectDirectory: f.GetDirectory(),
        ProjectName: f.GetFilename().FullPath.Split(new string[] { ".csproj" }, StringSplitOptions.None)[0],
        Framework: XmlPeek(f,"Project/PropertyGroup/TargetFramework/text()")
    )
).ToList();

Task("Restore Projects")
    .IsDependentOn("Clean Artifacts")
    .Does(() => {
        foreach(var path in settings.Select(s => s.File.FullPath)) DotNetCoreRestore(path);
    });

Task("Clean Artifacts")
    .ContinueOnError()
    .Does(() => {
        const string bin = "bin";
        const string obj = "obj";
        
        var delSettings =  new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        };
        foreach(var projectDirectory in settings.Select(s => s.ProjectDirectory.FullPath)) {
            Information($"Removing {bin} & {obj} Directories in  '{projectDirectory}'");
            DeleteDirectory($"{projectDirectory}/{bin}", delSettings);
            DeleteDirectory($"{projectDirectory}/{obj}", delSettings);
        }
    });

Task("Build Projects")
    .IsDependentOn("Restore Projects")
    .Does(() => {
    var set = new DotNetCoreBuildSettings {
        Configuration = "Release",
    };
    DotNetCoreBuild(GetFiles("*.sln").First().FullPath, set);
    });

// WIP not packing project as expected
Task("Package Projects")
    .IsDependentOn("Build Projects")
    .Does(() => {
        foreach(var setting in settings.Where(s => s.Framework == "netcoreapp2.0")) {
            Information($"Packing {setting.ProjectName}");
            var packSettings = new DotNetCorePackSettings {
                OutputDirectory = setting.ProjectDirectory + Directory("bin/Release"),
                NoBuild = true
            };
            DotNetCorePack(setting.File.FullPath, packSettings);
        }
    });

RunTarget("Package Projects");