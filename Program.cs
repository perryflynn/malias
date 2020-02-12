using System;
using System.Linq;
using PerrysNetConsole;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Binder;
using NewJson = Newtonsoft.Json;
using CommandLine;
using System.Threading.Tasks;
using maliasmgr.Extensions;
using System.Text;
using System.Collections.Generic;

namespace maliasmgr
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Console output settings
            CoEx.ForcedBufferWidth = 100;

            // Load settings
            var config = LoadConfiguration();

            if (string.IsNullOrWhiteSpace(config.Provider))
            {
                CoEx.WriteLine($"No provider configured in '{GetConfigPath()}'");
                return 99;
            }

            // Load provider
            var provider = CreateProvider(config);

            if (provider == null)
            {
                CoEx.WriteLine("No valid provider was loaded.");
                return 99;
            }

            // Parse arguments
            int returnCode = 99;

            Parser.Default.ParseArguments<Args>(args).WithParsed(o =>
            {

                if (!string.IsNullOrWhiteSpace(o.CreateName))
                {
                    // Create a new alias
                    returnCode = CreateAlias(config, o, provider).WaitForValue<int>();
                }
                else if (o.List)
                {
                    // List existing aliases
                    returnCode = ListAliases(config, o, provider).WaitForValue<int>();
                }

            });

            return returnCode;
        }

        /// <summary>
        /// Get existing aliases
        /// </summary>
        /// <param name="config">Config</param>
        /// <param name="provider">Provider</param>
        /// <returns>Exit code</returns>
        private static async Task<IList<Data.Alias>> GetAliases(Data.MailiasConfig config, Data.IProvider provider)
        {
            return (await provider.GetAliases())
                .Where(a => a.LocalPart.StartsWith(config.Prefix) && a.Domain == config.MailDomain)
                .ToList();
        }

        /// <summary>
        /// Helper for a yes/no prompt
        /// </summary>
        /// <param name="question">Question to ask</param>
        /// <returns>true of yes was typed</returns>
        private static bool YesNo(string question)
        {
            var prompt = new Prompt()
            {
                AllowEmpty = false,
                Choices = new string[] { "y", "n" },
                Default = "n",
                Prefix = question,
                ValidateChoices = true
            };

            return prompt.DoPrompt() == "y";
        }

        /// <summary>
        /// Create an alias
        /// </summary>
        /// <param name="config">Config</param>
        /// <param name="args">CMD Args</param>
        /// <param name="provider">Provider</param>
        /// <returns>Exit code</returns>
        private static async Task<int> CreateAlias(Data.MailiasConfig config, Args args, Data.IProvider provider)
        {
            // Create a new random id
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();
            var random = new Random();
            while(builder.Length < config.UniqeIdLength)
            {
                var index = random.Next(0, chars.Length - 1);
                builder.Append(chars[index]);
            }

            // Build the email address
            var prefix = string.IsNullOrWhiteSpace(config.Prefix) ? "" : config.Prefix + ".";
            var key = builder.ToString();

            var mailAddress = $"{prefix}{args.CreateName}.{key}@{config.MailDomain}";

            // Check if the email address already exists
            var exists = (await GetAliases(config, provider))
                .FirstOrDefault(a => a.SourceAddress.StartsWith($"{prefix}{args.CreateName}."));

            if (exists != null)
            {
                if (!YesNo($"There is already an alias ({exists.SourceAddress}) with this name. Proceed?"))
                {
                    return 1;
                }
            }

            // Create the new alias
            var result = await provider.CreateAlias(mailAddress, config.TargetAddress);

            if (result == Data.CreateResult.Success || result == Data.CreateResult.AlreadyExists)
            {
                CoEx.WriteLine(mailAddress);
                return 0;
            }
            else
            {
                CoEx.WriteLine("Creation failed.");
                return 1;
            }
        }

        /// <summary>
        /// List existing aliases
        /// </summary>
        /// <param name="config">Config</param>
        /// <param name="args">CMD Args</param>
        /// <param name="provider">Provider</param>
        /// <returns>Exit Code</returns>
        private static async Task<int> ListAliases(Data.MailiasConfig config, Args args, Data.IProvider provider)
        {
            // Table header
            var header = new string[] { "Source", "Targets" };

            // Convert the alias items into table rows
            var aliases = (await GetAliases(config, provider))
                .Select(a => new string[] { a.SourceAddress, string.Join(", ", a.TargetAddresses) })
                .ToList();

            if(aliases.Count > 0)
            {
                // draw a table
                aliases.Insert(0, header);

                var rows = RowCollection.Create(aliases.ToArray());
                rows.Settings.Border.Enabled = true;

                CoEx.WriteLine("Existing Aliases");
                CoEx.WriteTable(rows);
                CoEx.WriteLine();

                return 0;
            }
            else
            {
                // no aliases found
                CoEx.WriteLine("No aliases found.");
                return 1;
            }
        }

        /// <summary>
        /// Default location for the configuration file
        /// </summary>
        /// <returns>Path of the configuration file</returns>
        private static string GetConfigPath()
        {
            var path = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(path, "malias.json");
        }

        /// <summary>
        /// Load the application configuration file
        /// ~/malias.json
        /// </summary>
        /// <returns>Deserialized configuration</returns>
        private static Data.MailiasConfig LoadConfiguration()
        {
            var configFile = GetConfigPath();

            // create empty config if the file does not exists
            if (!File.Exists(configFile))
            {
                var emptyConfig = new Data.MailiasConfig();
                var emptyConfigText = NewJson.JsonConvert.SerializeObject(emptyConfig, NewJson.Formatting.Indented);
                File.WriteAllText(configFile, emptyConfigText);
            }

            // Parse config
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(configFile, false, false)
                .Build();

            return configBuilder.Get<Data.MailiasConfig>();
        }

        /// <summary>
        /// Create a provider instance
        /// </summary>
        /// <param name="config">Config</param>
        /// <returns>The created provider</returns>
        private static Data.IProvider CreateProvider(Data.MailiasConfig config)
        {
            Data.IProvider provider = null;

            if (config.Provider == "AllInkl")
            {
                provider = new Provider.AllInkl.AllInklProvider();
            }

            if (provider != null)
            {
                provider.Configure(config);
                return provider;
            }

            return null;
        }
    }
}
