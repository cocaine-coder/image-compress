// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

var rootCommand = CreateRootCommandWithOptions();

// Note that the parameters of the handler method are matched according to the names of the options

rootCommand.Handler = CommandHandler.Create<CompressParameter>(param =>
 {
     var imageCompress = new ImageCompress();
     imageCompress.Compress(param);
 });

// Parse the incoming args and invoke the handler
return rootCommand.InvokeAsync(args).Result;

static RootCommand CreateRootCommandWithOptions()
{
    var inputOption = new Option<string>(
        aliases: new[] { "--input", "-i" },
        description: "path of image or directory")
    {
        IsRequired = true
    };

    inputOption.AddValidator(result =>
    {
        var value = result.GetValueOrDefault<string>("");
        if (!(File.Exists(value) || Directory.Exists(value)))
        {
            return $"{value} not exists";
        }
        return null;
    });

    var outputOption = new Option<string?>(
            aliases: new[] { "--output", "-o" },
            // parseArgument: result =>
            //  {
            //      var value = result.ToString().Substring(22).Trim(new[] { ' ', '<', '>' });
            //      if (string.IsNullOrEmpty(value))
            //      {
            //          var inputSymbolResultSet = result.Parent?.Children[0];
            //          var inputOptionResult = inputSymbolResultSet?.FindResultFor(inputOption);
            //          var input = inputOptionResult?.GetValueOrDefault()?.ToString();
            //          return Directory.Exists(input)?input:Path.GetDirectoryName(input);
            //      }
            //      return value;`
            //  },
            description: "output path. [default: directory of input]");

    // outputOption.AddValidator(result =>
    // {
    //     var value = result.GetValueOrDefault<string>("");
    //     if (!string.IsNullOrWhiteSpace(value) && !Directory.Exists(value))
    //     {
    //         return $"{value} not exists";
    //     }
    //     return null;
    // });

    var qualityOption = new Option<int>(
        aliases: new[] { "--quality", "-q" },
        getDefaultValue: () => 70,
        description: "quality of compressed image (only works on *.jpg)");

    qualityOption.AddValidator(result =>
    {
        var value = result.GetValueOrDefault<int>(0);
        if (value <= 0 || value > 100)
        {
            return "quality must > 0 and <= 100";
        }
        return null;
    });

    var limitOption = new Option<int>(
        aliases: new[] { "--limit", "-l" },
        getDefaultValue: () => 0,
        description: "min size of origin image"
    );

    limitOption.AddValidator(result =>
    {
        var value = result.GetValueOrDefault<int>(-1);
        if (value < 0)
        {
            return "limit must > 0";
        }
        return null;
    });

    return new RootCommand("compress image by C#, support *.jpg *.png")
    {
        inputOption,
        outputOption,
        new Option<bool>(
            aliases: new[] {"--convert","-cv"},
            getDefaultValue:()=>false,
            description: "whether to convert origin file"
        ),
        qualityOption,
        limitOption,
        new Option<bool>(
            aliases:new[]{"--recurse","-r"},
            getDefaultValue:()=>false,
            description: "whether recurse directory"
        )
    };
}

public static class CommandLineExtension
{
    public static T? GetValueOrDefault<T>(this OptionResult result, T? defaultValue)
    {
        try
        {
            return result.GetValueOrDefault<T>();
        }
        catch
        {
            return defaultValue;
        }
    }
}