////////////////////////////////////////////////////////////////////////////////////////////////////
//                                           Arguments
////////////////////////////////////////////////////////////////////////////////////////////////////
var configuration = Argument("configuration", "Release");
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                          Directories
////////////////////////////////////////////////////////////////////////////////////////////////////
var solution = GetFiles("*.sln").First();
var publishDirectory = Directory("artifacts");
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                            Projects
////////////////////////////////////////////////////////////////////////////////////////////////////
var projects = GetFiles("./**/*.csproj").Select(f => (
        File: f,
        Directory: f.GetDirectory(),
        Name: f.GetFilename().FullPath.Split(new string[] { f.GetExtension() }, StringSplitOptions.None)[0],
        Framework: XmlPeek(f,"Project/PropertyGroup/TargetFramework/text()")
    )
).ToList();
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                            Methods
////////////////////////////////////////////////////////////////////////////////////////////////////
public void RemoveDirectory(DirectoryPath path, DeleteDirectorySettings settings) {
    if (DirectoryExists(path)) {
        Information($"Deleting '{path.FullPath}'.");
        DeleteDirectory(path, settings);
    } else {
        Information($"Skipping deletion, could not find '{path.FullPath}'.");
    }
}
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                             Tasks
////////////////////////////////////////////////////////////////////////////////////////////////////
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

Task("Zip Projects")
    .IsDependentOn("Publish Projects")
    .Does(() => {
        var publishedProjects = GetDirectories($"{publishDirectory}/*") ;
        foreach (var publishedProject in publishedProjects) {
           var zipResult = $"{publishedProject.FullPath}.zip";
           Information($"Zipping '{publishedProject.FullPath}' -> '{zipResult}'");
           Zip(publishedProject.FullPath, zipResult);
        }
    });


Task("Deploy Projects")
    .IsDependentOn("Zip Projects")
    .Does(() => {
        var zippedFiles = GetFiles($"{publishDirectory}/*.zip");
        foreach (var zip in zippedFiles) {
            Information($"Deploying '{zip.FullPath}' -> Somwhere...");
        }
    });
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                             Start
////////////////////////////////////////////////////////////////////////////////////////////////////
RunTarget("Deploy Projects");
