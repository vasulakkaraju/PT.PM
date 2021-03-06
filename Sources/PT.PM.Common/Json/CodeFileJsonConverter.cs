﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PT.PM.Common.Json
{
    public class CodeFileJsonConverter : JsonConverter
    {
        public bool IncludeCode { get; set; } = true;

        public bool ExcludeDefaults { get; set; } = true;

        public CodeFile CodeFile { get; private set; } = CodeFile.Empty;

        public TextSpanJsonConverter TextSpanJsonConverter { get; set; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CodeFile);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = new JObject();
            var sourceCodeFile = (CodeFile)value;

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.RootPath))
                jObject.Add(nameof(sourceCodeFile.RootPath), sourceCodeFile.RootPath);

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.RelativePath))
                jObject.Add(nameof(sourceCodeFile.RelativePath), sourceCodeFile.RelativePath);

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.Name))
                jObject.Add(nameof(sourceCodeFile.Name), sourceCodeFile.Name);

            if (IncludeCode &&
                (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.Code)))
            {
                jObject.Add(nameof(sourceCodeFile.Code), sourceCodeFile.Code);
            }

            jObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            CodeFile result = new CodeFile((string)obj.GetValueIgnoreCase(nameof(CodeFile.Code)) ?? "")
            {
                RootPath = (string)obj.GetValueIgnoreCase(nameof(CodeFile.RootPath)) ?? "",
                RelativePath = (string)obj.GetValueIgnoreCase(nameof(CodeFile.RelativePath)) ?? "",
                Name = (string)obj.GetValueIgnoreCase(nameof(CodeFile.Name)) ?? "",
            };

            CodeFile = result;
            if (TextSpanJsonConverter != null)
            {
                TextSpanJsonConverter.CodeFile = result;
            }
            return result;
        }
    }
}
