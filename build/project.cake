public class Project {
    public FilePath File { get;  }
    public DirectoryPath Directory { get; }
    public string Name { get; }
    public string Framework { get;  }
    public string Sdk { get;  }
    public Project(FilePath filePath, DirectoryPath directoryPath, string name, string framework, string sdk) {
        File = filePath;
        Directory = directoryPath;
        Name = name;
        Framework = framework;
        Sdk = sdk;
    }
}
