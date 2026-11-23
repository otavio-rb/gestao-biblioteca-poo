using System.Globalization;
using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Repositories.Dapper;
using BibliotecaDigital.Web.Repositories.Interfaces;

var culturaBrasil = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaBrasil;
CultureInfo.DefaultThreadCurrentUICulture = culturaBrasil;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(opcoes =>
{
    opcoes.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
    opcoes.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddSingleton<IFabricaConexao, FabricaConexaoSqlite>();
builder.Services.AddScoped<InicializadorBanco>();

builder.Services.AddScoped<IEmprestimoRepository, EmprestimoRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<ICirculacaoRepository, CirculacaoRepository>();

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    escopo.ServiceProvider.GetRequiredService<InicializadorBanco>().GarantirBancoCriado();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culturaBrasil),
    SupportedCultures = new List<CultureInfo> { culturaBrasil },
    SupportedUICultures = new List<CultureInfo> { culturaBrasil }
});

app.UseExceptionHandler("/Home/Erro");
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
