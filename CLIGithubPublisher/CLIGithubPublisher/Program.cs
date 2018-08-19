using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CLIGithubPublisher
{
    class Program
    {
        private static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {


            if (args.Length == 10)
            {
                ReleaseJSON release = new ReleaseJSON();

                string filepath = null;
                string token = null;
                string repo_owner = null;
                string repo = null;




                foreach (string arg in args)
                {
                    if (arg.Contains("--git-token="))
                    {
                        token = arg.Split(new string[] { "--git-token=" }, StringSplitOptions.None)[1];
                    }
                    else if(arg.Contains("--repo-owner="))
                    {
                        repo_owner = arg.Split(new string[] { "--repo-owner=" }, StringSplitOptions.None)[1];
                    }
                    else if (arg.Contains("--repo="))
                    {
                        repo = arg.Split(new string[] { "--repo=" }, StringSplitOptions.None)[1];
                    }
                    else if(arg.Contains("--tag-name="))
                    {
                        release.tag_name = arg.Split(new string[] { "--tag-name=" }, StringSplitOptions.None)[1];
                    }
                    else if (arg.Contains("--target_commitish="))
                    {
                        release.target_commitish = arg.Split(new string[] { "--target_commitish=" }, StringSplitOptions.None)[1];
                    }
                    else if (arg.Contains("--name="))
                    {
                        release.name = arg.Split(new string[] { "--name=" }, StringSplitOptions.None)[1];
                    }
                    else if (arg.Contains("--body="))
                    {
                        release.body = arg.Split(new string[] { "--body=" }, StringSplitOptions.None)[1];
                    }
                    else if (arg.Contains("--draft="))
                    {
                        if (arg.Split(new string[] { "--draft=" }, StringSplitOptions.None)[1] == "true")
                        {
                            release.draft = true;
                        }
                        else
                        {
                            release.draft = false;
                        }
                    }
                    else if (arg.Contains("--prerelease="))
                    {
                        if (arg.Split(new string[] { "--prerelease=" }, StringSplitOptions.None)[1] == "true")
                        {
                            release.prerelease = true;
                        }
                        else
                        {
                            release.prerelease = false;
                        }
                    }
                    else if (arg.Contains("--file="))
                    {
                        filepath = arg.Split(new string[] { "--file=" }, StringSplitOptions.None)[1];
                    }
                }


                Console.WriteLine(JObject.FromObject(release).ToString(Formatting.Indented));


                bool success = PublishRelease(release, token, repo, repo_owner, filepath);
                Console.Write("Finished!");

            }
            else
            {
                if (args.Length >= 1)
                {
                    if (args[0] == "--help")
                    {
                        Console.WriteLine("Welcome to CLIGithubPublisher");
                        Console.WriteLine("Use the following arguments (all must be used):");
                        Console.WriteLine("--git-token=\"github token of the owner\"  = github token from/of the owner from the repo with push access.");
                        Console.WriteLine("--repo-owner=\"the_owner_of_the_repo\"  = owner of the repo, the repo url looks like https://github.com/RepoOwner/Repo");
                        Console.WriteLine("--repo=\"the repo name\"  = the repo to publish to, the repo url looks like https://github.com/RepoOwner/Repo");
                        Console.WriteLine("--tag-name=\"v0.0.0\"  = Use a tagname, version is most common and accepted.");
                        Console.WriteLine("--target_commitish=\"master\"  = Define source branch.");
                        Console.WriteLine("--name=\"My Application Name\"  = Define application name.");
                        Console.WriteLine("--body=\"This is my application, write some info here\"  = Define body of release.");
                        Console.WriteLine("--draft=true||false = Set if release is draft or not.");
                        Console.WriteLine("--prerelease=true||false = Set if release is prerelease or not.");
                        Console.WriteLine("--file=\"Path/To/File\" = file to be released.");

                    }
                }
                else
                {
                    Console.WriteLine("Error: Your missing arguments, type --help for help.");
                }
            }   
            
        }

        private static bool PublishRelease(ReleaseJSON release, string token, string repo, string repo_owner, string filepath) {

            client.DefaultRequestHeaders.Add("User-Agent", "CLIGithubPublisher");

            Console.WriteLine("Currently inititaiting connection with:");
            Console.WriteLine("https://api.github.com/repos/" + repo_owner + "/" + repo + "/releases");

            HttpResponseMessage message = client.PostAsync("https://api.github.com/repos/" + repo_owner + "/" + repo + "/releases?access_token=" + token, new StringContent(JObject.FromObject(release).ToString(), Encoding.UTF8, "application/json")).Result;


            if (message.IsSuccessStatusCode)
            {
                client.Dispose();

                // client = new HttpClient();
                // client.Timeout = TimeSpan.FromMinutes(10);
                //client.DefaultRequestHeaders.Add("User-Agent", "CLIGithubPublisher");


                HttpContent content = message.Content;

                string jsonResponse = content.ReadAsStringAsync().Result;

                JObject responseBody = JObject.Parse(jsonResponse);

                string assets_url = responseBody.Value<string>("upload_url").Split('{')[0];

                byte[] file = File.ReadAllBytes(filepath);


                Console.WriteLine("Start sending asset to: " + assets_url + "?name=" + Path.GetFileName(filepath));
                string responseInString = "404";
                using (var wb = new WebClient())
                {
                    wb.Headers.Add("User-Agent", "CLIGithubPublisher");
                    wb.Headers.Add("Content-Type", "application/zip");
                    var response = wb.UploadData(assets_url + "?name=" + Path.GetFileName(filepath) + "&access_token=" + token, "POST", file);

                    responseInString = Encoding.UTF8.GetString(response);
                }

                if (responseInString.Contains("201"))
                {
                    Console.WriteLine("Succesfully published release!");
                    return true;

                }

                return false;
                    /*    HttpResponseMessage uploadstatus = client.PostAsync(assets_url + "?name=" + Path.GetFileName(filepath) + "&access_token=" + token, byteContent).Result;


                        HttpContent contentuploadstatus = message.Content;

                        string uploadstatusResponse = contentuploadstatus.ReadAsStringAsync().Result;

                        if (uploadstatus.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Succesfully published release!");

                            Console.WriteLine(uploadstatusResponse);

                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Failed publishing release with error: " + uploadstatus.StatusCode.ToString());

                            Console.WriteLine(uploadstatusResponse);

                            return false;
                        } */

                }
            else
            {
                HttpContent content = message.Content;

                string response = content.ReadAsStringAsync().Result;
                Console.WriteLine("Failed creating release ;( :");
                Console.WriteLine(response);

                return false;
            }
        }
    }
}
