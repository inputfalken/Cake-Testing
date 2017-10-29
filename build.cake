////////////////////////////////////////////////////////////////////////////////////////////////////
//                                          Load Scripts                                          //
////////////////////////////////////////////////////////////////////////////////////////////////////
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
var appDirectory = Directory("./src/apps/");
var testDirectory = Directory("./Tests/");
var libDirectory = Directory("./src/libs/");
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                            Projects                                            //
////////////////////////////////////////////////////////////////////////////////////////////////////
var projects = GetProjects("./**/*.csproj");
var applicationsProjects = projects.Where(p => p.File.FullPath.Contains(appDirectory.Path.FullPath)).ToList();
var testProjects = projects.Where(p => p.File.FullPath.Contains(testDirectory.Path.FullPath)).ToList();
var libProjects = projects.Where(p => p.File.FullPath.Contains(libDirectory.Path.FullPath)).ToList();
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                             Tasks                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////
const string bin = "/bin";

Task("Project Information")
    .Does(() => {
        Information($"{ProjectOrProjects(applicationsProjects)} in directory '{appDirectory.Path.FullPath}'.");
        Information($"{ProjectOrProjects(testProjects)} in directory '{testDirectory.Path.FullPath}'.");
        Information($"{ProjectOrProjects(libProjects)} in directory '{libDirectory.Path.FullPath}'.");
    });

Task("Clean Build")
    .IsDependentOn("Project Information")
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
    .IsDependentOn("Clean Build")
    .Does(() => DotNetCoreRestore(solution.FullPath));

Task("Build & Publish Projects")
    .IsDependentOn("Restore Projects")
    .Does(() => DotNetCorePublish( solution.FullPath, new DotNetCorePublishSettings { Configuration = configuration }));

Task("Test Projects")
    .IsDependentOn("Build & Publish Projects")
    .Does(() => {
        foreach(var project in testProjects) {
            DotNetCoreTest(
                project.File.FullPath,
                new DotNetCoreTestSettings() {
                    Configuration = configuration,
                    NoBuild = true
                }
            );
        }
    });

Task("Zip Applications")
    .IsDependentOn("Test Projects")
    .Does(() => {
        CreateDirectory(distDirectory);
        foreach (var project in applicationsProjects) {
           var zipResult = $"{distDirectory}/{project.Name}.zip";
           var path = project.Directory + Directory($"{bin}/{configuration}/{project.Framework}/publish/");
           Information($"Zipping '{path}' -> '{zipResult}'");
           Zip(path, zipResult);
        }
    });


Task("Deploy Applications")
    .IsDependentOn("Zip Applications")
    .Does(() => {
        var zippedFiles = GetFiles($"{distDirectory}/*.zip");
        foreach (var zip in zippedFiles) {
            Information($"Deploying '{zip.FullPath}' -> Somwhere...");
        }
    });
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                             Start                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////
RunTarget("Deploy Applications");
