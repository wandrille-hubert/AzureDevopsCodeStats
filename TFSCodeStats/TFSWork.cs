using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TFSCodeStats.Models;

namespace TFSCodeStats
{
    class TFSWork
    {
        public List<UserStat> userStats = new List<UserStat>();
        string personalaccesstoken;
        public TFSWork(string url, string pat)
        {
            // Initialize connection to azure devops
            personalaccesstoken = pat;
            var networkCredential = new VssBasicCredential(string.Empty, pat);
            VssConnection connection = new VssConnection(new Uri(url), networkCredential);

            // Initialize ProjectHttpClient and GitHttpClient
            var projclient = connection.GetClient<ProjectHttpClient>();
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();

            // Get list of projects credentials have access to
            var projcollection = projclient.GetProjects().Result;

            foreach (var proj in projcollection)
            {
                // Get list of repos for a project
                List<GitRepository> repos = gitClient.GetRepositoriesAsync(proj.Id.ToString()).Result;

                // Set commits query criteria
                GitQueryCommitsCriteria criteria = new GitQueryCommitsCriteria()
                {
                    // Add criterias in here, some examples: ToDate, FromDate, Top, FromCommitId
                };

                foreach (GitRepository repo in repos)
                {
                    // Get list of commits for a repo
                    List<GitCommitRef> commits = gitClient.GetCommitsAsync(repo.Id, criteria).Result.OrderBy(x => x.Committer.Date).ToList();

                    for (int i = 0; i < commits.Count; i++)
                    {
                        // ASSUMPTION: i=0 is first commit and no code/files in this commit
                        // TODO: update to handle all cases
                        if (i != 0)
                        {
                            // Get list of changes for a commit
                            GitCommitChanges changes = gitClient.GetChangesAsync(commits[i].CommitId, repo.Id).Result;

                            foreach (var change in changes.Changes)
                            {
                                // Only will collect stats pertaining to *.cs files
                                if (change.Item.Path.EndsWith(".cs"))
                                {
                                    // Collect file stats
                                    var obj = userStats.FirstOrDefault(x => x.email == commits[i].Committer.Email && x.projectName == proj.Name && x.repoName == repo.Name);
                                    if (obj != null)
                                    {
                                        if (change.ChangeType == VersionControlChangeType.Add)
                                            obj.filesAdded = obj.filesAdded + 1;
                                        else if (change.ChangeType == VersionControlChangeType.Delete)
                                            obj.filesDeleted = obj.filesDeleted + 1;
                                        else if (change.ChangeType == VersionControlChangeType.Edit)
                                            obj.filesModified = obj.filesModified + 1;
                                    }
                                    else
                                    {
                                        UserStat user = new UserStat();
                                        user.projectName = proj.Name;
                                        user.repoName = repo.Name;
                                        user.email = commits[i].Committer.Email;
                                        user.name = commits[i].Committer.Name;
                                        if (change.ChangeType == VersionControlChangeType.Add)
                                            user.filesAdded = user.filesAdded + 1;
                                        else if (change.ChangeType == VersionControlChangeType.Delete)
                                            user.filesDeleted = user.filesDeleted + 1;
                                        else if (change.ChangeType == VersionControlChangeType.Edit)
                                            user.filesModified = user.filesModified + 1;
                                        userStats.Add(user);
                                    }

                                    // Collect code stats
                                    ChurnFileStats(url, proj.Name, repo.Id.ToString(), repo.Name, change, pat, commits[i].CommitId, commits[i - 1].CommitId, commits[i].Committer.Email, commits[i].Committer.Name);
                                }
                            }
                        }
                    }

                }
            }
        }

