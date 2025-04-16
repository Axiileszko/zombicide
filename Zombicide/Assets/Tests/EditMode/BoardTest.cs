using Assets.Persistence;
using Model;
using NUnit.Framework;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
public class BoardTest
{
    private IMapLoader _mapLoader = new DummyMapLoader();

    [Test]
    public void GetStreetByTilesTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Model.Board.Street street0 = model.Board.GetStreetByTiles(13, 20); 
        Model.Board.Street street1 = model.Board.GetStreetByTiles(22, 20); 
        Model.Board.Street street2 = model.Board.GetStreetByTiles(13, 22);
        Assert.IsNull(street2);
        Assert.AreEqual(5, street0.Tiles.Count);
        Assert.AreEqual(5, street1.Tiles.Count);
    }
    [Test]
    public void GetBuildingByTileTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Model.Board.Building building0 = model.Board.GetBuildingByTile(0);
        Model.Board.Building building1 = model.Board.GetBuildingByTile(7);
        Model.Board.Building building2 = model.Board.GetBuildingByTile(13);
        Assert.IsNull(building2);
        Assert.AreEqual(3, building0.Rooms.Count);
        Assert.AreEqual(2, building0.Rooms.Count(x => x.Type == Model.Board.TileType.DARKROOM));
        Assert.AreEqual(5, building1.Rooms.Count);
        Assert.AreEqual(2, building1.Rooms.Count(x => x.Type == Model.Board.TileType.DARKROOM));
    }
    [Test]
    public void GetTileByIDTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        var tile12 = model.Board.GetTileByID(12);
        var tile4 = model.Board.GetTileByID(4);
        var tile24 = model.Board.GetTileByID(24);
        var tile7 = model.Board.GetTileByID(7);
        var tile14 = model.Board.GetTileByID(14);
        Assert.IsNotNull(tile12.SpawnType);
        Assert.IsTrue(tile12.Type == Model.Board.TileType.STREET);
        Assert.AreEqual(2, tile12.Neighbours.Count);
        Assert.IsNotNull(tile4.PimpWeapon);
        Assert.IsNotNull(tile4.Objective);
        Assert.IsTrue(tile4.Type == Model.Board.TileType.ROOM);
        Assert.AreEqual(2, tile4.Neighbours.Count);
        Assert.IsTrue(tile24.Type == Model.Board.TileType.STREET);
        Assert.AreEqual(1, tile24.Neighbours.Count);
        Assert.IsNotNull(tile7.PimpWeapon);
        Assert.IsTrue(tile7.Type == Model.Board.TileType.DARKROOM);
        Assert.AreEqual(2, tile7.Neighbours.Count);
        Assert.IsTrue(tile14.IsStart);
        Assert.IsTrue(tile14.Type == Model.Board.TileType.STREET);
        Assert.AreEqual(3, tile14.Neighbours.Count);
    }
    [Test]
    public void CanMoveTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.IsTrue(model.Board.CanMove(model.Board.GetTileByID(14), model.Board.GetTileByID(17)));
        Assert.IsTrue(model.Board.CanMove(model.Board.GetTileByID(0), model.Board.GetTileByID(1)));
        Assert.IsFalse(model.Board.CanMove(model.Board.GetTileByID(17), model.Board.GetTileByID(4)));
        var tile17 = model.Board.GetTileByID(17);
        tile17.OpenDoor(tile17.Neighbours.First(x => x.Destination.Id == 4), ItemFactory.GetGenericWeaponByName(ItemName.AXE));
        Assert.IsTrue(model.Board.CanMove(model.Board.GetTileByID(17), model.Board.GetTileByID(4)));
    }
    [Test]
    public void GetShortestPathTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.AreEqual(3, Model.Board.Board.GetShortestPath(model.Board.GetTileByID(17), model.Board.GetTileByID(12)));
        Assert.AreEqual(-1, Model.Board.Board.GetShortestPath(model.Board.GetTileByID(0), model.Board.GetTileByID(10)));
        Assert.AreEqual(3, Model.Board.Board.GetShortestPath(model.Board.GetTileByID(6), model.Board.GetTileByID(9)));
        Assert.AreEqual(2, Model.Board.Board.GetShortestPath(model.Board.GetTileByID(14), model.Board.GetTileByID(16)));
        var tile17 = model.Board.GetTileByID(17);
        var tile16 = model.Board.GetTileByID(16);
        tile17.OpenDoor(tile17.Neighbours.First(x => x.Destination.Id == 4), ItemFactory.GetGenericWeaponByName(ItemName.AXE));
        tile16.OpenDoor(tile16.Neighbours.First(x => x.Destination.Id == 3), ItemFactory.GetGenericWeaponByName(ItemName.AXE));
        Assert.AreEqual(2, Model.Board.Board.GetShortestPath(model.Board.GetTileByID(4), model.Board.GetTileByID(14)));
        Assert.AreEqual(3, Model.Board.Board.GetShortestPath(model.Board.GetTileByID(4), model.Board.GetTileByID(21)));
    }
}
