using Microsoft.AspNetCore.Http;

namespace aspnet.shared.middleware.static_compressed_file;

public class StaticCompressedFileOptions
{
    private PathString _requestPath = PathString.Empty;

    public PathString RequestPath
    {
        get => _requestPath;
        set
        {
            if (value.HasValue && value.Value!.EndsWith('/'))
            {
                throw new ArgumentException("Request path must not end in a forward slash");
            }

            _requestPath = value;
        }
    }
}
