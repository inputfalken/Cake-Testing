////////////////////////////////////////////////////////////////////////////////////////////////////
//                                           Arguments
////////////////////////////////////////////////////////////////////////////////////////////////////
var configuration = Argument<string>("configuration", "Release");
var distTarget = Argument<string>("dist", "dist");
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                          Directories
////////////////////////////////////////////////////////////////////////////////////////////////////
var solution = GetFiles("*.sln").First();
var distDirectory = Directory(distTarget);
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
const string bin = "/bin";
const string obj = "/obj";
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                             Tasks
////////////////////////////////////////////////////////////////////////////////////////////////////
Task("Clean Artifacts")
    .Does(() => {
        var settings =  new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        };
        RemoveDirectory(distDirectory, settings);
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

Task("Publish Projects")
    .IsDependentOn("Restore Projects")
    .Does(() => {
        DotNetCorePublish(
            solution.FullPath, 
            new DotNetCorePublishSettings {
                Configuration = configuration
            }
        );
    });

Task("Zip Projects")
    .IsDependentOn("Publish Projects")
    .Does(() => {
        CreateDirectory("artifacts");
        foreach (var project in projects) {
           var zipResult = $"{distDirectory}/{project.Name}.zip";
           var path = project.Directory + Directory($"{bin}/{configuration}");
           Information($"Zipping '{path}' -> '{zipResult}'");
           Zip(path, zipResult);
        }
    });


Task("Deploy Projects")
    .IsDependentOn("Zip Projects")
    .Does(() => {
        var zippedFiles = GetFiles($"{distDirectory}/*.zip");
        foreach (var zip in zippedFiles) {
            Information($"Deploying '{zip.FullPath}' -> Somwhere...");
        }
    });
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                             Start
////////////////////////////////////////////////////////////////////////////////////////////////////
RunTarget("Deploy Projects");
