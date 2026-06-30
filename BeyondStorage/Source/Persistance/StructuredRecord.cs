using System.Collections.Generic;
using System.Text;

namespace BeyondStorage.Source.Persistance;

public class StructuredRecord
{
    /*** Contains a list of key/value pairs that go into a StructuredFile
     * all lines in the format: tag|key:length:value|key:length:value
     ***/

    public string Tag
    {
        get; private set;
    }

    private readonly Dictionary<string, StructuredField> _fields = [];

    public int FieldCount
    {
        get
        {
            return _fields?.Count ?? 0;
        }
    }

    public StructuredRecord(string tag)
    {
        Tag = tag.ToLowerInvariant();
    }

    public void Clear()
    {
        _fields.Clear();
    }

    public bool IsEmpty()
    {
        return FieldCount == 0;
    }

    public bool HasField(string key)
    {
        return _fields.ContainsKey(key.ToLowerInvariant());
    }

    public StructuredField GetField(string key)
    {
        _fields.TryGetValue(key.ToLowerInvariant(), out StructuredField field);
        return field;
    }

    public void SetField(string key, string value)
    {
        _fields[key.ToLowerInvariant()] = new StructuredField(value);
    }

    public void AssignFrom(string line)
    {
        Clear();

        if (string.IsNullOrEmpty(line))
        {
            return;
        }

        var pos = 0;
        int lineLen = line.Length;

        while (pos < lineLen)
        {
            // key 
            int colonPos1 = line.IndexOf(':', pos);
            if (colonPos1 <= pos)
            {
                return;
            }

            string key = line.Substring(pos, colonPos1 - pos).ToLowerInvariant();
            pos = colonPos1 + 1;

            // length
            int colonPos2 = line.IndexOf(':', pos);
            if (colonPos2 < pos)
            {
                return;
            }

            if (!int.TryParse(line.Substring(pos, colonPos2 - pos), out int valueLen) || valueLen < 0)
            {
                return;
            }

            pos = colonPos2 + 1;

            // value (exactly valueLen chars — may contain '|' or ':')
            if (pos + valueLen > lineLen)
            {
                return;
            }

            string value = line.Substring(pos, valueLen);
            pos += valueLen;

            _fields[key] = new StructuredField(value);

            // skip field separator
            if (pos < lineLen && line[pos] == '|')
            {
                pos++;
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder(Tag);
        foreach (var (key, value) in _fields)
        {
            sb.Append('|');
            sb.Append(key);
            sb.Append(':');
            sb.Append(value);
        }
        return sb.ToString();
    }
}
