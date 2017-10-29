////////////////////////////////////////////////////////////////////////////////////////////////////
//                                          Load Scripts                                          //
////////////////////////////////////////////////////////////////////////////////////////////////////
#load "./build/project.cake"
//NOTE Requires './build/project.cake'
#load "./build/helperMethods.cake"
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
var projects = GetProjects("./**/*.csproj");
var applicationsProjects = GetProjects("./src/apps/**/*.csproj");
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
        var testProjects = GetFiles("./Tests/**/*.csproj");
        foreach(var project in testProjects) {
            DotNetCoreTest(
                project.FullPath,
                new DotNetCoreTestSettings() {
                    Configuration = configuration,
                    NoBuild = true
                }
            );
        }
    });

Task("Zip Projects")
    .IsDependentOn("Test Projects")
    .Does(() => {
        CreateDirectory(distDirectory);
        foreach (var project in applicationsProjects) {
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
