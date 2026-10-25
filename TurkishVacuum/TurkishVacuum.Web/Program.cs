using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;
using TurkishVacuum.Web.Models;
using TurkishVacuum.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Türkçe karakterler HTML çıktısında &#x131; gibi kodlanmadan, okunur şekilde yazılsın.
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// Instagram: en popüler gönderiler arka planda düzenli olarak çekilir (bkz. README).
builder.Services.Configure<InstagramOptions>(builder.Configuration.GetSection(InstagramOptions.SectionName));
builder.Services.AddSingleton<InstagramFeed>();
builder.Services.AddHostedService<InstagramSyncService>();
builder.Services
    .AddHttpClient(InstagramSyncService.HttpClientName, (services, client) =>
    {
        var options = services.GetRequiredService<IOptions<InstagramOptions>>().Value;
        client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(15);
    })
    // Varsayılan HttpClient günlüğü istek adresini yazar; adreste erişim anahtarı bulunduğu için kapatılır.
    .RemoveAllLoggers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/hata");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 404 vb. durumlarda boş tarayıcı hatası yerine site içi hata sayfası gösterilir.
app.UseStatusCodePagesWithReExecute("/hata/{0}");

// Eski adresler arama motorlarındaki değerini kaybetmesin diye kalıcı (301) yönlendirilir.
app.UseRewriter(new RewriteOptions()
    .AddRedirect("(?i)^home(/index)?/?$", "/", StatusCodes.Status301MovedPermanently)
    .AddRedirect("(?i)^home/yedek_parca/?$", "/yedek-parca", StatusCodes.Status301MovedPermanently)
    .AddRedirect("(?i)^home/destek/?$", "/destek", StatusCodes.Status301MovedPermanently)
    .AddRedirect("(?i)^home/hakkimizda/?$", "/hakkimizda", StatusCodes.Status301MovedPermanently)
    .AddRedirect("(?i)^home/iletisim/?$", "/iletisim", StatusCodes.Status301MovedPermanently));

// Dil, tarayıcı ayarına göre değil adrese göre belirlenir: /en/... İngilizce, diğerleri Türkçe.
// Böylece arama motorları her dili ayrı ve kalıcı bir adreste görür.
var supportedCultures = new[] { new CultureInfo("tr-TR"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    // Varsayılan sağlayıcılar (Accept-Language, çerez, sorgu dizesi) bilerek listeye alınmaz.
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CustomRequestCultureProvider(context =>
        {
            // Hata sayfası yeniden çalıştırıldığında asıl istenen adresin dili kullanılır.
            var path = context.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath ?? context.Request.Path;
            var culture = SiteRoutes.IsEnglishPath(path) ? "en-US" : "tr-TR";
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
        })
    }
});

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    // CSS/JS dosyaları asp-append-version ile sürümlendiği için tarayıcı önbelleği güvenle uzatılabilir.
    OnPrepareResponse = context =>
        context.Context.Response.Headers.CacheControl = "public,max-age=604800"
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
