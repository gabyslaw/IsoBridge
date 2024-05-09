using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IsoBridge.ISO8583.Templates;

namespace IsoBridge.ISO8583
{
    public sealed class TemplateLoader
    {
        private readonly string _path;
        private IReadOnlyDictionary<string, Iso8583Template>? _cache;

        public TemplateLoader(string path) => _path = path;

        public IReadOnlyDictionary<string, Iso8583Template> Load()
        {
            if (_cache is not null)
                return _cache;

            if (!File.Exists(_path))
                throw new FileNotFoundException($"Template file not found at {_path}");

            var json = File.ReadAllText(_path);
            var templates = JsonSerializer.Deserialize<Dictionary<string, Iso8583Template>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new Dictionary<string, Iso8583Template>();

            _cache = templates;
            return _cache;
        }

        public Iso8583Template GetTemplate(string mti)
        {
            var all = Load();
            if (all.TryGetValue(mti, out var template))
                return template;

            throw new InvalidOperationException($"No template found for MTI '{mti}'.");
        }
    }
}