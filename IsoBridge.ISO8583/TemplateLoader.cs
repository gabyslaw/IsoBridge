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
            if (_cache is not null) return _cache;
            var json = File.ReadAllText(_path);
            var templates = JsonSerializer.Deserialize<List<Iso8583Template>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            _cache = templates.ToDictionary(t => t.Mti);
            return _cache;
        }

        public Iso8583Template GetTemplate(string mti) =>
            Load().TryGetValue(mti, out var t)
                ? t
                : throw new InvalidOperationException($"No template for MTI {mti}");
    }
}