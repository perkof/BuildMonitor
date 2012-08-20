using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using Delcom804002_BDriver;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;

namespace BuildMonitor
{
    class Program
    {
        private static string teamProjectName = "tfsProject";
        private static string projectCollectionName = "tfsCollection";
        private static string tfsServer = "http://tfsserver.com/tfs";

        static void Main(string[] args)
        {

            TfsConfigurationServer configurationServer = ConnectToTfs();
            LightDevice light;

            try
            {
                light = new LightDevice();
            } catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                return;
            }
            
            var collection = GetProjectCollection(configurationServer);
           
            if (collection != null)
            {
                while (true)
                {
                    var latestBuild = GetBuildStatus(collection);
                    Console.WriteLine("Build Status: {0}", latestBuild);

                    light.TurnOffLight(LightColour.Red);
                    light.TurnOffLight(LightColour.Green);
                    light.TurnOffLight(LightColour.Orange);

                    if(latestBuild == BuildStatus.InProgress)
                    {
                        light.TurnOnLight(LightColour.Orange);
                    }
                    else if (latestBuild == BuildStatus.Succeeded)
                    {
                        light.TurnOnLight(LightColour.Green);
                    }
                    else
                    {
                        light.TurnOnLight(LightColour.Red);
                    }
                    Thread.Sleep(10000);
                }
            } else
            {
                Console.WriteLine("Could not connect to Project Collection");
            }

        }

        private static BuildStatus GetBuildStatus(TfsTeamProjectCollection collection)
        {
            var buildServer = collection.GetService<IBuildServer>();
            
            var builds = buildServer.QueryBuilds(teamProjectName);
            return builds.OrderByDescending(b => b.StartTime).First().Status;
        }

        private static TfsConfigurationServer ConnectToTfs()
        {
            Uri tfsUri = new Uri(tfsServer);
            TfsConfigurationServer configurationServer =
                TfsConfigurationServerFactory.GetConfigurationServer(tfsUri);

            return configurationServer;
        }

        private static TfsTeamProjectCollection GetProjectCollection(TfsConfigurationServer configurationServer)
        {
            // Get the catalog of team project collections
            ReadOnlyCollection<CatalogNode> collectionNodes = configurationServer.CatalogNode.QueryChildren(
                new[] { CatalogResourceTypes.ProjectCollection },
                false, CatalogQueryOptions.None);

            foreach (CatalogNode collectionNode in collectionNodes)
            {
                if (collectionNode.Resource.DisplayName == projectCollectionName)
                {
                    Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
                    return configurationServer.GetTeamProjectCollection(collectionId);
                }
            }

            return null;
        }
    }
}
