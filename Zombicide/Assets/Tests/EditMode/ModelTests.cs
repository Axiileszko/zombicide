using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Persistence;
using Model;
using Model.Board;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Persistence;
using UnityEngine;
using UnityEngine.TestTools;
using static UnityEngine.GraphicsBuffer;

public class ModelTests
{
    private IMapLoader _mapLoader = new DummyMapLoader();

    [Test]
    public void NumberOfPlayersOnTileTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.AreEqual(3, model.NumberOfPlayersOnTile(14));
        Survivor s = SurvivorFactory.GetSurvivorByName("Amy");
        s.MoveTo(model.Board.GetTileByID(17));
        Assert.AreEqual(2, model.NumberOfPlayersOnTile(14));
        Assert.AreEqual(1, model.NumberOfPlayersOnTile(17));
        Assert.AreEqual(0, model.NumberOfPlayersOnTile(13));
    }
    [Test]
    public void GetSurvivorByNameTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        string name = "Amy";
        Survivor s = SurvivorFactory.GetSurvivorByName(name);
        Assert.AreEqual(s, model.GetSurvivorByName(name));
        Assert.AreEqual(s.GetHashCode(), model.GetSurvivorByName(name).GetHashCode());
        Assert.AreEqual(null, model.GetSurvivorByName("None"));
    }
    [Test]
    public void GetZombiesInPriorityOrderOnTileTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        model.SpawnZombie(ZombieType.WALKER, 2, model.Board.GetTileByID(17), false);
        model.SpawnZombie(ZombieType.FATTY, 1, model.Board.GetTileByID(17), false);
        model.SpawnZombie(ZombieType.FATTY, 1, model.Board.GetTileByID(17), true);
        model.SpawnZombie(ZombieType.RUNNER, 1, model.Board.GetTileByID(17), true);
        List<Zombie> tile17=model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17));
        List<Zombie> tile14=model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(14));
        List<Zombie> tile13=model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(13));
        Assert.AreEqual(3, tile17.Count);
        Assert.AreEqual(2, tile14.Count);
        Assert.AreEqual(0, tile13.Count);
        Assert.IsTrue((FattyZombie)tile17[0] is FattyZombie);
        Assert.IsTrue((WalkerZombie)tile17[1] is WalkerZombie);
        Assert.IsTrue((WalkerZombie)tile17[2] is WalkerZombie);
        Assert.IsTrue((FattyZombie)tile14[0] is FattyZombie);
        Assert.IsTrue((RunnerZombie)tile14[1] is RunnerZombie);
    }
    [Test]
    public void GetSurvivorsOnTileTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        List<Survivor> tile14=model.GetSurvivorsOnTile(model.Board.GetTileByID(14));
        Assert.AreEqual("Amy", tile14[0].Name);
        Assert.AreEqual("Wanda", tile14[1].Name);
        Assert.AreEqual("Ned", tile14[2].Name);
        Survivor s = SurvivorFactory.GetSurvivorByName("Amy");
        s.MoveTo(model.Board.GetTileByID(17));
        List<Survivor> tile17 = model.GetSurvivorsOnTile(model.Board.GetTileByID(17));
        tile14 = model.GetSurvivorsOnTile(model.Board.GetTileByID(14));
        Assert.AreEqual("Amy", tile17[0].Name);
        Assert.AreEqual("Wanda", tile14[0].Name);
        Assert.AreEqual("Ned", tile14[1].Name);
    }
    [Test]
    public void StartGameTest()
    {
        GameModel model = new GameModel(_mapLoader);
        Assert.AreEqual(1, model.SpawnCount);
        Assert.IsNull(model.StartTile);
        Assert.IsNull(model.Board);
        Assert.AreEqual(0, model.SurvivorLocations.Count);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.AreEqual(3, model.SpawnCount);
        Assert.IsNotNull(model.StartTile);
        Assert.IsNotNull(model.Board);
        Assert.AreEqual(3, model.SurvivorLocations.Count);
    }
    [Test]
    public void LoadGameTest()
    {
        GameModel model = new GameModel(_mapLoader);
        Assert.IsNull(model.Board);
        model.LoadGame(0);
        Assert.AreEqual(3, model.Board.Buildings.Count);
        Assert.AreEqual(25, model.Board.Tiles.Count);
        Assert.AreEqual(5, model.Board.Streets.Count);
    }
    [Test]
    public void GiveSurvivorsGenericWeaponTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.IsNull(SurvivorFactory.GetSurvivorByName("Amy").RightHand);
        Assert.IsNull(SurvivorFactory.GetSurvivorByName("Wanda").RightHand);
        Assert.IsNull(SurvivorFactory.GetSurvivorByName("Ned").RightHand);
        model.GiveSurvivorsGenericWeapon(new List<ItemName>() { ItemName.AXE, ItemName.BASEBALLBAT, ItemName.PISTOL });
        Assert.IsTrue(SurvivorFactory.GetSurvivorByName("Amy").RightHand.Name == ItemName.AXE);
        Assert.IsTrue(SurvivorFactory.GetSurvivorByName("Wanda").RightHand.Name == ItemName.BASEBALLBAT);
        Assert.IsTrue(SurvivorFactory.GetSurvivorByName("Ned").RightHand.Name == ItemName.PISTOL);
    }
    [Test]
    public void SetPlayerOrderTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.AreEqual(0, model.PlayerOrder.Count);
        model.SetPlayerOrder(new List<string>() { "Amy", "Ned", "Wanda" });
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Amy"), model.CurrentPlayer);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Amy"), model.PlayerOrder[0]);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Ned"), model.PlayerOrder[1]);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Wanda"), model.PlayerOrder[2]);
    }
    [Test]
    public void BuildingOpenedTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        model.GiveSurvivorsGenericWeapon(new List<ItemName>() { ItemName.AXE, ItemName.BASEBALLBAT, ItemName.PISTOL });
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        amy.MoveTo(model.Board.GetTileByID(17));
        var b = model.Board.GetBuildingByTile(4);
        var tileC=model.Board.GetTileByID(17).Neighbours.First(x=>x.Destination.Id==4);
        Assert.IsFalse(b.IsOpened);
        amy.CurrentTile.OpenDoor(tileC, (Weapon)amy.RightHand);
        Assert.IsTrue(model.BuildingOpened(tileC));
        Assert.IsTrue(b.IsOpened);
    }
    [Test]
    public void NextPlayerTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        model.SetPlayerOrder(new List<string>() { "Amy", "Ned", "Wanda" });
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Amy"), model.CurrentPlayer);
        model.NextPlayer();
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Ned"), model.CurrentPlayer);
        model.NextPlayer();
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Wanda"), model.CurrentPlayer);
    }
    [Test]
    public void ShiftPlayerOrderTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        model.SetPlayerOrder(new List<string>() { "Amy", "Ned", "Wanda" });
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Amy"), model.CurrentPlayer);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Amy"), model.PlayerOrder[0]);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Ned"), model.PlayerOrder[1]);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Wanda"), model.PlayerOrder[2]);
        model.ShiftPlayerOrder();
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Ned"), model.CurrentPlayer);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Ned"), model.PlayerOrder[0]);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Wanda"), model.PlayerOrder[1]);
        Assert.AreEqual(SurvivorFactory.GetSurvivorByName("Amy"), model.PlayerOrder[2]);
    }
    [Test]
    public void CheckWinTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.IsFalse(model.GameOver);
        model.GameEnded += Model_GameEnded;
        SurvivorFactory.GetSurvivorByName("Amy").IsDead = true;
        SurvivorFactory.GetSurvivorByName("Wanda").IsDead = true;
        SurvivorFactory.GetSurvivorByName("Ned").IsDead = true;
        model.CheckWin();
    }
    private void Model_GameEnded(object sender, bool e)
    {
        Assert.IsFalse(e);
    }

    [Test]
    public void SortZombiesByNewPriorityTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        model.SpawnZombie(ZombieType.WALKER, 2, model.Board.GetTileByID(17), false);
        model.SpawnZombie(ZombieType.FATTY, 1, model.Board.GetTileByID(17), false);
        List<Zombie> tile17 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17));
        Assert.IsTrue((FattyZombie)tile17[0] is FattyZombie);
        Assert.IsTrue((WalkerZombie)tile17[1] is WalkerZombie);
        Assert.IsTrue((WalkerZombie)tile17[2] is WalkerZombie);
        List<Zombie> newPriority = model.SortZombiesByNewPriority(tile17, new List<string>() { "WalkerZombie", "FattyZombie" });
        Assert.IsTrue((WalkerZombie)newPriority[0] is WalkerZombie);
        Assert.IsTrue((WalkerZombie)newPriority[1] is WalkerZombie);
        Assert.IsTrue((FattyZombie)newPriority[2] is FattyZombie);
    }
    [Test]
    public void RemoveZombieTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        model.SpawnZombie(ZombieType.WALKER, 1, model.Board.GetTileByID(17), false);
        model.SpawnZombie(ZombieType.FATTY, 1, model.Board.GetTileByID(13), false);
        List<Zombie> tile17 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17));
        List<Zombie> tile13 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(13));
        Assert.AreEqual(1, tile17.Count);
        Assert.AreEqual(1, tile13.Count);
        model.RemoveZombie(tile17[0]);
        model.RemoveZombie(tile13[0]);
        tile17 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17));
        tile13 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(13));
        Assert.AreEqual(0, tile17.Count);
        Assert.AreEqual(0, tile13.Count);
    }
    [Test]
    public void FindNextStepToNoisiestTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        model.Board.GetTileByID(15).NoiseCounter += 2;
        MapTile step = model.FindNextStepToNoisiest(model.Board.GetTileByID(20));
        Assert.AreEqual(17, step.Id);
        model.Board.GetTileByID(23).NoiseCounter += 3;
        step = model.FindNextStepToNoisiest(model.Board.GetTileByID(17));
        Assert.AreEqual(20, step.Id);
    }
    [Test]
    public void SpawnZombiesTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        List<(ZombieType, int, int, int, int, bool)> spawns = new List<(ZombieType, int, int, int, int, bool)>
        {
            new(ZombieType.WALKER, 1, 2, 3, 4, false),
            new(ZombieType.WALKER, 2, 2, 3, 4, false),
            new(ZombieType.FATTY, 1, 2, 3, 4, false)
        };
        Assert.AreEqual(0, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(16)).Count);
        Assert.AreEqual(0, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(12)).Count);
        Assert.AreEqual(0, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20)).Count);
        model.SpawnZombies(spawns);
        Assert.AreEqual(1, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(16)).Count);
        Assert.AreEqual(2, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(12)).Count);
        Assert.AreEqual(1, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20)).Count);
    }
    [Test]
    public void SpawnZombiesOnTileTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.AreEqual(0, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17)).Count);
        model.SpawnZombiesOnTile(new(ZombieType.WALKER, 2, 2, 3, 4, false), model.Board.GetTileByID(17));
        List<Zombie> tile17 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17));
        Assert.AreEqual(2, tile17.Count);
        Assert.IsTrue(tile17[0] is WalkerZombie);
        Assert.IsTrue(tile17[1] is WalkerZombie);
    }
    [Test]
    public void SpawnZombieTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Assert.AreEqual(0, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17)).Count);
        model.SpawnZombie(ZombieType.WALKER, 2, model.Board.GetTileByID(17), false);
        Assert.AreEqual(2, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17)).Count);
        model.Board.GetTileByID(15).NoiseCounter++;
        Assert.AreEqual(0, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20)).Count);
        model.SpawnZombie(ZombieType.WALKER, 1, model.Board.GetTileByID(20), true);
        Assert.AreEqual(0, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20)).Count);
        Assert.AreEqual(3, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(17)).Count);
    }
}
