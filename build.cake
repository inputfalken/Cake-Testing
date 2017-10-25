var target = Argument("target", "Default");
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
    var settings = new DotNetCoreBuildSettings {
        Configuration = "Release",
    };
    DotNetCoreBuild(GetFiles("*.sln").First().FullPath, settings);
    });

// WIP not packing project as expected
Task("Package Projects")
    .IsDependentOn("Build Projects")
    .Does(() => {
        foreach(var project in projects.Where(p => p.Framework == "netcoreapp2.0")) {
            Information($"Packing {project.Name}");
            var settings = new DotNetCorePackSettings {
                OutputDirectory = project.Directory + Directory("bin/Release"),
                NoBuild = true
            };
            DotNetCorePack(project.File.FullPath, settings);
        }
    });

RunTarget("Package Projects");