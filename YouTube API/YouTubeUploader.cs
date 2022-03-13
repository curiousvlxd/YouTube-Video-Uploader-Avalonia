using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTube_Video_Uploader.YouTubeAPI;

namespace YouTube_Video_Uploader__cross_platform_.YouTube_API
{
 public static class YoutubeUploader
{
    public static async Task<State> Authorize(this Credentials credentials)
    {
        try
        {
            ClientSecrets clientSecrets;
            if (!string.IsNullOrEmpty(credentials.SecretsPath))
            {
                using (FileStream fileStream = new FileStream(credentials.SecretsPath, FileMode.Open, FileAccess.Read))
                    clientSecrets = GoogleClientSecrets.FromStream((Stream)fileStream).Secrets;
            }
            else if (!string.IsNullOrEmpty(credentials.SecretJSON))
            {
                using (MemoryStream memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(credentials.SecretJSON)))
                    clientSecrets = GoogleClientSecrets.FromStream((Stream)memoryStream).Secrets;
            }
            else
            {
                if (string.IsNullOrEmpty(credentials.ClientID) || string.IsNullOrEmpty(credentials.ClientSecret))
                    return new State((State)null, "Secret is not found! Please, provide JSON, or path to the secret file, or Client ID and Client Secret values!");
                clientSecrets = new ClientSecrets()
                {
                    ClientId = credentials.ClientID,
                    ClientSecret = credentials.ClientSecret
                };
            }
            UserCredential userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, (IEnumerable<string>)new string[1]
            {
          YouTubeService.Scope.YoutubeUpload
            }, credentials.ChannelID, CancellationToken.None, (IDataStore)new FileDataStore(credentials.AuthorizationDataStorePath));
            return new State(new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = (IConfigurableHttpClientInitializer)userCredential,
                ApplicationName = credentials.ApplicationName
            }));
        }
        catch (Exception ex)
        {
            return new State((State)null, ex);
        }
    }

    public static async Task<State> Upload(this Task<State> stateTask, Video video)
    {
        State state = (State)null;
        try
        {
            return await (state = await stateTask).Upload(video);
        }
        catch (Exception ex)
        {
            return new State(state, ex);
        }
    }

    public static async Task<State> Upload(this State state, Video video1)
    {
        try
        {
            if (state.Service == null)
                return new State(state, "'YouTubeService' is not available. Please, perform Authorization first.");
            Google.Apis.YouTube.v3.Data.Video body = new Google.Apis.YouTube.v3.Data.Video()
            {
                Snippet = new VideoSnippet()
                {
                    Title = video1.Title,
                    Description = video1.Description,
                    CategoryId = ((int)video1.Category).ToString(),
                    Tags = (IList<string>)video1.Tags
                },
                Status = new VideoStatus()
                {
                    PrivacyStatus = video1.Privacy.ToString().ToLower()
                }
            };
            string videoId = string.Empty;
            Statuses uploadStatus = Statuses.Failed;
            Exception exception = (Exception)null;
            using (FileStream fileStream = new FileStream(video1.VideoPath, FileMode.Open))
            {
                VideosResource.InsertMediaUpload insertMediaUpload = state.Service.Videos.Insert(body, (Repeatable<string>)"snippet,status", (Stream)fileStream, "video/*");
                insertMediaUpload.ProgressChanged += (Action<IUploadProgress>)(progress =>
                {
                    if (progress.Status == UploadStatus.Completed)
                    {
                        uploadStatus = Statuses.Succeed;
                    }
                    else
                    {
                        if (progress.Status != UploadStatus.Failed)
                            return;
                        uploadStatus = Statuses.Failed;
                        exception = progress.Exception;
                    }
                });
                insertMediaUpload.ResponseReceived += (Action<Google.Apis.YouTube.v3.Data.Video>)(video6 => videoId = video6.Id);
                IUploadProgress uploadProgress = await insertMediaUpload.UploadAsync();
            }
            return uploadStatus == Statuses.Succeed ? new State(state.Service, videoId) : new State(state, exception != null ? exception.Message : "Video upload failed!");
        }
        catch (Exception ex)
        {
            return new State(state, ex);
        }
    }

    public static async Task<State> Upload(this Task<State> stateTask, Thumbnail thumbnail)
    {
        State state = (State)null;
        try
        {
            return await (state = await stateTask).Upload(thumbnail);
        }
        catch (Exception ex)
        {
            return new State(state, ex);
        }
    }

    public static async Task<State> Upload(this State state, Thumbnail thumbnail)
    {
        try
        {
            if (state.Service == null)
                return new State(state, "'YouTubeService' is not available. Please, perform Authorization first.");
            string empty = string.Empty;
            string videoId;
            if (!string.IsNullOrEmpty(thumbnail.VideoID))
            {
                videoId = thumbnail.VideoID;
            }
            else
            {
                if (string.IsNullOrEmpty(state.VideoID))
                    return new State(state, "Not possible to upload thumbnail because VideoID was not provided!");
                videoId = state.VideoID;
            }
            Statuses uploadStatus = Statuses.Failed;
            Exception exception = (Exception)null;
            using (FileStream fileStream = new FileStream(thumbnail.ThumbnailPath, FileMode.Open))
            {
                ThumbnailsResource.SetMediaUpload setMediaUpload = state.Service.Thumbnails.Set(videoId, (Stream)fileStream, string.Empty);
                setMediaUpload.ProgressChanged += (Action<IUploadProgress>)(progress =>
                {
                    if (progress.Status == UploadStatus.Completed)
                    {
                        uploadStatus = Statuses.Succeed;
                    }
                    else
                    {
                        if (progress.Status != UploadStatus.Failed)
                            return;
                        uploadStatus = Statuses.Failed;
                        exception = progress.Exception;
                    }
                });
                IUploadProgress uploadProgress = await setMediaUpload.UploadAsync();
            }
            return uploadStatus == Statuses.Succeed ? new State(state.Service) : new State(state, exception != null ? exception.Message : "Thumbnail upload failed!");
        }
        catch (Exception ex)
        {
            return new State(state, ex);
        }
    }
}
}
