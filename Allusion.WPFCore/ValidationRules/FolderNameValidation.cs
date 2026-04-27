using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace Allusion.WPFCore.ValidationRules;

public static class FolderNameValidation
{
    public static ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var illegalPathChars = Path.GetInvalidPathChars();
        var illegalFilenameChars = Path.GetInvalidFileNameChars();

        if (value is not string folderName)
            return new ValidationResult(false, "folder name is required");

        foreach (var c in folderName)
        {
            if (illegalFilenameChars.Contains(c))
                return new ValidationResult(false, "illegal character " + c);
            if (illegalPathChars.Contains(c))
                return new ValidationResult(false, "illegal character " + c);
        }

        return ValidationResult.ValidResult;
    }
}
