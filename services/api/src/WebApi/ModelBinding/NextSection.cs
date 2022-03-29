using System.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace WebApi.ModelBinding;

public class NextSection
{
    private readonly MultipartSection _section;
    private readonly List<string> _validationErrors;

    public NextSection(MultipartSection section, List<string> validationErrors)
    {
        Debug.Assert(!section.BaseStreamOffset.HasValue);
        _section = section;
        _validationErrors = validationErrors;
    }

    public MyFormMultipartSection? AsFormSection()
    {
        if (_section.IsFormData(out ContentDispositionHeaderValue? contentDisposition))
        {
            return new MyFormMultipartSection(_section, contentDisposition!, _validationErrors);
        }

        _validationErrors.Add($"Multipart section '{contentDisposition?.Name}' is not form data");
        return null;
    }

    public MyFormMultipartSection? AsFormSection(string sectionName)
    {
        MyFormMultipartSection? section = AsFormSection();

        if (section is null)
        {
            return null;
        }

        if (section.Name == sectionName)
        {
            return section;
        }

        _validationErrors.Add($"Multipart section '{section.Name}' does not match '{sectionName}'");
        return null;
    }

    public MyFileMultipartSection? AsFileSection()
    {
        if (_section.IsFile(out ContentDispositionHeaderValue? contentDisposition))
        {
            return new MyFileMultipartSection(_section, contentDisposition!, _validationErrors);
        }

        _validationErrors.Add($"Multipart section '{contentDisposition?.Name}' is not a file");
        return null;
    }

    public MyFileMultipartSection? AsFileSection(string sectionName)
    {
        MyFileMultipartSection? section = AsFileSection();

        if (section is null)
        {
            return null;
        }

        if (section.Name == sectionName)
        {
            return section;
        }

        _validationErrors.Add($"Multipart section '{section.Name}' does not match '{sectionName}'");
        return null;
    }
}
