using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Il2Cpp;
using MelonLoader;
using ShalazamPlugin.Extensions;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.SDK;

public class ShalazamWebsocketClient : IShalazamClient
{
    private readonly ConcurrentDictionary<uint, string> _ongoingRequests;
    private ClientWebSocket _ws = new();
    private Task? _receiveTask;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly Uri _endpoint = new("wss://shalazam.info/api/v1/client");
    private readonly string _apiKey;
    private const int ReconnectDelayMs = 5000;

    public event Func<string, Task>? MessageReceived;
    public event Func<WebSocketCloseStatus?, string?, Task>? Disconnected;

    private string[] _roles = Array.Empty<string>();
    public string? Username { get; private set; }

    public ShalazamWebsocketClient(string apiKey, CancellationToken cancellationToken = default)
    {
        _apiKey = apiKey;
        _ws.Options.SetRequestHeader("Authorization", apiKey);
        _ws.Options.SetRequestHeader("X-Plugin-Version", ModMain.PluginVersion);
        _ws.ConnectAsync(_endpoint, CancellationToken.None).GetAwaiter().GetResult();

        // start background receive loop
        MessageReceived += OnMessageReceived;
        Disconnected += OnDisconnect;
        _receiveTask = Task.Run(() => ReceiveLoop(cancellationToken), cancellationToken);

        _ongoingRequests = new ConcurrentDictionary<uint, string>();
    }

    private static Task OnDisconnect(WebSocketCloseStatus? closeStatus, string? disconnectStatusMessage)
    {
        MelonLogger.Warning($"Oh no we disconnected :( Status {closeStatus}, arg2 {disconnectStatusMessage}");

        return Task.CompletedTask;
    }

    public void PostResourceLocation(NetworkWorldItem networkWorldItem)
    {
        if (!_roles.Contains(Permissions.CreateResource))
        {
            return;
        }

        PostRequest(networkWorldItem.ToPostResourcePayload());
    }

    public void PostWorldItemLocation(NetworkWorldItem networkWorldItem)
    {
        if (!_roles.Contains(Permissions.CreateLocation))
        {
            return;
        }

        PostRequest(networkWorldItem.ToPostLocationPayload());
    }

    public void PostMonster(EntityNpcGameObject entityNpcGameObject)
    {
        if (!_roles.Contains(Permissions.CreateMonster) || !_roles.Contains(Permissions.CreateMonsterLocation))
        {
            return;
        }

        PostRequest(entityNpcGameObject.ToMonsterPayload());
    }

    public void PostItem(Item item)
    {
        if (!_roles.Contains(Permissions.CreateItem))
        {
            return;
        }

        PostRequest(item.ToItemPayload());
    }

    public void PostAbility(AbilityData ability)
    {
        if (!_roles.Contains(Permissions.CreateAbility))
        {
            return;
        }

        PostRequest(ability.ToRequestPayload());
    }

    public void PostBuffs(IEnumerable<BuffData> buffs)
    {
        if (!_roles.Contains(Permissions.CreateBuff))
        {
            return;
        }

        foreach (var buff in buffs)
        {
            // Skip null and blank/padding entries (Id 0), which would all collide on the
            // Id-keyed request map and spam duplicate-request warnings.
            if (buff == null || buff.Id == 0)
            {
                continue;
            }

            PostRequest(buff.ToBuffPayload());
        }
    }

    public void PostNpc(EntityNpcGameObject entityNpcGameObject)
    {
        if (!_roles.Contains(Permissions.CreateNpc))
        {
            return;
        }

        PostRequest(entityNpcGameObject.ToNpcPayload());
    }

    public void PostNpcVendorItems(uint networkId, string npcName, IEnumerable<NpcVendorItemEntry> items)
    {
        if (!_roles.Contains(Permissions.CreateNpc))
        {
            return;
        }

        var payload = new NpcVendorItemsPayload
        {
            Id = networkId,
            Type = "npc-vendor-items",
            NpcVendorItems = new NpcVendorItemsBody
            {
                Id = HashHelper.StableHash(npcName),
                Data = new NpcVendorItemsData(npcName, items)
            }
        };

        PostRequest(payload);
    }

