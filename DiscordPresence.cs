using System;
using System.Text;
using DiscordRPC;

public static class DiscordPresence
{

    static DiscordRpcClient client = new DiscordRpcClient("475456035851599874", pipe: 0);

    public static void Initialize()
    {
        // == Subscribe to some events
        client.OnReady += (sender, msg) =>
        {
            Logger.Info("[DISCORD] Connected to discord with user {0}", msg.User.Username);
        };

        client.OnPresenceUpdate += (sender, msg) =>
        {
            Logger.Info("[DISCORD] Presence has been updated! ");
        };

        client.Initialize();
    }

    public static void Update(PresenceStatus? presence)
    {   
        string? state = presence?.args?.activity?.state;
        if(state is not null) state = (Encoding.UTF8.GetByteCount(state) <= 128) ? state : "";

        string? details = presence?.args?.activity?.details;
        if(details is not null) details = (Encoding.UTF8.GetByteCount(details) <= 128) ? details : "";

        DiscordRPC.Timestamps timestamps = new DiscordRPC.Timestamps();
        DateTime start = new DateTime(1970, 1, 1).AddSeconds(Convert.ToInt32(presence?.args?.activity?.timestamps?.start));
        DateTime end = new DateTime(1970, 1, 1).AddSeconds(Convert.ToInt32(presence?.args?.activity?.timestamps?.end));
        if(start.Year >= DateTime.Now.Year) timestamps.Start = start;
        if(end.Year >= DateTime.Now.Year) timestamps.End = end;

        DiscordRPC.Assets assets = new DiscordRPC.Assets();
        string? large_image = presence?.args?.activity?.assets?.large_image;
        if(large_image is not null) assets.LargeImageKey = (Encoding.UTF8.GetByteCount(large_image) <= 128) ? large_image : assets.LargeImageKey;

        string? small_image = presence?.args?.activity?.assets?.small_image;
        if(small_image is not null) assets.SmallImageKey = (Encoding.UTF8.GetByteCount(small_image) <= 128) ? small_image : assets.SmallImageKey;

        string? large_text = presence?.args?.activity?.assets?.large_text;
        if(large_text is not null) assets.LargeImageText = (Encoding.UTF8.GetByteCount(large_text) <= 128) ? large_text : assets.LargeImageText;

        string? small_text = presence?.args?.activity?.assets?.small_text;
        if(small_text is not null) assets.SmallImageText = (Encoding.UTF8.GetByteCount(small_text) <= 128) ? small_text : assets.SmallImageText;

        if(presence?.args?.activity?.details == "No menu")
        {
            client.ClearPresence();
            return;
        }

        client.SetPresence(new RichPresence()
        {
            Details = details,
            State = presence?.args?.activity?.state,
            Timestamps = timestamps,
            Assets = assets
        });
    }

    public static void Dispose()
    {
        client.Dispose();
    }
}

public class PresenceStatus
{
    public string? nonce {get; set;}
    public string? cmd {get; set;}
    public Arguments? args {get; set;}

    public PresenceStatus()
    {
        nonce = null;
        cmd = null;
        args = new Arguments() {
            pid = null,
            activity = new Activity() {
                state = null,
                details = null,
                timestamps = new TimeStamps() {
                    start = 0,
                    end = 0
                },
                assets = new Assets(){
                    large_image = null,
                    large_text = null,
                    small_image = null,
                    small_text = null
                },
                instance = false,
            },
        };
    }
}

public class Arguments
{
    public int? pid {get; set;}
    public Activity? activity {get; set;}
}

public class Activity
{
    public string? state {get; set;}
    public string? details {get; set;}
    public TimeStamps? timestamps {get; set;}
    public Assets? assets {get; set;}
    public bool? instance {get; set;}
}

public class Assets
{
    public string? large_image {get; set;}
    public string? large_text {get; set;}
    public string? small_image {get; set;}
    public string? small_text {get; set;}
}

public class TimeStamps
{
    public int start {get;set;}
    public int end {get; set;}
}