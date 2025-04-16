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

public class SurvivorTest
{
    private IMapLoader _mapLoader = new DummyMapLoader();

    [Test]
    public void CanUpgradeToTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Assert.AreEqual(0, amy.APoints);
        Assert.IsTrue(amy.CanUpgradeTo() == 0);
        amy.PickUpObjective();
        amy.PickUpObjective();
        Assert.AreEqual(10, amy.APoints);
        Assert.IsTrue(amy.CanUpgradeTo() == 1);
    }
    [Test]
    public void CanOpenDoorOnTileTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        amy.MoveTo(model.Board.GetTileByID(17));
        amy.PutIntoHand(true,ItemFactory.GetGenericWeaponByName(ItemName.AXE));
        Assert.IsTrue(amy.CanOpenDoorOnTile());
        model.SpawnZombie(ZombieType.WALKER, 1, model.Board.GetTileByID(17), false);
        Assert.IsFalse(amy.CanOpenDoorOnTile());
    }
    [Test]
    public void HasPlentyOfShellsTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        amy.PutIntoBackpack(new List<Item>() { ItemFactory.GetItemByName(ItemName.PLENTYOFSHELLS) });
        ned.PutIntoHand(true, ItemFactory.GetItemByName(ItemName.PLENTYOFSHELLS));
        Assert.IsFalse(wanda.HasPlentyOfShells());
        Assert.IsTrue(amy.HasPlentyOfShells());
        Assert.IsTrue(ned.HasPlentyOfShells());
    }
    [Test]
    public void HasPlentyOfBulletsTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        amy.PutIntoBackpack(new List<Item>() { ItemFactory.GetItemByName(ItemName.PLENTYOFBULLETS) });
        ned.PutIntoHand(true, ItemFactory.GetItemByName(ItemName.PLENTYOFBULLETS));
        Assert.IsFalse(wanda.HasPlentyOfBullets());
        Assert.IsTrue(amy.HasPlentyOfBullets());
        Assert.IsTrue(ned.HasPlentyOfBullets());
    }
    [Test]
    public void GetAvailableAttacksOnTileTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        model.SpawnZombie(ZombieType.WALKER, 1, model.Board.GetTileByID(14), false);
        model.SpawnZombie(ZombieType.WALKER, 1, model.Board.GetTileByID(17), false);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        amy.PutIntoHand(true,ItemFactory.GetItemByName(ItemName.PISTOL));
        ned.PutIntoHand(true,ItemFactory.GetItemByName(ItemName.AXE));
        Assert.IsTrue(amy.GetAvailableAttacksOnTile(amy.CurrentTile).Contains("Melee"));
        Assert.IsTrue(amy.GetAvailableAttacksOnTile(amy.CurrentTile).Contains("Range"));
        Assert.IsTrue(ned.GetAvailableAttacksOnTile(ned.CurrentTile).Contains("Melee"));
        Assert.AreEqual(0,wanda.GetAvailableAttacksOnTile(ned.CurrentTile).Count);
    }
    [Test]
    public void HasFlashLightTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        amy.PutIntoBackpack(new List<Item>() { ItemFactory.GetItemByName(ItemName.FLASHLIGHT) });
        ned.PutIntoHand(true, ItemFactory.GetItemByName(ItemName.FLASHLIGHT));
        Assert.IsFalse(wanda.HasFlashLight());
        Assert.IsTrue(amy.HasFlashLight());
        Assert.IsTrue(ned.HasFlashLight());
    }
    [Test]
    public void MoveTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Assert.AreEqual(14, amy.CurrentTile.Id);
        amy.Move(model.Board.GetTileByID(17));
        Assert.AreEqual(17, amy.CurrentTile.Id);
    }
    [Test]
    public void SlipperyMoveTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ostara" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ostara = SurvivorFactory.GetSurvivorByName("Ostara");
        Assert.AreEqual(14, amy.CurrentTile.Id);
        Assert.AreEqual(14, ostara.CurrentTile.Id);
        amy.SlipperyMove(model.Board.GetTileByID(17));
        ostara.SlipperyMove(model.Board.GetTileByID(17));
        Assert.AreEqual(14, amy.CurrentTile.Id);
        Assert.AreEqual(17, ostara.CurrentTile.Id);
    }
    [Test]
    public void SprintMoveTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        Assert.AreEqual(14, amy.CurrentTile.Id);
        Assert.AreEqual(14, wanda.CurrentTile.Id);
        amy.SprintMove(model.Board.GetTileByID(17));
        wanda.SprintMove(model.Board.GetTileByID(20));
        Assert.AreEqual(14, amy.CurrentTile.Id);
        Assert.AreEqual(20, wanda.CurrentTile.Id);
    }
    [Test]
    public void ChargeTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Lou" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor lou = SurvivorFactory.GetSurvivorByName("Lou");
        Assert.AreEqual(14, amy.CurrentTile.Id);
        Assert.AreEqual(14, lou.CurrentTile.Id);
        amy.Charge(model.Board.GetTileByID(17));
        lou.Charge(model.Board.GetTileByID(20));
        Assert.AreEqual(14, amy.CurrentTile.Id);
        Assert.AreEqual(20, lou.CurrentTile.Id);
    }
    [Test]
    public void JumpTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Bunny G" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor bunnyg = SurvivorFactory.GetSurvivorByName("Bunny G");
        Assert.AreEqual(14, amy.CurrentTile.Id);
        Assert.AreEqual(14, bunnyg.CurrentTile.Id);
        amy.Jump(model.Board.GetTileByID(17));
        bunnyg.UpgradeTo(2, 2);
        bunnyg.Jump(model.Board.GetTileByID(20));
        Assert.AreEqual(14, amy.CurrentTile.Id);
        Assert.AreEqual(20, bunnyg.CurrentTile.Id);
    }
    [Test]
    public void AttackTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        amy.PutIntoHand(true, ItemFactory.GetItemByName(ItemName.KATANA));
        ned.PutIntoHand(true, ItemFactory.GetItemByName(ItemName.SNIPERRIFLE));
        ned.MoveTo(model.Board.GetTileByID(13));
        model.SpawnZombie(ZombieType.FATTY, 2, model.Board.GetTileByID(20), false);
        model.SpawnZombie(ZombieType.FATTY, 2, model.Board.GetTileByID(14), false);
        model.SpawnZombie(ZombieType.WALKER, 2, model.Board.GetTileByID(14), false);
        Assert.AreEqual(4, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(14)).Count);
        Assert.AreEqual(2, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20)).Count);
        amy.Attack(model.Board.GetTileByID(14), (Weapon)amy.RightHand, true, new List<int>() { 4, 1 }, new List<string>() { "WalkerZombie", "FattyZombie" });
        ned.Attack(model.Board.GetTileByID(20), (Weapon)ned.RightHand, true, new List<int>() { 3, 1 }, null);
        Assert.AreEqual(3, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(14)).Count);
        Assert.AreEqual(1, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20)).Count);
        amy.Attack(model.Board.GetTileByID(14), (Weapon)amy.RightHand, true, new List<int>() { 4, 5 }, new List<string>() { "WalkerZombie", "FattyZombie" });
        ned.Attack(model.Board.GetTileByID(20), (Weapon)ned.RightHand, true, new List<int>() { 3, 3 }, null);
        Assert.AreEqual(2, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(14)).Count);
        Assert.AreEqual(0, model.GetZombiesInPriorityOrderOnTile(model.Board.GetTileByID(20)).Count);
    }
    [Test]
    public void PickUpObjective()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        amy.MoveTo(model.Board.GetTileByID(17));
        var tileC = model.Board.GetTileByID(17).Neighbours.First(x => x.Destination.Id == 4);
        model.Board.GetTileByID(17).OpenDoor(tileC, ItemFactory.GetGenericWeaponByName(ItemName.AXE));
        amy.MoveTo(model.Board.GetTileByID(4));
        Assert.AreEqual(0, amy.ObjectiveCount);
        Assert.AreEqual(0, amy.APoints);
        Assert.IsNotNull(amy.CurrentTile.Objective);
        amy.PickUpObjective();
        Assert.AreEqual(1, amy.ObjectiveCount);
        Assert.AreEqual(5, amy.APoints);
        Assert.IsNull(amy.CurrentTile.Objective);
    }
    [Test]
    public void ThrowAwayTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Assert.AreEqual(0, amy.APoints);
        Assert.AreEqual(0, ned.APoints);
        amy.ThrowAway(ItemFactory.GetItemByName(ItemName.RICE));
        ned.ThrowAway(ItemFactory.GetItemByName(ItemName.PISTOL));
        Assert.AreEqual(3, amy.APoints);
        Assert.AreEqual(0, ned.APoints);
    }
    [Test]
    public void SkipTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Assert.IsFalse(amy.FinishedRound);
        Assert.IsFalse(ned.FinishedRound);
        amy.Skip();
        Assert.IsTrue(amy.FinishedRound);
        Assert.IsFalse(ned.FinishedRound);
        ned.Skip();
        Assert.IsTrue(amy.FinishedRound);
        Assert.IsTrue(ned.FinishedRound);
    }
    [Test]
    public void PickGenericWeaponTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Assert.IsNull(amy.RightHand);
        Assert.IsNull(ned.RightHand);
        amy.PickGenericWeapon(ItemFactory.GetGenericWeaponByName(ItemName.AXE));
        ned.PickGenericWeapon(ItemFactory.GetGenericWeaponByName(ItemName.PISTOL));
        Assert.IsNotNull(amy.RightHand);
        Assert.IsTrue(amy.RightHand.Name==ItemName.AXE);
        Assert.IsNotNull(ned.RightHand);
        Assert.IsTrue(ned.RightHand.Name==ItemName.PISTOL);
    }
    [Test]
    public void NewRoundTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        wanda.SprintMove(model.Board.GetTileByID(20));
        amy.Skip();
        wanda.Skip();
        Assert.IsTrue(amy.FinishedRound);
        Assert.IsTrue(wanda.FinishedRound);
        Assert.IsTrue(wanda.SprintMovedAlready);
        amy.NewRound();
        wanda.NewRound();
        Assert.IsFalse(amy.FinishedRound);
        Assert.IsFalse(wanda.FinishedRound);
        Assert.IsFalse(wanda.SprintMovedAlready);
    }
    [Test]
    public void TakeDamageTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        amy.SurvivorDied += Amy_SurvivorDied;
        Assert.AreEqual(3, amy.HP);
        Assert.IsFalse(amy.IsDead);
        amy.TakeDamage(3);
        Assert.AreEqual(0, amy.HP);
        Assert.IsTrue(amy.IsDead);
    }
    private void Amy_SurvivorDied(object sender, string e)
    {
        Assert.IsTrue(e == "Amy");
    }

    [Test]
    public void ResetTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        amy.PickGenericWeapon(ItemFactory.GetGenericWeaponByName(ItemName.AXE));
        amy.PickUpObjective();
        amy.TakeDamage(1);
        amy.PutIntoBackpack(new List<Item>() { ItemFactory.GetItemByName(ItemName.AXE), ItemFactory.GetItemByName(ItemName.CANNEDFOOD) });
        Assert.AreEqual(2, amy.HP);
        Assert.IsNotNull(amy.RightHand);
        Assert.AreEqual(5,amy.APoints);
        Assert.AreEqual(2,amy.BackPack.Count);
        amy.Reset();
        Assert.AreEqual(3, amy.HP);
        Assert.IsNull(amy.RightHand);
        Assert.AreEqual(0, amy.APoints);
        Assert.AreEqual(0, amy.BackPack.Count);
    }
    [Test]
    public void PutIntoBackpackTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Assert.AreEqual(0, amy.BackPack.Count);
        amy.PutIntoBackpack(new List<Item>() { ItemFactory.GetItemByName(ItemName.AXE), ItemFactory.GetItemByName(ItemName.CANNEDFOOD), ItemFactory.GetItemByName(ItemName.PISTOL) });
        Assert.AreEqual(3, amy.BackPack.Count);
        amy.PutIntoBackpack(new List<Item>() { ItemFactory.GetItemByName(ItemName.AXE), ItemFactory.GetItemByName(ItemName.CANNEDFOOD), 
            ItemFactory.GetItemByName(ItemName.PISTOL),ItemFactory.GetItemByName(ItemName.PISTOL) });
        Assert.AreEqual(3, amy.BackPack.Count);
    }
    [Test]
    public void PutIntoHandTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Assert.IsNull(amy.RightHand);
        Assert.IsNull(amy.LeftHand);
        amy.PutIntoHand(true, ItemFactory.GetItemByName(ItemName.AXE));
        Assert.IsNotNull(amy.RightHand);
        Assert.IsTrue(amy.RightHand.Name == ItemName.AXE);
        Assert.IsNull(amy.LeftHand);
        amy.PutIntoHand(false, ItemFactory.GetItemByName(ItemName.PISTOL));
        Assert.IsNotNull(amy.RightHand);
        Assert.IsNotNull(amy.LeftHand);
        Assert.IsTrue(amy.LeftHand.Name == ItemName.PISTOL);
    }
    [Test]
    public void OnUsedActionTest()
    {
        GameModel model = new GameModel(_mapLoader);
        model.StartGame(new List<string>() { "Amy", "Wanda", "Ned" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        amy.SetFreeActions();
        amy.SetActions(model.Board.GetTileByID(17));
        Assert.AreEqual(0, amy.UsedAction);
        Assert.AreEqual(1, amy.FreeActions.Count);
        amy.OnUsedAction("Move", null);
        Assert.AreEqual(0, amy.UsedAction);
        Assert.AreEqual(0, amy.FreeActions.Count);
        amy.OnUsedAction("Rearrange Items", null);
        Assert.AreEqual(1, amy.UsedAction);
        Assert.AreEqual(0, amy.FreeActions.Count);
    }
}
