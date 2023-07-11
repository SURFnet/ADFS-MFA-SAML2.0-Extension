// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeysJsonConverter.cs" company="Winvision bv">
//   Copyright (c) Winvision bv. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class LoaGroupConfigurationsJsonConverter : JsonConverter
    {
        private readonly Type[] types;

        public LoaGroupConfigurationsJsonConverter()
        {
            this.types = new[]
            {
                typeof(List<LoaGroupConfiguration>)
            };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var result = new List<LoaGroupConfiguration>();
            var key = string.Empty;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Boolean:
                    case JsonToken.Null:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        throw new NotSupportedException();

                    case JsonToken.String:
                        result.Add(new LoaGroupConfiguration(key, Convert.ToString(reader.Value)));
                        break;

                    default:
                        if (reader.Value != null)
                        {
                            key = reader.Value.ToString();
                        }

                        break;
                }
            }

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return this.types.Any(t => t == objectType);
        }
    }
}