    public void PostMastery(MasteryPayload payload)
    {
        if (!_roles.Contains(Permissions.CreateMastery))
        {
            return;
        }

        PostRequest(payload);
    }

    public void PostDrops(EntityNpcGameObject entityNpcGameObject, bool isSkinning, IEnumerable<Item> itemsDropped)
    {
        if (!_roles.Contains(Permissions.CreateMonster))
        {
            return;
        }

        var validItems = itemsDropped.Where(x => x?.Template != null).ToList();
        if (validItems.Count == 0)
        {
            return;
        }

        var payload = new DropPayload
        {
            Drop = new DropBody
            {
                MonsterName = entityNpcGameObject.info.DisplayName,
                Source = isSkinning ? "skinning" : "kill",
                Items = validItems.Select(x => new ItemDrop
                {
                    Id = x.Template.ItemId,
                    Name = x.Template.ItemName
                })
            },
            Type = "drop"
        };

        PostRequest(payload);
    }

    private Task OnMessageReceived(string body)
    {
        var response = JsonSerializer.Deserialize<WebsocketPayload>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (response?.Type == "me")
        {
            var userInfo = JsonSerializer.Deserialize<UserPayload>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Username = userInfo.Me.Username;
            _roles = userInfo.Me.Permissions;
        }
        else
        {
            if (!_ongoingRequests.TryRemove(response.Id, out var requestBody))
            {
                MelonLogger.Error("Failed to remove ongoing request from dictionary. This shouldn't happen.");
                return Task.CompletedTask;
            }

            if (response.Type != "success")
            {
                MelonLogger.Error($"Request failed, response: {body}. Original request: {requestBody}");
            }
        }

        return Task.CompletedTask;
    }

    private void PostRequest<T>(T payload) where T : WebsocketPayload
    {
        payload.IsTestRealm = Globals.IsPtr;

        if (_ws.State != WebSocketState.Open)
        {
            MelonLogger.Warning("Websocket is not connected. Dropping request.");
            return;
        }

        var bodyString = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

        if (!_ongoingRequests.TryAdd(payload.Id, bodyString))
        {
            MelonLogger.Warning($"We tried to post a duplicate request. This may be a bug, please report it :) Request: {bodyString}");
        }

        _ws.SendAsync(Encoding.UTF8.GetBytes(bodyString), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await EnsureConnected(cancellationToken);
                    await ReceiveMessages(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    await HandleDisconnect(WebSocketCloseStatus.InternalServerError, ex.Message);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(ReconnectDelayMs, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal cancellation
        }
    }

    private async Task EnsureConnected(CancellationToken cancellationToken)
    {
        await _connectLock.WaitAsync(cancellationToken);

        try
        {
            if (_ws.State == WebSocketState.Open)
            {
                return;
            }

            try
            {
                _ws.Dispose();
            }
            catch
            {
                // ignore dispose exceptions
            }

            _ws = new ClientWebSocket();
            _ws.Options.SetRequestHeader("Authorization", _apiKey);
            _ws.Options.SetRequestHeader("X-Plugin-Version", ModMain.PluginVersion);

            MelonLogger.Msg("Connecting to Shalazam websocket...");
            await _ws.ConnectAsync(_endpoint, cancellationToken);
            MelonLogger.Msg("Connected to Shalazam websocket.");
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task ReceiveMessages(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        while (_ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            WebSocketReceiveResult result;

            try
            {
                result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await HandleDisconnect(WebSocketCloseStatus.InternalServerError, ex.Message);
                return;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await HandleDisconnect(_ws.CloseStatus, _ws.CloseStatusDescription);
                return;
            }

            var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
            if (MessageReceived != null)
            {
                await MessageReceived(msg);
            }
        }
    }

    private async Task HandleDisconnect(WebSocketCloseStatus? closeStatus, string? disconnectStatusMessage)
    {
        Username = null;
        _roles = Array.Empty<string>();
        _ongoingRequests.Clear();

        if (Disconnected != null)
        {
            await Disconnected(closeStatus, disconnectStatusMessage);
        }
    }
}
