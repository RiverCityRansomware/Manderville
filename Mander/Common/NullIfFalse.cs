using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manderville.Common
{
    public class NullIfFalseConverter : JsonConverter
    {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            if (reader.TokenType == JsonToken.Boolean)
            {
                return null;
            } else if (objectType == typeof(double) && reader.Value == null) {
                return 0.0;
            }
            else
            {
                return serializer.Deserialize(reader, objectType);
            }


        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

    }
}
