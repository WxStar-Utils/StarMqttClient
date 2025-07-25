using System.Text.Json;
using MQTTnet;
using StarMqttClient.Commands;
using WxStarManager.Models;

namespace StarMqttClient;

public partial class BrokerClient 
{
    /// <summary>
    /// Publishes data files as a string and sends the correct command format for the Star model specified.
    /// </summary>
    /// <param name="starModel">Generic version of WeatherStar model.</param>
    /// <param name="starData">StarData object.</param>
    /// <param name="national">Setting to true sends the command to the national feed for the specified StarModel.</param>
    /// <param name="priority">Determines whether to send the data over a receiver's priority message port.</param>
    /// <param name="starUuid">Optional, when set publishes data command to a specific unit instead of globally.</param>
    public async Task PublishData(WxStarModel starModel, StarData starData, bool national, bool priority, string? starUuid)
    {
        string topic;

        if (starUuid != null)
        {
            topic = "wxstar/data/" + starUuid;
        }
        else
        {
            switch (starModel)
            {
                case WxStarModel.IntelliStar:
                    topic = "wxstar/data" + (national ? "/national/" : "/") + "i1" + (priority ? "/priority" : "");
                    break;
                case WxStarModel.IntelliStar2:
                    topic = "wxstar/data" + (national ? "/national/" : "/") + "i2" + (priority ? "/priority" : "");
                    break;
                case WxStarModel.WeatherStarXl:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("MQTT Publishing commands only permit generic unit families.");
            }
            
        }
        
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.Serialize(starData))
            .Build();

        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }
    
    
    public async Task PublishDataBurst(WxStarModel starModel, List<StarData> starDataList, bool national, bool priority, string? starUuid)
    {
        if (starDataList.Count == 0)
        {
            return;
        }
        
        foreach (var data in starDataList)
        {
            await PublishData(starModel, data, national, priority, starUuid);

            await Task.Delay(500);      // Small delay to avoid flooding on receivers
        }
    }
    
    
    /// <summary>
    /// Publishes formatted WeatherStar commands.
    /// </summary>
    /// <param name="starModel">Generic version of WeatherStar model.</param>
    /// <param name="command">Command to run. {0} can be used as a substitute for a filename.</param>
    /// <param name="national">Setting to true sends the command to the national feed for the specified StarModel.</param>
    /// <param name="starUuid">Optional, when set publishes data command to a specific unit instead of globally.</param>
    public async Task PublishCommand(WxStarModel starModel, string command, bool national, string? starUuid)
    {
        string topic;

        if (starUuid != null)
        {
            topic = "wxstar/data/" + starUuid;
        }
        else
        {
            switch (starModel)
            {
                case WxStarModel.IntelliStar:
                    topic = "wxstar/data" + (national ? "/national/" : "/") + "i1";
                    break;
                case WxStarModel.IntelliStar2:
                    topic = "wxstar/data" + (national ? "/national/" : "/") + "i2";
                    break;
                case WxStarModel.WeatherStarXl:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("MQTT Publishing commands only permit generic unit families.");
            }
        }

        GenericCmd genericCmd  = new()
        {
            Command = command,
        };

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.Serialize(genericCmd))
            .Build();

        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }

    public async Task PublishCommandBurst(WxStarModel starModel, List<string> commands, bool national, string? starUuid)
    {
        if (commands.Count == 0)
        {
            return;
        }

        foreach (string command in commands)
        {
            await PublishCommand(starModel, command, national, starUuid);
        }
    }

    /// <summary>
    /// Publishes presentation cue information.
    /// </summary>
    /// <param name="cueList">List containing the individual unit cue information.</param>
    /// <param name="presentationId">The presentation ID, called later by a run presentation cue.</param>
    public async Task PublishLoadCue(List<LoadCue> cueList, string presentationId)
    {
        if (cueList.Count < 1)
            return;

        var cueCommandRoot = new LoadCueRoot()
        {
            CueId = presentationId,
            Cues = cueList
        };

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("wxstar/cues")
            .WithPayload(JsonSerializer.Serialize(cueCommandRoot))
            .Build();

        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }

    /// <summary>
    /// Publishes a run cue for the specified presentation ID
    /// </summary>
    /// <param name="presentationId">The presentation ID of a loaded presentation.</param>
    /// <param name="startTime">System timestamp to begin the presentation at.</param>
    public async Task PublishRunCue(string presentationId, DateTime startTime)
    {
        var runCommand = new RunCue()
        {
            StartTime = startTime.ToString("MM/dd/yyyy HH:mm:ss:00"),
            CueId = presentationId,
        };

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("wxstar/cues")
            .WithPayload(JsonSerializer.Serialize(runCommand))
            .Build();

        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }
}
