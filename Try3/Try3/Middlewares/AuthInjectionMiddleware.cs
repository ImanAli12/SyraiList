using Microsoft.AspNetCore.Identity;
using RealEstateWebApp.Models;
using System.Text;

namespace RealEstateWebApp.Middlewares
{
    public class AuthInjectionMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthInjectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (IsStaticFile(path))
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            try
            {
                await _next(context);
            }
            catch
            {
                context.Response.Body = originalBody;
                throw;
            }

            memStream.Position = 0;
            var bodyText = await new StreamReader(memStream, Encoding.UTF8).ReadToEndAsync();
            context.Response.Body = originalBody;

            var contentType = context.Response.ContentType ?? "";

            if (contentType.Contains("text/html")
                && bodyText.Contains("</body>")
                && !bodyText.Contains("syrelisAuthInjected"))
            {
                var signInManager = context.RequestServices
                    .GetRequiredService<SignInManager<ApplicationUser>>();

                var isAuth = signInManager.IsSignedIn(context.User);
                var username = context.User.Identity?.Name ?? "مستخدم";

                var injection = BuildInjection(isAuth, username);
                bodyText = bodyText.Replace("</body>", injection + "\n</body>");
            }

            var bytes = Encoding.UTF8.GetBytes(bodyText);
            context.Response.ContentLength = bytes.Length;
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        private static bool IsStaticFile(string path)
        {
            var exts = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif",
                               ".svg", ".ico", ".woff", ".woff2", ".ttf",
                               ".map", ".webp", ".mp4", ".pdf", ".json" };
            return exts.Any(e => path.EndsWith(e));
        }

        private static string BuildInjection(bool isAuth, string username)
        {
            var safeUser = System.Net.WebUtility.HtmlEncode(username);
            var authFlag = isAuth ? "true" : "false";
            var sb = new StringBuilder();

            sb.Append("<!-- syrelisAuthInjected -->\n");

            // ✅ متغيرات من السيرفر (مصدر الحقيقة)
            sb.Append("<script>window.syrelisIsAuthenticated=");
            sb.Append(authFlag);
            sb.Append(";window.syrelisUserName='");
            sb.Append(safeUser);
            sb.Append("';</script>\n");

            // ✅ CSS
            sb.Append("<link rel='stylesheet' href='/css/syrelis-auth.css' />\n");

            // ✅ HTML (المودالات)
            sb.Append(GetModalHtml());

            // ✅ JS
            sb.Append("<script src='/js/syrelis-auth.js'></script>\n");

            return sb.ToString();
        }

        private static string GetModalHtml()
        {
            // ⚠️ كل الاقتباسات هنا single quotes فقط — لا تضع " أبدًا
            return @"
<div id='authModal' role='dialog' aria-hidden='true'>
    <div class='syrelis-modal-card'>
        <button type='button' class='syrelis-close' id='syrelisCloseAuth'>&times;</button>
        <h2 class='syrelis-title' id='syrelisModalTitle'>SYRELIS</h2>
        <p class='syrelis-subtitle' id='syrelisModalSubtitle'>سجل دخولك للوصول إلى حسابك</p>
        <div id='syrelisLoginPanel'>
            <input type='email' id='syrelisLoginEmail' class='syrelis-input' placeholder='البريد الإلكتروني' />
            <input type='password' id='syrelisLoginPassword' class='syrelis-input' placeholder='كلمة المرور' />
            <button type='button' class='syrelis-btn' id='syrelisDoLogin'>تسجيل الدخول</button>
            <div class='syrelis-toggle'>ليس لديك حساب؟ <a id='syrelisSwitchToSignup'>إنشاء حساب جديد</a></div>
        </div>
        <div id='syrelisSignupPanel' style='display:none'>
            <input type='text' id='syrelisSignupName' class='syrelis-input' placeholder='الاسم الكامل' />
            <input type='tel' id='syrelisSignupPhone' class='syrelis-input' placeholder='رقم الهاتف 09xxxxxxxx' />
            <input type='email' id='syrelisSignupEmail' class='syrelis-input' placeholder='البريد الإلكتروني' />
            <input type='password' id='syrelisSignupPassword' class='syrelis-input' placeholder='كلمة المرور 8 خانات' />
            <input type='password' id='syrelisSignupConfirm' class='syrelis-input' placeholder='تأكيد كلمة المرور' />
            <button type='button' class='syrelis-btn' id='syrelisDoSignup'>إنشاء حساب</button>
            <div class='syrelis-toggle'>لديك حساب؟ <a id='syrelisSwitchToLogin'>تسجيل الدخول</a></div>
        </div>
    </div>
</div>

<div id='loginRequiredModal' role='dialog' aria-hidden='true'>
    <div class='syrelis-modal-card'>
        <button type='button' class='syrelis-close' id='syrelisCloseRequired'>&times;</button>
        <div class='syrelis-icon'>&#128274;</div>
        <h2 class='syrelis-title'>يجب تسجيل الدخول أولاً</h2>
        <p class='syrelis-text' id='syrelisRequiredText'>للوصول إلى هذه الصفحة، يجب أن تكون مسجلاً دخولك.</p>
        <div class='syrelis-buttons'>
            <button type='button' class='syrelis-btn-cancel' id='syrelisCancelRequired'>إلغاء</button>
            <button type='button' class='syrelis-btn-login' id='syrelisGoToLogin'>سجل دخولك الآن</button>
        </div>
    </div>
</div>
";
        }
    }
}