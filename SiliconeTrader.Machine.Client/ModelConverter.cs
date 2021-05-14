using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace SiliconeTrader.Machine.Client
{
    public class ModelConverter : IModelConverter
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string Serialize<T>(T @object)
        {
            return JsonConvert.SerializeObject(@object, JsonSerializerSettings);
        }

        public string ToHttpQuery<T>(T @params)
        {
            if (@params == null)
            {
                return string.Empty;
            }

            IEnumerable<string> getQueryData()
            {
                foreach (PropertyInfo param in @params.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name))
                {
                    object value = param.GetValue(@params, null);

                    if (value != null && value != default)
                    {
                        yield return this.GetQueryData(param, param.GetValue(@params, null).ToString().ToLowerInvariant());
                    }
                }
            }

            IEnumerable<string> properties = getQueryData();

            // queryString will be set to for example: "Id=1&State=26&Prefix=f&Index=oo"
            string queryString = string.Join("&", properties.ToArray());

            return queryString;
        }

        private string GetQueryData(PropertyInfo param, string value)
        {
            string camelCaseName = char.ToLowerInvariant(param.Name[0]) + param.Name.Substring(1);
            string encodedValue = HttpUtility.UrlEncode(value);

            return $"{camelCaseName}={encodedValue}";
        }
    }
}