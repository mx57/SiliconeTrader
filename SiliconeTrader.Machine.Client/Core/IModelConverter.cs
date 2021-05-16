namespace SiliconeTrader.Machine.Client.Core
{
    public interface IModelConverter
    {
        T Deserialize<T>(string json);

        string Serialize<T>(T @object);

        /// <summary>
        /// Generates http query parameters from the given object by looping through all non-null properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="params"></param>
        /// <returns></returns>
        string ToHttpQuery<T>(T @params);
    }
}