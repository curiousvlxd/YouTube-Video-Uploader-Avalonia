using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using YouTube_Video_Uploader__cross_platform_.YouTube_API;

namespace YouTube_Video_Uploader.YouTubeAPI
{
public class State
{
    public State(YouTubeService service)
    {
        this.Status = Statuses.Succeed;
        this.Service = service;
    }

    public State(YouTubeService service, string videoID)
    {
        this.Status = Statuses.Succeed;
        this.Service = service;
        this.VideoID = videoID;
    }

    public State(State sourceState, string errorMessage)
    {
        this.Service = sourceState.Service;
        this.VideoID = sourceState.VideoID;
        this.Status = Statuses.Failed;
        this.Error = errorMessage;
    }

    public State(State sourceState, Exception exception)
    {
        if (sourceState == null)
        {
            this.Service = (YouTubeService)null;
            this.VideoID = string.Empty;
        }
        else
        {
            this.Service = sourceState.Service;
            this.VideoID = sourceState.VideoID ?? string.Empty;
        }
        this.Status = Statuses.Failed;
        this.Error = exception.ToString();
    }

    public YouTubeService Service { get; private set; }

    public string VideoID { get; private set; } = string.Empty;

    public Statuses Status { get; private set; }

    public string Error { get; private set; } = string.Empty;
}
}
