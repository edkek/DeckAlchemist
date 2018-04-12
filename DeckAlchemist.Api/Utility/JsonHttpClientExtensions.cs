﻿using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeckAlchemist.Api.Utility
{
    public static class JsonHttpClientExtensions
    {
        public class JsonContent : StringContent
        {
            public JsonContent(object obj) : base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"){}
        }

        public static HttpClient Auth(this HttpClient client, string idToken)
        {
            client.DefaultRequestHeaders.Add("Authorization", idToken);
            return client;
        }

        public static Task<HttpResponseMessage> PutAsync(this HttpClient client, string requestUri, object obj)
        {
            return client.PutAsync(requestUri, new JsonContent(obj));
        }

        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string requestUri, object obj)
        {
            return client.PostAsync(requestUri, new JsonContent(obj));
        }

        public static Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var task = new Task<T>(() =>
            {
                var str = content.ReadAsStringAsync();
                str.Wait();
                return JsonConvert.DeserializeObject<T>(str.Result);
            });
            task.Start();
            return task;
        }

    }
}
