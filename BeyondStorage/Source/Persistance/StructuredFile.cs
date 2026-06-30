using System.Collections.Generic;
using System.IO;

namespace BeyondStorage.Source.Persistance;

public class StructuredFile
{
    /*** This class processes structured text files with the following definition:
     * all lines in the format: tag|key:length:value|key:length:value
     * keys may appear in different orders or not be present in all lines
     * there has to be a tag per line
     * there has to be at least one key per line
     * tag format: [a-zA-Z0-9_], must be non-empty
     * key format: [a-zA-Z0-9_], must be non-empty
     * length format: integer range 0 or greater
     * first line may or may not be a header line, containing metadata
     * header line format: META|ver:length:value
     * header line may contain other keys
     * header line has to contain the "ver" key
     * value for "ver" in the header MUST be an integer greater than 0
     * tags are not case-sensitive
     * keys are not case-sensitive
     * multiple tags are allowed per file. a tag is the same concept as a record type, or the classifier for the line of data
     ***/

    private readonly Dictionary<string, StructuredRecord> _records = [];
    private StructuredRecord _meta = null;

    public int Version => _meta?.GetField("ver")?.AsInt ?? 0;
    public bool HasMeta => _meta != null;

    public void Clear()
    {
        _records.Clear();
        _meta = null;
    }

    public bool HasRecord(string tag)
    {
        return _records.ContainsKey(tag.ToLowerInvariant());
    }

    public StructuredRecord GetRecord(string tag)
    {
        _records.TryGetValue(tag.ToLowerInvariant(), out StructuredRecord record);
        return record;
    }

    public void AddRecord(StructuredRecord record)
    {
        _records[record.Tag] = record;
    }

    public void SetMeta(int version)
    {
        _meta = new StructuredRecord("meta");
        _meta.SetField("ver", version.ToString());
    }

    public void ReadFile(string fileName)
    {
        Clear();
        if (!File.Exists(fileName))
        {
            return;
        }

        bool isFirstLine = true;
        using var reader = new StreamReader(fileName);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            int pipePos = line.IndexOf('|');
            if (pipePos <= 0)
            {
                continue;
            }

            string tag = line.Substring(0, pipePos).ToLowerInvariant();
            string rest = line.Substring(pipePos + 1);

            var record = new StructuredRecord(tag);
            record.AssignFrom(rest);

            if (isFirstLine && tag == "meta")
            {
                isFirstLine = false;
                var verField = record.GetField("ver");
                if (verField != null && verField.AsInt > 0)
                {
                    _meta = record;
                }
                continue;
            }
            isFirstLine = false;

            if (!record.IsEmpty())
            {
                _records[tag] = record;
            }
        }
    }

    public void WriteFile(string fileName)
    {
        using var writer = new StreamWriter(fileName, append: false);

        if (_meta != null)
        {
            writer.WriteLine(_meta);
        }

        foreach (var record in _records.Values)
        {
            writer.WriteLine(record);
        }
    }
}
