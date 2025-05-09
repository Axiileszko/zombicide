﻿using Model.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Board
{
    #nullable enable
    public enum TileType
    {
        DARKROOM,ROOM,STREET
    }
    public enum ZombieSpawnType
    {
        FIRST, RED, GREEN, BLUE
    }
    public class MapTile
    {
        public int Id {  get; }
        public TileType? Type { get; }
        public List<TileConnection> Neighbours { get; } = new List<TileConnection>();
        public PimpWeapon? PimpWeapon { get; private set; }
        public bool IsExit { get;}
        public bool IsStart { get;}
        public ZombieSpawnType? SpawnType {  get; }
        public Objective? Objective { get; private set; }
        public int NoiseCounter {  get; set; }
        public MapTile(int id, TileType? type, bool hasObjective, bool hasPimpW, bool isExit, bool isStart,ZombieSpawnType? spawnType)
        {
            NoiseCounter = 0;
            Id = id;
            Type = type;
            IsExit = isExit;
            IsStart = isStart;
            SpawnType=spawnType;
            if (hasObjective)
                Objective = new Objective();
            if (hasPimpW)
                PimpWeapon = ItemFactory.GetPimpWeapon();
        }
        public void OpenDoor(TileConnection tileConnection,Weapon weapon)
        {
            if(!Neighbours.Contains(tileConnection) || !weapon.CanOpenDoors || tileConnection.IsDoorOpen) return;
            if(weapon.IsLoud)
                NoiseCounter++;
            tileConnection.IsDoorOpen = true;
            tileConnection.Destination.Neighbours.First(x=>x.Destination.Id==Id).IsDoorOpen = true;
        }
        public TileConnection GetTileConnectionById(int from, int to)
        {
            if (from == Id)
            {
                return Neighbours.First(x=>x.Destination.Id == to);
            }
            else
            {
                return Neighbours.First(x => x.Destination.Id == from);
            }
        }
        public void AddNeighbour(TileConnection connection)
        {
            Neighbours.Add(connection);
        }
        public PimpWeapon PickUpPimpWeapon()
        {
            var pimpWeapon = PimpWeapon;
            PimpWeapon = null;
            return pimpWeapon!;
        }
        public Objective PickUpObjective()
        {
            var obj = Objective;
            Objective = null;
            return obj!;
        }
    }
}
