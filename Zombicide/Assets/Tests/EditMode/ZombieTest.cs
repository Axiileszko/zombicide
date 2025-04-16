using Assets.Persistence;
using Model;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using NUnit.Framework;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
public class ZombieTest
{
    private IMapLoader _mapLoader = new DummyMapLoader();

    [Test]
    public void MoveTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        model.Board.GetTileByID(15).NoiseCounter += 2;
        amy.MoveTo(model.Board.GetTileByID(15));
        ned.MoveTo(model.Board.GetTileByID(15));
        wanda.SprintMove(model.Board.GetTileByID(19));
        wanda.MoveTo(model.Board.GetTileByID(18));
        model.SpawnZombie(ZombieType.WALKER, 1, model.Board.GetTileByID(20), false);
        List<Zombie> tile20 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20));
        Assert.AreEqual(1, tile20.Count);
        model.EndRound();
        List<Zombie> tile19 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(19));
        tile20 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20));
        Assert.AreEqual(0, tile20.Count);
        Assert.AreEqual(1, tile19.Count);
    }
    [Test]
    public void AttackTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        amy.MoveTo(model.Board.GetTileByID(15));
        ned.MoveTo(model.Board.GetTileByID(15));
        model.SpawnZombie(ZombieType.WALKER, 1, model.Board.GetTileByID(14), false);
        Assert.AreEqual(3, wanda.HP);
        List<Zombie> tile14 = model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(14));
        tile14[0].Attack(model.GetSurvivorsOnTile(model.Board.GetTileByID(14)));
        Assert.AreEqual(2, wanda.HP);
    }
}