        async void ChurnFileStats(string url, string projName, string repoId, string repoName, GitChange gitChange, string pat, string currentcommit, string previouscommit, string email, string name)
        {
            string valuesjson = "";

            // Different valuesjson created based on whether file exists in currentcommit and previouscommit
            if (gitChange.Item.OriginalObjectId == null)
            {
                var values = new Dictionary<string, string>
                {
                    { "originalVersion", "GC"+previouscommit },
                    { "modifiedPath", gitChange.Item.Path },
                    { "modifiedVersion", "GC"+currentcommit },
                    { "partialDiff", "true" },
                    { "includeCharDiffs", "true" }
                };

                valuesjson = JsonConvert.SerializeObject(values, Formatting.Indented);
            }
            else if (gitChange.Item.ObjectId == null || gitChange.Item.ObjectId == "")
            {
                var values = new Dictionary<string, string>
                {
                    { "originalPath", gitChange.Item.Path },
                    { "originalVersion", "GC"+previouscommit },
                    { "modifiedVersion", "GC"+currentcommit },
                    { "partialDiff", "true" },
                    { "includeCharDiffs", "true" }
                };

                valuesjson = JsonConvert.SerializeObject(values, Formatting.Indented);
            }
            else
            {
                var values = new Dictionary<string, string>
                {
                    { "originalPath", gitChange.Item.Path },
                    { "originalVersion", "GC"+previouscommit },
                    { "modifiedPath", gitChange.Item.Path },
                    { "modifiedVersion", "GC"+currentcommit },
                    { "partialDiff", "true" },
                    { "includeCharDiffs", "true" }
                };
                var content = new FormUrlEncodedContent(values);
                valuesjson = JsonConvert.SerializeObject(values, Formatting.Indented);
            }
            
            string endpoint = url + projName + "/_api/_versioncontrol/fileDiff?__v=5&diffParameters=" + valuesjson + "&repositoryId=" + repoId;


            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", pat))));

                using (HttpResponseMessage response = client.GetAsync(endpoint).Result)
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Read response into a FileDiff object
                        var result = JsonConvert.DeserializeObject<FileDiff>(responseBody);

                        if (result != null)
                        {
                            foreach (var block in result.blocks)
                            {
                                if (block.changeType == 1)
                                {
                                    var diff = block.mLinesCount - block.oLinesCount;

                                    var obj = userStats.FirstOrDefault(x => x.email == email && x.projectName == projName && x.repoName == repoName);
                                    if (obj != null)
                                    {
                                        obj.linesAdded = obj.linesAdded + diff;
                                    }
                                    else
                                    {
                                        UserStat user = new UserStat();
                                        user.projectName = projName;
                                        user.repoName = repoName;
                                        user.email = email;
                                        user.name = name;
                                        user.linesAdded = diff;
                                        userStats.Add(user);
                                    }
                                }
                                else if (block.changeType == 2)
                                {
                                    var diff = block.oLinesCount - block.mLinesCount;

                                    var obj = userStats.FirstOrDefault(x => x.email == email && x.projectName == projName && x.repoName == repoName);
                                    if (obj != null)
                                    {
                                        obj.linesRemoved = obj.linesRemoved + diff;
                                    }
                                    else
                                    {
                                        UserStat user = new UserStat();
                                        user.projectName = projName;
                                        user.repoName = repoName;
                                        user.email = email;
                                        user.name = name;
                                        user.linesRemoved = diff;
                                        userStats.Add(user);
                                    }
                                }
                                else if (block.changeType == 3)
                                {
                                    var obj = userStats.FirstOrDefault(x => x.email == email && x.projectName == projName && x.repoName == repoName);
                                    if (obj != null)
                                    {
                                        obj.linesModified = obj.linesModified + block.mLinesCount;
                                    }
                                    else
                                    {
                                        UserStat user = new UserStat();
                                        user.projectName = projName;
                                        user.repoName = repoName;
                                        user.email = email;
                                        user.name = name;
                                        user.linesModified = block.mLinesCount;
                                        userStats.Add(user);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // An error occured, one possibility is that a non success status code returned
                    }
                }
            }
        }
    }
}
