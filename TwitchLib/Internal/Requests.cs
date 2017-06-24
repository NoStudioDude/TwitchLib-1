namespace TwitchLib.Internal
{
    #region using directives
    using Newtonsoft.Json;
    using System.IO;
    using System.Net;
    using Exceptions.API;
    using Newtonsoft.Json.Serialization;
    using System.Threading.Tasks;
    using System;
    using System.Text;
    #endregion
    internal static class Requests
    {
        public static class TwitchApp {
            public static string Get(string url)
            {
                return new System.Net.WebClient().DownloadString(url);
            }
            public static T Get<T>(string url)
            {
                return JsonConvert.DeserializeObject<T>(new System.Net.WebClient().DownloadString(url));
            }

            public static async Task<string> Post(string url, string payload)
            {
                var req = WebRequest.CreateHttp(url);
                req.Method = "POST";
                req.ContentType = "application/json";

                using (var writer = new StreamWriter(req.GetRequestStream()))
                {
                    writer.Write(payload);
                }

                var response = req.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }

            public static async Task<T> Post<T>(string url, string token, string payload = null)
            {
                string resp = await Post(url, token, payload);
                return JsonConvert.DeserializeObject<T>(resp, TwitchLibJsonDeserializer);
            }

            public static async Task<T> PostModel<T>(string url, string token, Models.App.RequestModels.BaseModel model)
            {
                string resp = await Post(url, token, JsonConvert.SerializeObject(model));
                return JsonConvert.DeserializeObject<T>(resp, TwitchLibJsonDeserializer);
            }

            public static async Task<string> Post(string url, string token, string payload = null)
            {
                var req = WebRequest.CreateHttp(url);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Headers["AuthenticationToken"] = token;

                if (payload != null)
                {
                    using (var writer = new StreamWriter(req.GetRequestStream()))
                        await writer.WriteAsync(payload);
                }

                var response = await req.GetResponseAsync();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return await reader.ReadToEndAsync();
                }
            }

            public static async Task<Models.App.Login.OAuth> Auth(string code)
            {
                string url = @"https://logins-v1.curseapp.net/login/twitch-oauth";
                string payload = "{\"ClientID\":\"jf3xu125ejjjt5cl4osdjci6oz6p93r\",\"Code\":\"" + code + "\",\"State\":\"48ea05795ea572833b3b51dbdd59efe3\",\"RedirectUri\":\"" + @"https://web.curseapp.net/laguna/passport-callback.html" + "\"}";

                var req = WebRequest.CreateHttp(url);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.ContentLength = 200;

                using (var writer = new StreamWriter(req.GetRequestStream()))
                {
                    await writer.WriteAsync(payload);
                }

                var response = req.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<Models.App.Login.OAuth>(await reader.ReadToEndAsync(), TwitchLibJsonDeserializer);
                }
            }
        }

        public enum API
        {
            v3 = 3,
            v5 = 5,
            Void = 0
        }

        #region POST
        #region PostGenericModel
        public async static Task<T> PostGenericModel<T>(string url, Models.API.RequestModel model, string accessToken = null, API api = API.v5, string clientId = null)
        {
            if (model != null)
                return JsonConvert.DeserializeObject<T>(await generalRequest(url, "POST", TwitchLibJsonSerializer.SerializeObject(model), accessToken, api, clientId), TwitchLibJsonDeserializer);
            else
                return JsonConvert.DeserializeObject<T>(await generalRequest(url, "POST", "", accessToken, api), TwitchLibJsonDeserializer);
        }
        #endregion
        #region PostGeneric
        public async static Task<T> PostGeneric<T>(string url, string payload, string accessToken = null, API api = API.v5, string clientId = null)
        {
            return JsonConvert.DeserializeObject<T>(await generalRequest(url, "POST", payload, accessToken, api, clientId), TwitchLibJsonDeserializer);
        }
        #endregion
        #region PostModel
        public async static Task PostModel(string url, Models.API.RequestModel model, string accessToken = null, API api = API.v5, string clientId = null)
        {
            await generalRequest(url, "POST", TwitchLibJsonSerializer.SerializeObject(model), accessToken, api, clientId);
        }
        #endregion
        #region Post
        public async static Task Post(string url, string payload, string accessToken = null, API api = API.v5, string clientId = null)
        {
            await generalRequest(url, "POST", payload, accessToken, api, clientId);
        }
        #endregion
        #endregion
        #region GET
        #region GetGeneric
        public async static Task<T> GetGeneric<T>(string url, string accessToken = null, API api = API.v5, string clientId = null)
        {
            return JsonConvert.DeserializeObject<T>(await generalRequest(url, "GET", null, accessToken, api, clientId), TwitchLibJsonDeserializer);
        }
        #endregion
        #region GetSimpleGeneric
        public async static Task<T> GetSimpleGeneric<T>(string url)
        {
            return JsonConvert.DeserializeObject<T>(await simpleRequest(url), TwitchLibJsonDeserializer);
        }
        #endregion
        #endregion
        #region DELETE
        #region Delete
        public async static Task<string> Delete(string url, string accessToken = null, API api = API.v5, string clientId = null)
        {
            return await generalRequest(url, "DELETE", null, accessToken, api, clientId);
        }
        #endregion
        #region DeleteGeneric
        public async static Task<T> DeleteGeneric<T>(string url, string accessToken = null, API api = API.v5, string clientId = null)
        {
            return JsonConvert.DeserializeObject<T>(await generalRequest(url, "DELETE", null, accessToken, api, clientId), TwitchLibJsonDeserializer);
        }
        #endregion



        #endregion
        #region PUT
        #region PutGeneric
        public async static Task<T> PutGeneric<T>(string url, string payload, string accessToken = null, API api = API.v5, string clientId = null)
        {
            return JsonConvert.DeserializeObject<T>(await generalRequest(url, "PUT", payload, accessToken, api, clientId), TwitchLibJsonDeserializer);
        }
        #endregion
        #region Put
        public async static Task<string> Put(string url, string payload, string accessToken = null, API api = API.v5, string clientId = null)
        {
            return await generalRequest(url, "PUT", payload, accessToken, api, clientId);
        }
        #endregion
        #region PutBytes
        public async static Task PutBytes(string url, byte[] payload)
        {
            try
            {
                using (var client = new WebClient())
                    client.UploadData(new Uri(url), "PUT", payload);
            }
            catch (WebException ex) { handleWebException(ex); }
        }
        #endregion
        #endregion

        #region generalRequest
        private async static Task<string> generalRequest(string url, string method, object payload = null, string accessToken = null, API api = API.v5, string clientId = null)
        {
            if (clientId == null)
                checkForCredentials();
            url = appendClientId(url, clientId);

            var request = WebRequest.CreateHttp(url);
            request.Method = method;
            request.ContentType = "application/json";

            if (api != API.Void)
                request.Accept = $"application/vnd.twitchtv.v{(int)api}+json";

            if (!string.IsNullOrEmpty(accessToken))
                request.Headers["Authorization"] = $"OAuth {Common.Helpers.FormatOAuth(accessToken)}";
            else if (!string.IsNullOrEmpty(TwitchAPI.Shared.AccessToken))
                request.Headers["Authorization"] = $"OAuth {TwitchAPI.Shared.AccessToken}";

            if (payload != null)
                using (var writer = new StreamWriter(await request.GetRequestStreamAsync()))
                    writer.Write(payload);

            try
            {
                var response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string data = await reader.ReadToEndAsync();
                    return data;
                }
            }
            catch (WebException ex) { handleWebException(ex); }

            return null;
        }
        #endregion
        #region simpleRequest
        // credit: https://stackoverflow.com/questions/14290988/populate-and-return-entities-from-downloadstringcompleted-handler-in-windows-pho
        public async static Task<string> simpleRequest(string url)
        {
            var tcs = new TaskCompletionSource<string>();
            var client = new WebClient();

            DownloadStringCompletedEventHandler h = null;
            h = (sender, args) =>
            {
                if (args.Cancelled)
                    tcs.SetCanceled();
                else if (args.Error != null)
                    tcs.SetException(args.Error);
                else
                    tcs.SetResult(args.Result);

                client.DownloadStringCompleted -= h;
            };

            client.DownloadStringCompleted += h;
            client.DownloadString(new Uri(url));

            return await tcs.Task;
        }
        #endregion

        #region appendClientId
        private static string appendClientId(string url, string clientId = null)
        {
            if (clientId == null)
                return url.Contains("?")
                    ? $"{url}&client_id={TwitchAPI.Shared.ClientId}"
                    : $"{url}?client_id={TwitchAPI.Shared.ClientId}";
            else
                return url.Contains("?")
                    ? $"{url}&client_id={clientId}"
                    : $"{url}?client_id={clientId}";
        }
        #endregion
        #region checkForCredentials
        private static void checkForCredentials()
        {
            if (string.IsNullOrEmpty(TwitchAPI.Shared.ClientId) && string.IsNullOrWhiteSpace(TwitchAPI.Shared.AccessToken))
                throw new InvalidCredentialException("All API calls require Client-Id or OAuth token. Set Client-Id by using SetClientId(\"client_id_here\")");
        }
        #endregion

        #region handleWebException
        private static void handleWebException(WebException e)
        {
            HttpWebResponse errorResp = e.Response as HttpWebResponse;
            if (errorResp == null)
                throw e;
            switch (errorResp.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException("Your request failed because either: \n 1. Your ClientID was invalid/not set.\n 2. You requested a username when the server was expecting a user ID.");
                case HttpStatusCode.Unauthorized:
                    throw new BadScopeException("Your request was blocked due to bad credentials (do you have the right scope for your access token?).");
                case HttpStatusCode.NotFound:
                    throw new BadResourceException("The resource you tried to access was not valid.");
                case (HttpStatusCode)422:
                    throw new NotPartneredException("The resource you requested is only available to channels that have been partnered by Twitch.");
                default:
                    throw e;
            }
        }
        #endregion

        #region SerialiazationSettings
        public static JsonSerializerSettings TwitchLibJsonDeserializer = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };

        public class TwitchLibJsonSerializer
        {
            private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                ContractResolver = new LowercaseContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            public static string SerializeObject(object o)
            {
                return JsonConvert.SerializeObject(o, Formatting.Indented, Settings);
            }

            public class LowercaseContractResolver : DefaultContractResolver
            {
                protected override string ResolvePropertyName(string propertyName)
                {
                    return propertyName.ToLower();
                }
            }
        }
        #endregion
    }
}