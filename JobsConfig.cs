using Microsoft.Extensions.Configuration;

class JobsConfig
{
    public string JobsDirectory {get; private set;}
    public string JobsPattern {get; private set;}
    public string JobsURL {get; private set;}
    public bool UseJson {get; private set;}

    private static JobsConfig instance;
    
    private JobsConfig(string file_path)
    {
        var config = new ConfigurationBuilder().AddJsonFile(file_path).Build();
        JobsDirectory = config.GetValue<string>("jobs_directory");
        JobsPattern = config.GetValue<string>("jobs_prefix", "") + "*";
        JobsURL = config.GetValue<string>("jobs_url");
        UseJson = config.GetValue<bool>("use_json", false);
    }
    public static void Create(string file_path)
    {
        if(instance==null)
        {
            instance = new JobsConfig(file_path);
        }
    }
    public static JobsConfig Get() => instance;
}