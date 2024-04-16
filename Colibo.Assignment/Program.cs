using System.Net.Http.Headers;
using System.Xml;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Colibo.Assignment.Models;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Formatting = Newtonsoft.Json.Formatting;

namespace Colibo.Assignment
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
                // Logging Server URL
                //builder.AddSeq("http://localhost:5341");
            });

            ILogger logger = loggerFactory.CreateLogger<Program>();

            // Get an access token for MS Graph API access
            var app = ConfidentialClientApplicationBuilder
            .Create("1d071267-e9b0-449c-bf44-fe278a923929")
            .WithTenantId("88592590-0df3-4d4f-8f2b-c86731dc0c44")
            .WithClientSecret("Kw_c09wp-u6PJ5siFtSu2Vu-..5_I33W~a")
            //.WithLogging((level, message, containsPii) =>
            //{
            //    Console.WriteLine($"MSAL: {level} {message} ");
            //}, LogLevel.Info, enablePiiLogging: true, enableDefaultPlatformLogging: true)
            .WithAuthority(new Uri("https://login.microsoftonline.com/88592590-0df3-4d4f-8f2b-c86731dc0c44/v2.0/.well-known/openid-configuration"))
            .Build();
            var tokenResponse = await app.AcquireTokenForClient(new[] { ".default" }).ExecuteAsync();

            // Sample access token usage on HttpClient
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                tokenResponse.AccessToken);

            HttpResponseMessage response = await client.GetAsync("https://graph.microsoft.com/v1.0/users");
            var content = await response.Content.ReadAsStringAsync();
            var graphModel = JsonConvert.DeserializeObject<Models.GraphModel>(content);


            // Read xml data from embedded file
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.Export.xml");

            // Read xml file from local path
            var xmlPath = "C:\\Users\\Filip\\Desktop\\Colibo.Assignment\\Colibo.Assignment\\Resources\\Export.xml";
            var xmlContent = await File.ReadAllTextAsync(xmlPath);

            // Load xml content into XmlDocument
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            // Convert xml data to json
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            // Deserialize json data to object
            var coliboModel = JsonConvert.DeserializeObject<ColiboModel>(jsonText);

            try
            {
                CombineData(coliboModel!.Data!.Persons!.Person!, graphModel!.Value!, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error combining data");
            }

            // Monitor xml file for changes
            var fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = Path.GetDirectoryName(xmlPath)!;
            fileWatcher.Filter = "Export.xml";
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;

            Console.WriteLine(fileWatcher.Path + fileWatcher.Filter);

            fileWatcher.Changed += async (sender, e) =>
            {
                // Read xml file
                var xmlContent = await File.ReadAllTextAsync(xmlPath);

                // Load xml content into XmlDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                // Convert xml data to json
                jsonText = JsonConvert.SerializeXmlNode(doc);
                // Deserialize json data to object
                coliboModel = JsonConvert.DeserializeObject<ColiboModel>(jsonText);

                // Combine data from both sources
                if (coliboModel != null && graphModel != null)
                {
                    try
                    {
                        CombineData(coliboModel.Data!.Persons!.Person!, graphModel.Value!, logger);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error combining data");
                    }
                }
            };

            fileWatcher.EnableRaisingEvents = true;

            await Task.Delay(-1);
        }

        public static void CombineData(List<Person> persons, List<Value> graphPersons, ILogger logger)
        {
            logger.LogInformation("Combining data from sources...");

            List<Person> newElements = new List<Person>();   
            // Replace person data with graph data if person data is missing
            foreach (var person in persons)
            {
                var graphPerson = graphPersons?.FirstOrDefault(x => x.DisplayName == person.Name);

                if (graphPerson != null)
                {
                    // Replace person data with graph data if person data is missing
                    //if (person.Number != Convert.ToInt32(graphPerson.Id))
                    //{
                    //    person.Number = Convert.ToInt32(graphPerson.Id);
                    //    if (person.Number != 0)
                    //        logger.LogInformation($"Added number '{graphPerson.Id}' to person '{person.Name}'");
                    //}

                    person.Number = person.Number;
                    
                    person.Name ??= graphPerson.DisplayName;
                    if (person.Name != null)

                    person.Email ??= graphPerson.Mail;
                    if (person.Email != null)

                    person.Mobile ??= graphPerson.MobilePhone;
                    if (person.Mobile != null)

                    person.Title ??= graphPerson.JobTitle;
                    if (person.Title != null)

                    person.Address ??= graphPerson.OfficeLocation;
                    if (person.Address != null)

                    person.City ??= graphPerson.OfficeLocation;
                }
            }

            // Get all users from graph that are not in the persons list
            var newGraphPersons = graphPersons?.Where(x => !persons.Any(y => y.Name == x.DisplayName)).ToList();

            // Add new users to the persons list
            foreach (var newGraphPerson in newGraphPersons!)
            {
                newElements.Add(new Person
                {
                    Number = Guid.NewGuid().ToString(),
                    Name = newGraphPerson.DisplayName,
                    Email = newGraphPerson.Mail,
                    Mobile = newGraphPerson.MobilePhone,
                    Title = newGraphPerson.JobTitle,
                    Address = newGraphPerson.OfficeLocation,
                    City = newGraphPerson.OfficeLocation
                });
            }

            persons.DistinctBy(x => x.Number);

            // Write the updated data to a file
            File.WriteAllText("C:\\Users\\Filip\\Desktop\\Colibo.Assignment\\Colibo.Assignment\\Resources\\combiedData.json", JsonConvert.SerializeObject(persons, Formatting.Indented));

            // Put / Patch / Delete - HTTP endpoints
            // Implemention

            logger.LogInformation("Data combination complete.");
        }
    }
}
