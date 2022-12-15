class PresenceStatus
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
                    start = null,
                    end = null
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

class Arguments
{
    public int? pid {get; set;}
    public Activity? activity {get; set;}
}

class Activity
{
    public string? state {get; set;}
    public string? details {get; set;}
    public TimeStamps? timestamps {get; set;}
    public Assets? assets {get; set;}
    public bool? instance {get; set;}
}

class Assets
{
    public string? large_image {get; set;}
    public string? large_text {get; set;}
    public string? small_image {get; set;}
    public string? small_text {get; set;}
}

class TimeStamps
{
    public int? start {get; set;}
    public int? end {get; set;}
}