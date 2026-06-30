using System.Globalization;

namespace BeyondStorage.Source.Persistance;

public class StructuredField
{
    /*** contains the functions needed for a field in a StructuredRecord
     * a field is this part of a line: key:length:value
     * the record will have the key, no need to duplicate it here
     ***/

    public string AsString
    {
        get;
        private set => field = value ?? "";
    }

    public int Length
    {
        get
        {
            return AsString.Length;
        }
    }

    public StructuredField(string value)
    {
        AsString = value;
    }

    public int AsInt
    {
        get => int.TryParse(AsString, out int v) ? v : default;
        set => AsString = value.ToString();
    }

    public float AsFloat
    {
        get => float.TryParse(AsString, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : default;
        set => AsString = value.ToString(CultureInfo.InvariantCulture);
    }

    public override string ToString() => $"{Length}:{AsString}";

    public bool AsBool
    {
        get
        {
            if (bool.TryParse(AsString, out bool b))
            {
                return b;
            }

            if (int.TryParse(AsString, out int i))
            {
                return i != 0;
            }

            return default;
        }
        set => AsString = value.ToString();
    }
}
