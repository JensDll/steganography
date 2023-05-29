using FluentValidation;

namespace aspnet.shared.middleware.static_compressed_file;

public class StaticCompressedFileOptionsValidator : AbstractValidator<StaticCompressedFileOptions>
{
    public StaticCompressedFileOptionsValidator()
    {
        RuleFor(options => options.RequestPath).Must(path => !path.Value!.EndsWith('/'))
            .WithMessage("Must not end with a trailing forward slash ('/')")
            .When(path => path.RequestPath.HasValue);
    }
}
