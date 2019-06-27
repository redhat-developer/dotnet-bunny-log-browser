using Microsoft.Extensions.Configuration;
class JobsConfig
{
    public string JobsDirectory{get; private set;}
    public string JobsPrefix{get; private set;}
    private static JobsConfig instance;
    private JobsConfig(string file_path)
    {
        IConfiguration config=new ConfigurationBuilder().AddJsonFile(file_path).Build();
        JobsDirectory=config.GetValue<string>("jobs_directory");
        JobsPrefix=config.GetValue<string>("jobs_prefix");
    }
    public static void Create(string file_path)
    {
        if(instance==null)
        {
            instance=new JobsConfig(file_path);
        }
    }
    public static JobsConfig Get() => instance;
}