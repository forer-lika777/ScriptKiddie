using Script_Kiddie.Models;
using Script_Kiddie.Resources.Localization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Script_Kiddie.Interfaces;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Script_Kiddie.Services;

public class UIALoginService : ILoginService
{
    private readonly HttpClient httpClient;
    private readonly HttpClientHandler httpClientHandler;

    private readonly ILogger<UIALoginService> logger;

    private const string BASE_URL = "https://jxfw.gdut.edu.cn";
    private const string UIA_LOGIN_URL = "https://authserver.gdut.edu.cn/authserver/login?service=https%3A%2F%2Fjxfw.gdut.edu.cn%2Fnew%2FssoLogin";

    private const string MAIN_PAGE_URL = BASE_URL + "/login!welcome.action";
    private const string LOG_OUT_URL = BASE_URL + "/new/logout";

    private const string GET_ACCOUNT_INFO_REFERRER = BASE_URL + "/xjkpxx!xjkpList.action";
    private const string GET_ACCOUNT_INFO_URL = BASE_URL + "/xjkpxx!xjkpxx.action";

    private const string AES_CHARS = "ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678";
    private readonly Random random;

    public UIALoginService(HttpClient httpClient, HttpClientHandler httpClientHandler, ILogger<UIALoginService> logger)
    {
        this.httpClient = httpClient;
        this.httpClientHandler = httpClientHandler;
        this.logger = logger;

        random = new Random();
    }

    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        try
        {
            logger.LogDebug(AccountServiceStr.BeginLoggingIn);

            if (httpClientHandler.CookieContainer.Count == 0 && loginOption.LoadCookie)
            {
                if (loginOption.CookieLoadMode == CookieLoadMode.CookieString)
                {
                    var cookies = JsonSerializer.Deserialize(loginOption.CookieContent, CookieJsonContext.Default.ListCookieItem);
                    if (cookies != null && cookies.Count > 0)
                    {
                        // 设置到 HttpClientHandler
                        foreach (var cookie in cookies)
                        {
                            var domain = cookie.Domain ?? BASE_URL;
                            var path = cookie.Path ?? "/";

                            // 用 Cookie 自己的 Domain 构造 Uri，而不是用 MAIN_PAGE_URL
                            var uri = new Uri($"https://{domain.TrimStart('.')}{path}");

                            var c = new Cookie(cookie.Name, cookie.Value)
                            {
                                Domain = domain,
                                Path = path
                            };
                            httpClientHandler.CookieContainer.Add(uri, c);
                        }
                    }
                }
                else if (loginOption.CookieLoadMode == CookieLoadMode.Cookie)
                {
                    if (loginOption.Cookie == null)
                    {
                        throw new NullReferenceException("Cookie load mode is cookie item, but cookie in login option is null.");
                    }
                    var uri = new Uri(BASE_URL);
                    httpClientHandler.CookieContainer.Add(uri, loginOption.Cookie);
                }
            }

            var response = await httpClient.GetAsync(BASE_URL);
            string? responseurl = response.RequestMessage?.RequestUri?.ToString() ?? string.Empty;

            if (responseurl == MAIN_PAGE_URL)
            {
                return await BuildLoginResult(loginOption, httpClient, httpClientHandler, response.StatusCode);
            }
            else if (!responseurl.Contains(UIA_LOGIN_URL))
            {
                throw new Exception(AccountServiceStr.PageRedirectedToOtherUrl);
            }

            if (loginOption.LoadCookie)
            {
                logger.LogDebug(AccountServiceStr.UIA_LoadCookieLoginFailedWarning);
            }

            if (string.IsNullOrEmpty(loginOption.UserName) || string.IsNullOrEmpty(loginOption.Password))
            {
                throw new Exception("Username or password is empty. Login failed.");
            }

            string html = await response.Content.ReadAsStringAsync();

            var content = BuildLoginData(html, loginOption);

            logger.LogDebug(AccountServiceStr.SendPost);

            response = await httpClient.PostAsync(UIA_LOGIN_URL, content);

            if (response.RequestMessage?.RequestUri?.ToString() == MAIN_PAGE_URL)
            {
                logger.LogDebug(AccountServiceStr.LoginSuccess);
                return await BuildLoginResult(loginOption, httpClient, httpClientHandler, response.StatusCode);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                logger.LogDebug(AccountServiceStr.UIA_WrongUsernameOrPasswordMessage);
                if (loginOption.UserName.Length == 10)
                {
                    throw new Exception(AccountServiceStr.UIA_UsernameOrPasswordIncorrect);
                }
                else
                {
                    throw new Exception(AccountServiceStr.UIA_UsernameNotValidError);
                }
            }

            return new LoginResult
            {
                Success = false,
                Message = AccountServiceStr.LoginFailedForUnknownReason,
                StatusCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new LoginResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    // ========== 序列化 Cookie 为 JSON 字符串 ==========
    private async Task<string> SerializeCookiesToJson(HttpClientHandler handler)
    {
        var cookies = ExtractCookiesFromContainer(handler);

        var json = JsonSerializer.Serialize(
            cookies,
            CookieJsonContext.Default.ListCookieItem
        );

        return json;
    }

    // ========== 从 CookieContainer 提取 Cookie 列表 ==========
    private List<CookieItem> ExtractCookiesFromContainer(HttpClientHandler handler)
    {
        var result = new List<CookieItem>();
        var cookieCollection = handler.CookieContainer.GetAllCookies();

        foreach (Cookie cookie in cookieCollection)
        {
            result.Add(new CookieItem
            {
                Name = cookie.Name,
                Value = cookie.Value,
                Domain = cookie.Domain,
                Path = cookie.Path,
                Expires = cookie.Expires,
                Secure = cookie.Secure,
                HttpOnly = cookie.HttpOnly
            });
        }

        return result;
    }

    private async Task<LoginResult> BuildLoginResult(LoginOption loginOption, HttpClient httpClient, HttpClientHandler httpClientHandler, HttpStatusCode statusCode)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, GET_ACCOUNT_INFO_URL);
            request.Headers.Referrer = new Uri(GET_ACCOUNT_INFO_REFERRER);

            var response = await httpClient.SendAsync(request);

            string html = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(html);

            return new LoginResult
            {
                Success = true,
                Message = AccountServiceStr.LoginSuccess,
                StatusCode = statusCode,
                CookieContent = loginOption.ExportCookie ? await SerializeCookiesToJson(httpClientHandler) : string.Empty,
                AccountName = GetAccountName(document) ?? "获取账户姓名失败",
                Grade = GetGrade(document) ?? "获取年级失败"
            };
        }
        catch (Exception ex){
            logger.LogDebug("Error while get account info: " + ex.Message);
            return new LoginResult
            {
                Success = true,
                Message = AccountServiceStr.LoginSuccess,
                StatusCode = statusCode,
                AccountName = "获取账户姓名失败",
                Grade = "获取年级失败"
            };
        }
    }

    private string? GetAccountName(HtmlDocument document)
    {
        try
        {
            var td = document.DocumentNode.SelectSingleNode("//td[contains(text(), '姓名：')]")
                ?? throw new HtmlWebException("Failed to get td from the html structure.");

            // 找到下一个 td 兄弟元素
            var nextTd = td.SelectSingleNode("following-sibling::td")
                ?? throw new HtmlWebException("Failed to get next td from the html structure.");

            // 获取 label 标签里的文本
            var label = nextTd.SelectSingleNode(".//label")
                ?? throw new HtmlWebException("Failed to get grade text from the td element.");

            string accountName = label.InnerText.Trim();

            logger.LogDebug("Account name: " + accountName);

            return accountName;
        }
        catch (HtmlWebException ex)
        {
            logger.LogDebug("Error while getting account name: " + ex);
            return null;
        }
    }

    private string? GetGrade(HtmlDocument document)
    {
        try
        {
            var td = document.DocumentNode.SelectSingleNode("//td[contains(text(), '所在年级：')]")
                ?? throw new HtmlWebException("Failed to get td from the html structure.");

            // 找到下一个 td 兄弟元素
            var nextTd = td.SelectSingleNode("following-sibling::td")
                ?? throw new HtmlWebException("Failed to get next td from the html structure.");

            // 获取 label 标签里的文本
            var label = nextTd.SelectSingleNode(".//label")
                ?? throw new HtmlWebException("Failed to get grade text from the td element.");

            string grade = label.InnerText.Trim();

            logger.LogDebug("Grade: " + grade);

            return grade;
        }
        catch (HtmlWebException ex)
        {
            logger.LogDebug("Failed to get Grade: " + ex);
            return null;
        }
    }

    private FormUrlEncodedContent BuildLoginData(string html, LoginOption loginOption)
    {
        logger.LogDebug(AccountServiceStr.UIA_BuildLoginData);
        string execution = ExtractValue(html, "name=\"execution\" value=\"");
        string encryptSalt = ExtractValue(html, "id=\"pwdEncryptSalt\" value=\"");
        if (string.IsNullOrWhiteSpace(execution) || string.IsNullOrWhiteSpace(encryptSalt))
        {
            throw new Exception(AccountServiceStr.UIA_CannotExtractExecutionOrEncryptSaltError);
        }

        string encryptedPassword = EncryptPassword(loginOption.Password, encryptSalt);

        // Build login data，username, password, _eventId, execution data is neccessary，
        // But we will keep the same with the original post.
        // preserve insertion order and allow inserting in the middle
        var formList = new List<KeyValuePair<string, string>>
            {
                new("username", loginOption.UserName),
                new("password", encryptedPassword),
                new("captcha", ""),
                new("_eventId", "submit"),
                new("cllt", "userNameLogin"),
                new("dllt", "generalLogin"),
                new("lt", ""),
                new("execution", execution),
            };

        // Insert UIArememberMe at a specific position (e.g. after "dllt")
        if (loginOption.RememberMe)
        {
            formList.Insert(3, new KeyValuePair<string, string>("rememberMe", "true"));
            logger.LogDebug(AccountServiceStr.UIA_EnableRememberMe);
        }

        return new FormUrlEncodedContent(formList);
    }

    private string ExtractValue(string html, string pattern)
    {
        try
        {
            int start = html.IndexOf(pattern);
            if (start == -1) return "";
            start += pattern.Length;
            int end = html.IndexOf('"', start);
            return end > start ? html[start..end] : "";
        }
        catch (Exception ex)
        {
            logger.LogDebug(AccountServiceStr.UIA_ExtractValueFailed, ex.Message);
            return "";
        }
    }

    /// <summary>
    /// 广东工厂大学统一身份认证密码加密的方式。加密的 javascript 代码可以直接从前端获取，此处翻译为 csharp 代码
    /// </summary>
    /// <param name="password"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    private string EncryptPassword(string password, string salt)
    {
        // 生成 64 位随机字符串前缀，前缀的每一位从 AES_CHARS 中选取
        // 请使用 RandomString() 方法生成有范围的随机比特数组
        string plainText = RandomString(64) + password;

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(salt);

        // 生成 16 位随机 iv，iv 的每一位从 AES_CHARS 中选取
        // 请使用 RandomString() 方法生成有范围的随机比特数组
        aes.IV = Encoding.UTF8.GetBytes(RandomString(16));

        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        string encryptPassword = Convert.ToBase64String(encryptBytes);
        return encryptPassword;
    }

    // 生成随机字符串
    private string RandomString(int length)
    {
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = AES_CHARS[random.Next(AES_CHARS.Length)];
        }
        return new string(result);
    }
}