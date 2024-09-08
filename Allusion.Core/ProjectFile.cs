using System.Text.Json;

namespace Allusion.Core;

public class ProjectFile
{
    public string Name { get; set; }
    public ImageItem[] Images { get; set; }

    public ProjectFile(string name)
    {
        Name = name;
    }

    public static ProjectFile? Read(string projectFileFullPath)
    {
        if (!File.Exists(projectFileFullPath)) return null;

        var projectJson = File.ReadAllText(projectFileFullPath);
        
        var project = JsonSerializer.Deserialize<ProjectFile>(projectJson);

        return project;
    }

    public static void Save(ProjectFile project, string path)
    {
        var fileName = Path.Combine(path, project.Name) + ".json";
    
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);
    
        File.WriteAllText(fileName, JsonSerializer.Serialize(project));
    }
}