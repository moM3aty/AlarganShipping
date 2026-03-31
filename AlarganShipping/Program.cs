using AlarganShipping.Models;
using AlarganShipping.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 1. إعداد الترجمة (عربي وانجليزي)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// 2. إعداد قاعدة البيانات
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. إعداد المصادقة (تسجيل الدخول)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddScoped<ICalculatorService, CalculatorService>();

var app = builder.Build();

// ========================================================================
// 4. الحل الجذري والسحري (تهيئة اللغات وضبط الفاصلة العشرية)
// ========================================================================

// أ. تجهيز الثقافة الإنجليزية (تستخدم النقطة تلقائياً)
var enCulture = new CultureInfo("en-US");

// ب. تجهيز الثقافة العربية، وإجبارها على استخدام النقطة (.) في الأرقام بدلاً من الفاصلة (,)
var arCulture = new CultureInfo("ar");
arCulture.NumberFormat.NumberDecimalSeparator = ".";
arCulture.NumberFormat.CurrencyDecimalSeparator = ".";

var supportedCultures = new[] { enCulture, arCulture };

// ج. إعداد خيارات الترجمة والـ Culture
var localizationOptions = new RequestLocalizationOptions
{
    // الواجهة الافتراضية عربي، لكن الأرقام والتواريخ الافتراضية أمريكي
    DefaultRequestCulture = new RequestCulture(culture: enCulture, uiCulture: arCulture),
    SupportedCultures = supportedCultures,     // لضبط الأرقام والتواريخ
    SupportedUICultures = supportedCultures,   // لضبط نصوص الواجهة والترجمة

    // منع المتصفح من فرض لغته الخاصة (تجاهل لغة المتصفح والاعتماد على الكوكيز أو الرابط)
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    }
};

app.UseRequestLocalization(localizationOptions);

// فرض الـ Culture على الـ Threads في الخلفية لضمان عمل الـ Model Binding بنجاح
CultureInfo.DefaultThreadCurrentCulture = enCulture;
CultureInfo.DefaultThreadCurrentUICulture = arCulture;

// ========================================================================

// 5. إعدادات بيئة العمل
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // للسماح بقراءة ملفات wwwroot

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 6. التوجيه الافتراضي
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7. تهيئة قاعدة البيانات بالبيانات الأولية عند تشغيل النظام
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        AlarganShipping.Data.DbInitializer.Initialize(context);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ تمت تهيئة قاعدة البيانات بنجاح!");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ حدث خطأ أثناء إدخال البيانات الأولية:");
        Console.WriteLine(ex.Message);
        if (ex.InnerException != null)
        {
            Console.WriteLine("تفاصيل الخطأ الداخلي: " + ex.InnerException.Message);
        }
        Console.ResetColor();
    }
}

app.Run();