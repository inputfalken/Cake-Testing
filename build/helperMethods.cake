#load "./project.cake"
public IReadOnlyList<Project> GetProjects(string path) {
    return GetFiles(path)
            .Select(f => new Project(
                    f,
                    f.GetDirectory(),
                    f.GetFilename().FullPath.Split(new string[] { f.GetExtension() }, StringSplitOptions.None)[0],
                    XmlPeek(f,"Project/PropertyGroup/TargetFramework/text()"),
                    XmlPeek(f,"Project/@Sdk")
                )
            )
            .ToList();
}
public void RemoveDirectory(DirectoryPath path, DeleteDirectorySettings settings) {
    if (DirectoryExists(path)) {
        Information($"Deleting '{path.FullPath}'.");
        DeleteDirectory(path, settings);
    } else {
        Information($"Skipping deletion, could not find '{path.FullPath}'.");
    }
}
