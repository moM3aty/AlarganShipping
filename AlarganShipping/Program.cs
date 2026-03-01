using AlarganShipping.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة خدمات MVC
builder.Services.AddControllersWithViews();

// 2. إعداد قاعدة البيانات - تأكد من وجود Connection String في appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. إعداد نظام الأمان (Authentication)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// حل مشكلة BinaryReader: تأكد من استخدام builder.Build() بشكل صحيح
var app = builder.Build();

// 4. إعداد خط معالجة الطلبات
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. تفعيل الهوية والصلاحيات
app.UseAuthentication();
app.UseAuthorization();

// 6. المسارات الافتراضية
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();