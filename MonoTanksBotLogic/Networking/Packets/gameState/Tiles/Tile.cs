﻿using MonoTanksBotLogic.Enums;
using MonoTanksBotLogic.JsonConverters;
using Newtonsoft.Json;

namespace MonoTanksBotLogic.Models;

/// <summary>
/// Represents tile.
/// </summary>
/// <param name="IsVisible">Represents if tile is visible in fog of war.</param>
/// <param name="ZoneIndex">Represents zone index in witch tile is located.</param>
/// <param name="Entities">Represents entities in a tile.</param>

[JsonConverter(typeof(TileJsonConverter))]
public record class Tile(bool IsVisible, int? ZoneIndex, Tile.TileEntity[] Entities)
{
    /// <summary>
    /// Represents base tile entity.
    /// </summary>
    public abstract record class TileEntity;

    /// <summary>
    /// Represents wall tile entity.
    /// </summary>
    public record class Wall : TileEntity;

    /// <summary>
    /// Represents enemy tank tile entity.
    /// 
    /// </summary>
    /// <param name="Direction">Represents direction of an enemy tank.</param>
    /// <param name="OwnerId">Represents owner id of an enemy tank.</param>
    /// <param name="Turret">Represents turret of an enemy tank.</param>
    public record class EnemyTank(Direction Direction, string OwnerId, EnemyTurret Turret) : TileEntity;

    /// <summary>
    /// Represents own tank tile entity.
    /// </summary>
    /// <param name="Direction">Represents direction of player own tank.</param>
    /// <param name="Health">Represents health of player own tank.</param>
    /// <param name="OwnerId">Represents owner id of player own tank.</param>
    /// <param name="Turret">Represents turret of player own tank.</param>
    /// <param name="SecondaryItem">Represents secondary item of player own tank.</param>
    public record class OwnTank(Direction Direction, int Health, string OwnerId, OwnTurret Turret, SecondaryItemType? SecondaryItem) : TileEntity;

    /// <summary>
    /// Represents bullet tile entitiy.
    /// </summary>
    /// <param name="Direction">Represents bullet direction.</param>
    /// <param name="Id">Represents bullet id.</param>
    /// <param name="Speed">Represents bullet speed.</param>
    /// <param name="Type">Represents bullet type.</param>
    public record class Bullet(Direction Direction, int Id, double Speed, BulletType Type) : TileEntity;

    /// <summary>
    /// Represents item tile entitiy.
    /// </summary>
    /// <param name="ItemType">Represents secondary item type.</param>
    public record class Item(SecondaryItemType ItemType) : TileEntity;

    /// <summary>
    /// Represents laser tile entitiy.
    /// </summary>
    /// <param name="Id">Represents laser id.</param>
    /// <param name="Orientation">Represents laser orientation.</param>
    public record class Laser(int Id, LaserDirection Orientation) : TileEntity;

    /// <summary>
    /// Represents mine tile entitiy.
    /// </summary>
    /// <param name="Id">Represents mine id.</param>
    /// <param name="ExplosionRemainingTicks">Represents remaining ticks to explosion.</param>
    /// <remarks>
    /// <para>
    /// The value is <see langword="null"/>
    /// if the mine hasn't exploded yet.
    /// </para>
    /// </remarks>
    public record class Mine(int Id, int? ExplosionRemainingTicks) : TileEntity;
}
