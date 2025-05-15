using WeihanLi.Common.Helpers;

namespace BalabalaSample;

public static class RenamingTool
{
    private static readonly Dictionary<string, string> ReplacePatterns = new()
    {
        { "Npgsql", "GaussDB" },
        { "npgsql", "gaussdb" }
    };
    
    public static async Task MainAsync()
    {
        var rootDir = @"C:\projects\source\huawei-cloud\gaussdb-dotnet";
        
        //
        // foreach (var files in GetFiles(rootDir).Reverse())
        // {
        //     foreach (var file in files)
        //     {
        //         var newFileName = file;
        //         
        //         // var content = await File.ReadAllTextAsync(file);
        //         // var newContent = content;
        //         //
        //         foreach (var (before, after) in replacePatterns)
        //         {
        //             //newContent = newContent.Replace(before, after);
        //             newFileName = newFileName.Replace(before, after);
        //         }
        //         //
        //         // if (newContent != content)
        //         // {
        //         //     await File.WriteAllTextAsync(file, newContent);
        //         //     Console.WriteLine($"file {file} content updated");
        //         // }
        //         
        //         if (newFileName != file)
        //         {
        //             File.Move(file, newFileName);
        //             Console.WriteLine($"file {file} renamed to {newFileName}");
        //         }
        //     }
        // }
        RenameFilesAndFolders(rootDir);
        Console.WriteLine();
    }
    
    static string Rename(string originalName)
    {
        var newName = originalName;
        
        foreach (var (before, after) in ReplacePatterns)
        {
            newName = newName.Replace(before, after);
        }

        return newName;
    }
    
    static void RenameFilesAndFolders(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            Console.WriteLine("Error: Directory does not exist.");
            return;
        }

        // Rename files
        foreach (var filePath in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileName(filePath);
                var newFileName = Rename(fileName);
                var newPath = Path.Combine(dir!, newFileName);

                if (filePath != newPath)
                {
                    File.Move(filePath, newPath);
                    Console.WriteLine($"Renamed file: {filePath} -> {newPath}");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteWithColor($"Failed to rename file: {filePath} - {ex.Message}", ConsoleColor.DarkRed);
            }
        }

        // Rename folders (reverse order to avoid path issues)
        // Rename folders from deepest to shallowest using depth (number of path separators)
        var allFolders = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

// Order by depth (count of directory separators), descending
        Array.Sort(allFolders, (a, b) =>
            b.Split(Path.DirectorySeparatorChar).Length.CompareTo(
                a.Split(Path.DirectorySeparatorChar).Length)
        );

        foreach (var folderPath in allFolders)
        {
            try
            {
                var parentDir = Path.GetDirectoryName(folderPath);
                var folderName = Path.GetFileName(folderPath);
                var newFolderName = Rename(folderName);
                var newPath = Path.Combine(parentDir, newFolderName);

                if (folderPath != newPath)
                {
                    Directory.Move(folderPath, newPath);
                    Console.WriteLine($"Renamed folder: {folderPath} -> {newPath}");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineWithColor($"Failed to rename folder: {folderPath} - {ex.Message}", ConsoleColor.DarkRed);
            }
        }
    }

    private static IEnumerable<string[]> GetFiles(string parent)
    {
        yield return Directory.GetFiles(parent);
        foreach (var dir in Directory.GetDirectories(parent))
        {
            if (dir.EndsWith(".git") || dir.EndsWith("bin") || dir.EndsWith("obj") || dir.EndsWith("artifacts"))
                continue;
            
            foreach (var files in GetFiles(dir))
            {
                yield return files;
            }
        }
    }
}
