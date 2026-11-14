using Dockyra.Domain.Entities;

namespace Dockyra.Domain.Rules;

/// <summary>
/// Enforces usability and performance bounds on nested folder hierarchies.
/// </summary>
public static class FolderHierarchyRules
{
    public const int MaxAllowedDepth = 3;
    public const int MaxItemsPerFolder = 100;

    public static bool CanCreateSubFolder(
        IReadOnlyDictionary<string, FolderItem> existingFolders,
        string? parentFolderId,
        out string? validationError)
    {
        if (string.IsNullOrWhiteSpace(parentFolderId))
        {
            validationError = null;
            return true;
        }

        int depth = 1;
        HashSet<string> visited = new(StringComparer.Ordinal);
        string? currentId = parentFolderId;

        while (!string.IsNullOrWhiteSpace(currentId))
        {
            if (!visited.Add(currentId))
            {
                validationError = "Cyclic folder hierarchy detected.";
                return false;
            }

            depth++;
            if (depth > MaxAllowedDepth)
            {
                validationError = $"Folder nesting cannot exceed {MaxAllowedDepth} levels.";
                return false;
            }

            if (!existingFolders.TryGetValue(currentId, out FolderItem? folder))
            {
                break;
            }

            currentId = folder.ParentFolderId;
        }

        validationError = null;
        return true;
    }
}

