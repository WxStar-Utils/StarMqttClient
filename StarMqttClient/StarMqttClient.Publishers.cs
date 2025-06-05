using System.Text.Json;
using MQTTnet;
using StarMqttClient.Commands;
using WxStarManager.Models;

namespace StarMqttClient;

public partial class StarMqttClient 
{
    /// <summary>
    /// Publishes data files as a string and sends the correct command format for the Star model specified.
    /// </summary>
    /// <param name="starModel">Generic version of WeatherStar model.</param>
    /// <param name="starData">StarData object.</param>
    /// <param name="national">Setting to true sends the command to the national feed for the specified StarModel.</param>
    /// <param name="starUuid">Optional, when set publishes data command to a specific unit instead of globally.</param>
    public async Task PublishData(WxStarModel starModel, StarData starData, bool national, string? starUuid)
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
        
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.Serialize(starData))
            .Build();

        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
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
            StartTime = startTime.ToString("dd/MM/yyyy HH:mm:ss:00"),
            CueId = presentationId,
        };

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("wxstar/cues")
            .WithPayload(JsonSerializer.Serialize(runCommand))
            .Build();

        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }
}