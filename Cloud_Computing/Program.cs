using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services;



var builder = WebApplication.CreateBuilder(args);

// Replace your existing awsOptions block with this:
var awsSection = builder.Configuration.GetSection("AWS");
var awsOptions = builder.Configuration.GetAWSOptions();

awsOptions.Credentials = new Amazon.Runtime.SessionAWSCredentials(
    awsSection["AccessKey"],
    awsSection["SecretKey"],
    awsSection["SessionToken"]
);
awsOptions.Region = Amazon.RegionEndpoint.USEast1; // Ensure this is us-east-1

builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();

// 1. 添加控制器與視圖服務
builder.Services.AddControllersWithViews();

// 2. 配置 MySQL 資料庫連接 (HeidiSQL)
// 這裡假設你的連接字串在 appsettings.json 中的名稱為 "DefaultConnection"
builder.Services.AddDbContext<DB>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// 3. 【關鍵註冊】註冊郵件服務 (IEmailService)
// 這解決了你截圖中出現的 InvalidOperationException 錯誤
builder.Services.AddScoped<IEmailService, EmailService>();

// 4. 添加 Session 服務 (用於登錄狀態管理)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session 30分鐘過期
    options.Cookie.HttpOnly = true;                // 增強安全性
    options.Cookie.IsEssential = true;             // 必要 Cookie
});

// 5. 如果你有用到 HttpContext 訪問 Session，建議添加此服務
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 配置 HTTP 請求管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // 支援 CSS, JS, 圖片等靜態文件

app.UseRouting();

// 6. 【關鍵順序】啟用 Session 中間件
// 必須放在 UseRouting 之後，UseAuthorization 之前
app.UseSession();

app.UseAuthorization();

// 7. 配置默認路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();