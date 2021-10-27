public class CompressParameter
{
    public string Input { get; set; }

    public string Output { get; set; }

    public bool Convert { get; set; }

    public int Quality { get; set; }

    public int Limit { get; set; }

    public bool Recurse { get; set; }

    public override string ToString()
    {
        var ret = "";

        foreach(var field in this.GetType().GetFields(bindingAttr:System.Reflection.BindingFlags.Public))
        {
            var name = field.Name;
            var value = field.GetValue(this);

            ret += $"{name} : {value}\n";
        }
        
        return ret;
    }
}