using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace TestService.Model
{
    public class Settings
    {
        private readonly IConfiguration configuration;

        public Settings(IConfiguration configuration) 
        {
            this.configuration = configuration;
        }    
        
        //временной интервал
        public int Timeout => configuration.GetValue<int>("Timeout");
        public string ResultPath => configuration.GetValue<string>("ResultPath");        

        //данные для конектна к базе
        public Dictionary<string, string> DataBase => configuration.GetSection("DataBase")
                                                                    .GetChildren()
                                                                    .ToDictionary(x => x.Key, x => x.Value);
        //данные о сайте
        public Dictionary<string, string> Websites => configuration.GetSection("Website")
                                                                    .GetChildren()
                                                                    .ToDictionary(x => x.Key, x => x.Value);

    }
}
