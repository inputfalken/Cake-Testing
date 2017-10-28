////////////////////////////////////////////////////////////////////////////////////////////////////
//                                           Arguments                                            //
////////////////////////////////////////////////////////////////////////////////////////////////////
var configuration = Argument<string>("configuration", "Release");
var distTarget = Argument<string>("dist", "dist");
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                          Directories                                           //
////////////////////////////////////////////////////////////////////////////////////////////////////
var solution = GetFiles("*.sln").First();
var distDirectory = Directory(distTarget);
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                            Projects                                            //
////////////////////////////////////////////////////////////////////////////////////////////////////
var projects = GetFiles("./**/*.csproj").Select(f => (
        File: f,
        Directory: f.GetDirectory(),
        Name: f.GetFilename().FullPath.Split(new string[] { f.GetExtension() }, StringSplitOptions.None)[0],
        Framework: XmlPeek(f,"Project/PropertyGroup/TargetFramework/text()"),
        Sdk: XmlPeek(f,"Project/@Sdk")
    )
).ToList();
var webProjects = projects.Where(x => x.Sdk == "Microsoft.NET.Sdk.Web").ToList();
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                            Methods                                             //
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
//                                             Tasks                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////
const string bin = "/bin";
Task("Clean Artifacts")
    .Does(() => {
        const string obj = "/obj";
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
    .Does(() => DotNetCoreRestore(solution.FullPath));


Task("Publish Projects")
    .IsDependentOn("Restore Projects")
    .Does(() => DotNetCorePublish( solution.FullPath, new DotNetCorePublishSettings { Configuration = configuration }));

Task("Test Projects")
    .IsDependentOn("Publish Projects")
    .Does(() => {
        DotNetCoreTest(
            "./Tests/Tests.csproj",
            new DotNetCoreTestSettings() {
                Configuration = configuration,
                NoBuild = true        
            }
        );
    });

Task("Zip Projects")
    .IsDependentOn("Test Projects")
    .Does(() => {
        CreateDirectory(distDirectory);
        foreach (var project in webProjects) {
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
//                                             Start                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////
RunTarget("Deploy Projects");
