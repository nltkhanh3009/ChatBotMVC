using ChatBotGemini;
using ChatBotGemini.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection("Gemini")
);
// Đăng ký IHttpClientFactory
builder.Services.AddHttpClient<GeminiService>();

// --- Cấu hình Session ---
// 1. Thêm dịch vụ Session
builder.Services.AddDistributedMemoryCache(); // Lưu session trong bộ nhớ (phù hợp cho dev/ứng dụng nhỏ)
                                              // Cân nhắc dùng SQL Server hoặc Redis cho production
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian session hết hạn nếu không hoạt động
    options.Cookie.HttpOnly = true; // Cookie chỉ truy cập được bởi server
    options.Cookie.IsEssential = true; // Cần thiết cho GDPR compliance
});
// -------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// --- Kích hoạt Session Middleware ---
// Quan trọng: Phải đặt UseSession() trước UseEndpoints() hoặc MapControllerRoute()
app.UseSession();
// -----------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map API controllers (nếu bạn dùng [ApiController] attribute)
app.MapControllers(); // Đảm bảo có dòng này để map API controller

app.Run();
