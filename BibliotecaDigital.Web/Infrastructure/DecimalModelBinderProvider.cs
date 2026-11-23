using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliotecaDigital.Web.Infrastructure;

public sealed class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tipo = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;

        return tipo == typeof(decimal) ? new DecimalModelBinder() : null;
    }
}
