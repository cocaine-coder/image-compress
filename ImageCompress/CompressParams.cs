public class CompressParameter
{
    public string Input { get; set; }

    public string Output { get; set; }

    public bool Convert { get; set; }

    public int Quality { get; set; }

    public int Limit { get; set; }

    public override string ToString()
    {
        return $"{nameof(Input)} : {Input}\n" +
               $"{nameof(Output)} : {Output}\n" +
               $"{nameof(Convert)} : {Convert}\n" +
               $"{nameof(Quality)} : {Quality}\n" +
               $"{nameof(Limit)} : {Limit}\n";
    }
}