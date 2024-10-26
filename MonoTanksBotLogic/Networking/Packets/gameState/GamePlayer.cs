﻿using MonoTanksBotLogic.JsonConverters;
using Newtonsoft.Json;

namespace MonoTanksBotLogic.Models;

/// <summary>
/// Represents game player.
/// </summary>
/// <param name="Id">Represents id of a game player.</param>
/// <param name="Nickname">Represents nickname of a game player.</param>
/// <param name="Color">Represents color of a game player.</param>
/// <param name="Ping">Represents ping of a game player.</param>
[JsonConverter(typeof(GamePlayerJsonConverter))]
public abstract record class GamePlayer(
    string Id,
    string Nickname,
    uint Color,
    int Ping);
