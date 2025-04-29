using Assets.Persistence;
using Model;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using NUnit.Framework;
using Persistence;
using System;
using System.Collections;
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
    [Test]
    public void SetFreeActionsTest()
    {
        GameModel model1 = new GameModel(_mapLoader);
        model1.StartGame(new List<string>() { "Amy", "Doug", "Elle","Josh","Ned","Wanda" }, 0);
        GameModel model2 = new GameModel(_mapLoader);
        model2.StartGame(new List<string>() { "Bunny G","Lili","Lou","Odin","Ostara" ,"Tiger Sam"}, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Assert.AreEqual(0, amy.FreeActions.Count);
        amy.SetFreeActions();
        Assert.AreEqual(1, amy.FreeActions.Count);
        Survivor doug = SurvivorFactory.GetSurvivorByName("Doug");
        Assert.AreEqual(0, doug.FreeActions.Count);
        doug.SetFreeActions();
        Assert.AreEqual(0, doug.FreeActions.Count);
        Survivor elle = SurvivorFactory.GetSurvivorByName("Elle");
        Assert.AreEqual(0, elle.FreeActions.Count);
        elle.SetFreeActions();
        Assert.AreEqual(0, elle.FreeActions.Count);
        Survivor josh = SurvivorFactory.GetSurvivorByName("Josh");
        Assert.AreEqual(0, josh.FreeActions.Count);
        josh.SetFreeActions();
        Assert.AreEqual(0, josh.FreeActions.Count);
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Assert.AreEqual(0, ned.FreeActions.Count);
        ned.SetFreeActions();
        Assert.AreEqual(1, ned.FreeActions.Count);
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        Assert.AreEqual(0, wanda.FreeActions.Count);
        wanda.SetFreeActions();
        Assert.AreEqual(0, wanda.FreeActions.Count);
        Survivor bunnyG = SurvivorFactory.GetSurvivorByName("Bunny G");
        Assert.AreEqual(0, bunnyG.FreeActions.Count);
        bunnyG.SetFreeActions();
        Assert.AreEqual(0, bunnyG.FreeActions.Count);
        Survivor lili = SurvivorFactory.GetSurvivorByName("Lili");
        Assert.AreEqual(0, lili.FreeActions.Count);
        lili.SetFreeActions();
        Assert.AreEqual(0, lili.FreeActions.Count);
        Survivor lou = SurvivorFactory.GetSurvivorByName("Lou");
        Assert.AreEqual(0, lou.FreeActions.Count);
        lou.SetFreeActions();
        Assert.AreEqual(1, lou.FreeActions.Count);
        Survivor odin = SurvivorFactory.GetSurvivorByName("Odin");
        Assert.AreEqual(0, odin.FreeActions.Count);
        odin.SetFreeActions();
        Assert.AreEqual(0, odin.FreeActions.Count);
        Survivor ostara = SurvivorFactory.GetSurvivorByName("Ostara");
        Assert.AreEqual(0, ostara.FreeActions.Count);
        ostara.SetFreeActions();
        Assert.AreEqual(0, ostara.FreeActions.Count);
        Survivor tigerSam = SurvivorFactory.GetSurvivorByName("Tiger Sam");
        Assert.AreEqual(0, tigerSam.FreeActions.Count);
        tigerSam.SetFreeActions();
        Assert.AreEqual(0, tigerSam.FreeActions.Count);
    }
    [Test]
    public void SetActionsTest()
    {
        GameModel model1 = new GameModel(_mapLoader);
        model1.StartGame(new List<string>() { "Amy", "Doug", "Elle", "Josh", "Ned", "Wanda" }, 0);
        GameModel model2 = new GameModel(_mapLoader);
        model2.StartGame(new List<string>() { "Bunny G", "Lili", "Lou", "Odin", "Ostara", "Tiger Sam" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Assert.AreEqual(0, amy.Actions.Count);
        amy.SetActions(model1.Board.GetTileByID(17));
        Assert.AreEqual(2, amy.Actions.Count);
        Survivor doug = SurvivorFactory.GetSurvivorByName("Doug");
        Assert.AreEqual(0, doug.Actions.Count);
        doug.SetActions(model1.Board.GetTileByID(17));
        Assert.AreEqual(2, doug.Actions.Count);
        Survivor elle = SurvivorFactory.GetSurvivorByName("Elle");
        Assert.AreEqual(0, elle.Actions.Count);
        elle.SetActions(model1.Board.GetTileByID(17));
        Assert.AreEqual(2, elle.Actions.Count);
        Survivor josh = SurvivorFactory.GetSurvivorByName("Josh");
        Assert.AreEqual(0, josh.Actions.Count);
        josh.SetActions(model1.Board.GetTileByID(17));
        Assert.AreEqual(2, josh.Actions.Count);
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Assert.AreEqual(0, ned.Actions.Count);
        ned.SetActions(model1.Board.GetTileByID(17));
        Assert.AreEqual(2, ned.Actions.Count);
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        Assert.AreEqual(0, wanda.Actions.Count);
        wanda.SetActions(model1.Board.GetTileByID(17));
        Assert.AreEqual(3, wanda.Actions.Count);
        Survivor bunnyG = SurvivorFactory.GetSurvivorByName("Bunny G");
        Assert.AreEqual(0, bunnyG.Actions.Count);
        bunnyG.SetActions(model2.Board.GetTileByID(17));
        Assert.AreEqual(2, bunnyG.Actions.Count);
        Survivor lili = SurvivorFactory.GetSurvivorByName("Lili");
        Assert.AreEqual(0, lili.Actions.Count);
        lili.SetActions(model2.Board.GetTileByID(17));
        Assert.AreEqual(2, lili.Actions.Count);
        Survivor lou = SurvivorFactory.GetSurvivorByName("Lou");
        Assert.AreEqual(0, lou.Actions.Count);
        lou.SetActions(model2.Board.GetTileByID(17));
        Assert.AreEqual(2, lou.Actions.Count);
        Survivor odin = SurvivorFactory.GetSurvivorByName("Odin");
        Assert.AreEqual(0, odin.Actions.Count);
        odin.SetActions(model2.Board.GetTileByID(17));
        Assert.AreEqual(2, odin.Actions.Count);
        Survivor ostara = SurvivorFactory.GetSurvivorByName("Ostara");
        Assert.AreEqual(0, ostara.Actions.Count);
        ostara.SetActions(model2.Board.GetTileByID(17));
        Assert.AreEqual(2, ostara.Actions.Count);
        Survivor tigerSam = SurvivorFactory.GetSurvivorByName("Tiger Sam");
        Assert.AreEqual(0, tigerSam.Actions.Count);
        tigerSam.SetActions(model2.Board.GetTileByID(17));
        Assert.AreEqual(2, tigerSam.Actions.Count);
    }
    [Test]
    public void UpgradeToTest()
    {
        GameModel model1 = new GameModel(_mapLoader);
        model1.StartGame(new List<string>() { "Amy", "Doug", "Elle", "Josh", "Ned", "Wanda" }, 0);
        GameModel model2 = new GameModel(_mapLoader);
        model2.StartGame(new List<string>() { "Bunny G", "Lili", "Lou", "Odin", "Ostara", "Tiger Sam" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        Assert.AreEqual(0, amy.Traits.Count);
        amy.UpgradeTo(2, 1);
        Assert.AreEqual(1, amy.Traits.Count);
        amy.UpgradeTo(2, 2);
        Assert.AreEqual(2, amy.Traits.Count);
        amy.UpgradeTo(3, 1);
        Assert.AreEqual(3, amy.Traits.Count);
        amy.UpgradeTo(3, 2);
        Assert.AreEqual(4, amy.Traits.Count);
        amy.UpgradeTo(3, 3);
        Assert.AreEqual(5, amy.Traits.Count);
        Survivor doug = SurvivorFactory.GetSurvivorByName("Doug");
        Assert.AreEqual(1, doug.Traits.Count);
        doug.UpgradeTo(2, 1);
        Assert.AreEqual(2, doug.Traits.Count);
        doug.UpgradeTo(2, 2);
        Assert.AreEqual(3, doug.Traits.Count);
        doug.UpgradeTo(3, 1);
        Assert.AreEqual(4, doug.Traits.Count);
        doug.UpgradeTo(3, 2);
        Assert.AreEqual(5, doug.Traits.Count);
        doug.UpgradeTo(3, 3);
        Assert.AreEqual(6, doug.Traits.Count);
        Survivor elle = SurvivorFactory.GetSurvivorByName("Elle");
        Assert.AreEqual(1, elle.Traits.Count);
        elle.UpgradeTo(2, 1);
        Assert.AreEqual(2, elle.Traits.Count);
        elle.UpgradeTo(2, 2);
        Assert.AreEqual(3, elle.Traits.Count);
        elle.UpgradeTo(3, 1);
        Assert.AreEqual(4, elle.Traits.Count);
        elle.UpgradeTo(3, 2);
        Assert.AreEqual(5, elle.Traits.Count);
        elle.UpgradeTo(3, 3);
        Assert.AreEqual(6, elle.Traits.Count);
        Survivor josh = SurvivorFactory.GetSurvivorByName("Josh");
        Assert.AreEqual(1, josh.Traits.Count);
        josh.UpgradeTo(2, 1);
        Assert.AreEqual(2, josh.Traits.Count);
        josh.UpgradeTo(2, 2);
        Assert.AreEqual(3, josh.Traits.Count);
        josh.UpgradeTo(3, 1);
        Assert.AreEqual(4, josh.Traits.Count);
        josh.UpgradeTo(3, 2);
        Assert.AreEqual(5, josh.Traits.Count);
        josh.UpgradeTo(3, 3);
        Assert.AreEqual(6, josh.Traits.Count);
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        Assert.AreEqual(0, ned.Traits.Count);
        ned.UpgradeTo(2, 1);
        Assert.AreEqual(1, ned.Traits.Count);
        ned.UpgradeTo(2, 2);
        Assert.AreEqual(2, ned.Traits.Count);
        ned.UpgradeTo(3, 1);
        Assert.AreEqual(3, ned.Traits.Count);
        ned.UpgradeTo(3, 2);
        Assert.AreEqual(4, ned.Traits.Count);
        ned.UpgradeTo(3, 3);
        Assert.AreEqual(5, ned.Traits.Count);
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        Assert.AreEqual(1, wanda.Traits.Count);
        wanda.UpgradeTo(2, 1);
        Assert.AreEqual(2, wanda.Traits.Count);
        wanda.UpgradeTo(2, 2);
        Assert.AreEqual(3, wanda.Traits.Count);
        wanda.UpgradeTo(3, 1);
        Assert.AreEqual(4, wanda.Traits.Count);
        wanda.UpgradeTo(3, 2);
        Assert.AreEqual(5, wanda.Traits.Count);
        wanda.UpgradeTo(3, 3);
        Assert.AreEqual(6, wanda.Traits.Count);
        Survivor bunnyG = SurvivorFactory.GetSurvivorByName("Bunny G");
        Assert.AreEqual(3, bunnyG.Traits.Count);
        bunnyG.UpgradeTo(2, 1);
        Assert.AreEqual(4, bunnyG.Traits.Count);
        bunnyG.UpgradeTo(2, 2);
        Assert.AreEqual(5, bunnyG.Traits.Count);
        bunnyG.UpgradeTo(3, 1);
        Assert.AreEqual(6, bunnyG.Traits.Count);
        bunnyG.UpgradeTo(3, 2);
        Assert.AreEqual(7, bunnyG.Traits.Count);
        bunnyG.UpgradeTo(3, 3);
        Assert.AreEqual(8, bunnyG.Traits.Count);
        Survivor lili = SurvivorFactory.GetSurvivorByName("Lili");
        Assert.AreEqual(1, lili.Traits.Count);
        lili.UpgradeTo(2, 1);
        Assert.AreEqual(2, lili.Traits.Count);
        lili.UpgradeTo(2, 2);
        Assert.AreEqual(3, lili.Traits.Count);
        lili.UpgradeTo(3, 1);
        Assert.AreEqual(4, lili.Traits.Count);
        lili.UpgradeTo(3, 2);
        Assert.AreEqual(5, lili.Traits.Count);
        lili.UpgradeTo(3, 3);
        Assert.AreEqual(6, lili.Traits.Count);
        Survivor lou = SurvivorFactory.GetSurvivorByName("Lou");
        Assert.AreEqual(2, lou.Traits.Count);
        lou.UpgradeTo(2, 1);
        Assert.AreEqual(3, lou.Traits.Count);
        lou.UpgradeTo(2, 2);
        Assert.AreEqual(4, lou.Traits.Count);
        lou.UpgradeTo(3, 1);
        Assert.AreEqual(5, lou.Traits.Count);
        lou.UpgradeTo(3, 2);
        Assert.AreEqual(6, lou.Traits.Count);
        lou.UpgradeTo(3, 3);
        Assert.AreEqual(7, lou.Traits.Count);
        Survivor odin = SurvivorFactory.GetSurvivorByName("Odin");
        Assert.AreEqual(2, odin.Traits.Count);
        odin.UpgradeTo(2, 1);
        Assert.AreEqual(3, odin.Traits.Count);
        odin.UpgradeTo(2, 2);
        Assert.AreEqual(4, odin.Traits.Count);
        odin.UpgradeTo(3, 1);
        Assert.AreEqual(5, odin.Traits.Count);
        odin.UpgradeTo(3, 2);
        Assert.AreEqual(6, odin.Traits.Count);
        odin.UpgradeTo(3, 3);
        Assert.AreEqual(7, odin.Traits.Count);
        Survivor ostara = SurvivorFactory.GetSurvivorByName("Ostara");
        Assert.AreEqual(1, ostara.Traits.Count);
        ostara.UpgradeTo(2, 1);
        Assert.AreEqual(2, ostara.Traits.Count);
        ostara.UpgradeTo(2, 2);
        Assert.AreEqual(3, ostara.Traits.Count);
        ostara.UpgradeTo(3, 1);
        Assert.AreEqual(4, ostara.Traits.Count);
        ostara.UpgradeTo(3, 2);
        Assert.AreEqual(5, ostara.Traits.Count);
        ostara.UpgradeTo(3, 3);
        Assert.AreEqual(6, ostara.Traits.Count);
        Survivor tigerSam = SurvivorFactory.GetSurvivorByName("Tiger Sam");
        Assert.AreEqual(2, tigerSam.Traits.Count);
        tigerSam.UpgradeTo(2, 1);
        Assert.AreEqual(3, tigerSam.Traits.Count);
        tigerSam.UpgradeTo(2, 2);
        Assert.AreEqual(4, tigerSam.Traits.Count);
        tigerSam.UpgradeTo(3, 1);
        Assert.AreEqual(5, tigerSam.Traits.Count);
        tigerSam.UpgradeTo(3, 2);
        Assert.AreEqual(6, tigerSam.Traits.Count);
        tigerSam.UpgradeTo(3, 3);
        Assert.AreEqual(7, tigerSam.Traits.Count);
    }
    [Test]
    public void GetTraitUpgradesTest()
    {
        GameModel model1 = new GameModel(_mapLoader);
        model1.StartGame(new List<string>() { "Amy", "Doug", "Elle", "Josh", "Ned", "Wanda" }, 0);
        GameModel model2 = new GameModel(_mapLoader);
        model2.StartGame(new List<string>() { "Bunny G", "Lili", "Lou", "Odin", "Ostara", "Tiger Sam" }, 0);
        Survivor amy = SurvivorFactory.GetSurvivorByName("Amy");
        List<string> list1= amy.GetTraitUpgrades(1);
        List<string> list2= amy.GetTraitUpgrades(2);
        List<string> list3= amy.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("+1 free ranged action"));
        Assert.IsTrue(list3.Contains("medic"));
        Survivor doug = SurvivorFactory.GetSurvivorByName("Doug");
        list1 = doug.GetTraitUpgrades(1);
        list2 = doug.GetTraitUpgrades(2);
        list3 = doug.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("+1 free combat action"));
        Assert.IsTrue(list3.Contains("slippery"));
        Survivor elle = SurvivorFactory.GetSurvivorByName("Elle");
        list1 = elle.GetTraitUpgrades(1);
        list2 = elle.GetTraitUpgrades(2);
        list3 = elle.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("+1 free ranged action"));
        Assert.IsTrue(list3.Contains("+1 free combat action"));
        Survivor josh = SurvivorFactory.GetSurvivorByName("Josh");
        list1 = josh.GetTraitUpgrades(1);
        list2 = josh.GetTraitUpgrades(2);
        list3 = josh.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("+1 free combat action"));
        Assert.IsTrue(list3.Contains("lucky"));
        Survivor ned = SurvivorFactory.GetSurvivorByName("Ned");
        list1 = ned.GetTraitUpgrades(1);
        list2 = ned.GetTraitUpgrades(2);
        list3 = ned.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("+1 free combat action"));
        Assert.IsTrue(list3.Contains("shove"));
        Survivor wanda = SurvivorFactory.GetSurvivorByName("Wanda");
        list1 = wanda.GetTraitUpgrades(1);
        list2 = wanda.GetTraitUpgrades(2);
        list3 = wanda.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("slippery"));
        Assert.IsTrue(list3.Contains("+1 free melee action"));
        Survivor bunnyG = SurvivorFactory.GetSurvivorByName("Bunny G");
        list1 = bunnyG.GetTraitUpgrades(1);
        list2 = bunnyG.GetTraitUpgrades(2);
        list3 = bunnyG.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("jump"));
        Assert.IsTrue(list3.Contains("+1 free combat action"));
        Survivor lili = SurvivorFactory.GetSurvivorByName("Lili");
        list1 = lili.GetTraitUpgrades(1);
        list2 = lili.GetTraitUpgrades(2);
        list3 = lili.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("sprint"));
        Assert.IsTrue(list3.Contains("+1 free move action"));
        Survivor lou = SurvivorFactory.GetSurvivorByName("Lou");
        list1 = lou.GetTraitUpgrades(1);
        list2 = lou.GetTraitUpgrades(2);
        list3 = lou.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("+1 free melee action"));
        Assert.IsTrue(list3.Contains("medic"));
        Survivor odin = SurvivorFactory.GetSurvivorByName("Odin");
        list1 = odin.GetTraitUpgrades(1);
        list2 = odin.GetTraitUpgrades(2);
        list3 = odin.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("+1 free move action"));
        Assert.IsTrue(list3.Contains("+1 free combat action"));
        Survivor ostara = SurvivorFactory.GetSurvivorByName("Ostara");
        list1 = ostara.GetTraitUpgrades(1);
        list2 = ostara.GetTraitUpgrades(2);
        list3 = ostara.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("+1 free move action"));
        Assert.IsTrue(list3.Contains("slippery"));
        Survivor tigerSam = SurvivorFactory.GetSurvivorByName("Tiger Sam");
        list1 = tigerSam.GetTraitUpgrades(1);
        list2 = tigerSam.GetTraitUpgrades(2);
        list3 = tigerSam.GetTraitUpgrades(3);
        Assert.IsTrue(list1.Contains("+1 action"));
        Assert.IsTrue(list2.Contains("sniper"));
        Assert.IsTrue(list3.Contains("shove"));
    }
}
