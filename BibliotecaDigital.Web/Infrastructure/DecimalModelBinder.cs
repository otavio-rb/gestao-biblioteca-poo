using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliotecaDigital.Web.Infrastructure;

public sealed class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var resultado = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (resultado == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, resultado);

        var texto = resultado.FirstValue;

        if (string.IsNullOrWhiteSpace(texto))
        {
            if (Nullable.GetUnderlyingType(bindingContext.ModelType) is not null)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }

            return Task.CompletedTask;
        }

        const NumberStyles estilos = NumberStyles.Float | NumberStyles.AllowThousands;

        if (decimal.TryParse(texto, estilos, CultureInfo.InvariantCulture, out var valor)
            || decimal.TryParse(texto, estilos, CultureInfo.CurrentCulture, out valor))
        {
            bindingContext.Result = ModelBindingResult.Success(valor);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Informe um valor numerico valido.");
        return Task.CompletedTask;
    }
}
