using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IsoBridge.Core.ISO;
using IsoBridge.Core.Models;
using Microsoft.Extensions.Options;

namespace IsoBridge.ISO8583
{
    public sealed class DefaultIsoParser : IIsoParser
    {
        private readonly Iso8583Options _options;
        private readonly TemplateLoader _templates;
        private readonly IFieldCodec _codec;

        public DefaultIsoParser(IOptions<Iso8583Options> options)
        {
            _options = options.Value;
            _templates = new TemplateLoader(_options.TemplatePath);
            _codec = new DefaultFieldCodec();
        }

        public ParseResult Parse(ReadOnlySpan<byte> isoBytes)
        {
            try
            {
                // first 4 bytes are always MTI
                var mti = Encoding.ASCII.GetString(isoBytes[..4]);
                var template = _templates.GetTemplate(mti);

                // parse bitmaps (8 or 16 bytes)
                var bitmapFields = BitmapUtils.ParseBitmap(isoBytes.Slice(4));
                var bitmapSize = bitmapFields.Max() > 64 ? 16 : 8;

                var index = 4 + bitmapSize;
                var remaining = isoBytes[index..];
                var fields = new Dictionary<int, string>();

                // decode field values in order
                foreach (var field in template.Fields.OrderBy(f => f.Id))
                {
                    if (!bitmapFields.Contains(field.Id))
                        continue;

                    var (value, used) = _codec.Decode(
                        remaining, field.Length, field.Variable,
                        field.VarLengthDigits, _options.UseBcd);

                    fields[field.Id] = value;
                    remaining = remaining[used..];
                }

                var msg = new IsoMessage(mti, fields);
                return new ParseResult(true, msg, null);
            }
            catch (Exception ex)
            {
                return new ParseResult(false, null, ex.Message);
            }
        }

        public byte[] Build(IsoMessage message)
        {
            var template = _templates.GetTemplate(message.Mti);
            var active = message.Fields.Keys.OrderBy(f => f).ToList();
            var bitmap = BitmapUtils.BuildBitmap(active);

            var buffer = new List<byte>(256);
            buffer.AddRange(Encoding.ASCII.GetBytes(message.Mti));
            buffer.AddRange(bitmap);

            foreach (var field in template.Fields.Where(f => active.Contains(f.Id)))
            {
                var value = message.Fields[field.Id];
                var bytes = _codec.Encode(value, field.Length, field.Variable, field.VarLengthDigits, _options.UseBcd);
                buffer.AddRange(bytes);
            }

            return buffer.ToArray();
        }
    }
}