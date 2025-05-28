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
    /// <param name="data">Star data file as a string.</param>
    /// <param name="command">Command to run. {0} can be used as a substitute for a filename.</param>
    /// <param name="national">Setting to true sends the command to the national feed for the specified StarModel.</param>
    /// <param name="starUuid">Optional, when set publishes data command to a specific unit instead of globally.</param>
    public async Task PublishData(WxStarModel starModel, string data, string command, bool national, string? starUuid)
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

        StarData dataCommand = new()
        {
            Command = command,
            Data = data
        };

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.Serialize(dataCommand))
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
    /// <param name="starModel">Generic version of WeatherStar model.</param>
    /// <param name="cueList">List containing the individual unit cue information.</param>
    /// <param name="presentationId">The presentation ID, called later by a run presentation cue.</param>
    public async Task PublishLoadCue(WxStarModel starModel, List<LoadCue> cueList, string presentationId)
    {
        string topic;

        switch (starModel)
        {
            case WxStarModel.IntelliStar:
                topic = "wxstar/cues/i1";
                break;
            case WxStarModel.IntelliStar2:
                topic = "wxstar/cues/i2";
                break;
            case WxStarModel.WeatherStarXl:
                throw new NotImplementedException();
            default:
                throw new ArgumentException("MQTT publishing commands only permit generic unit families.");
        }

        if (cueList.Count < 1)
            return;

        var cueCommandRoot = new LoadCueRoot()
        {
            CueId = presentationId,
            Cues = cueList
        };

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.Serialize(cueCommandRoot))
            .Build();

        await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }
}