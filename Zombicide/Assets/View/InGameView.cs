using Model.Characters.Survivors;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Model.Characters.Zombies;
using DG.Tweening;
using Assets.Controller;
using Network;
using Unity.VisualScripting;

namespace View
{
    public class InGameView : MonoBehaviour
    {
        #nullable enable
        #region Fields
        private GameObject? MapPrefab;
        [SerializeField] private GameObject? cameraDrag;
        [SerializeField] private GameObject? rightHand;
        [SerializeField] private GameObject? leftHand;
        [SerializeField] private List<GameObject> backPack = new List<GameObject>();
        private GameObject? aPointsText;
        private GameObject? healthText;
        private GameObject? usedActionsText;
        private GameObject? freeActionsLayoutGroup;
        private Dictionary<string, GameObject> playerPrefabs = new Dictionary<string, GameObject>();
        private GameObject? charImagePrefab;
        private HorizontalLayoutGroup? charListContainer;
        private GameObject? inventoryForS;
        private GameObject? sniperMenuForS;
        private Dictionary<int, GameObject> zombieCanvases = new Dictionary<int, GameObject>();
        #endregion
        #region Properties
        public static InGameView? Instance { get; private set; }
        public bool IsMenuOpen { get; set; } = false;
        #endregion
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        /// <summary>
        /// Loads the map prefab corresponding to the given ID.
        /// </summary>
        /// <param name="mapID">ID of the map that needs to be loaded</param>
        public void GenerateBoard(int mapID)
        {
            MapPrefab = Resources.Load<GameObject>($"Prefabs/Missions/Map_{mapID}");
            GameObject.Instantiate(MapPrefab);
        }
        /// <summary>
        /// Updates the player's inventory.
        /// </summary>
        public void UpdateItemSlots(Survivor survivor)
        {
            if (survivor!.LeftHand == null)
                leftHand!.GetComponent<Image>().sprite = Resources.Load<Sprite>("Objects/card_lefthand");
            else
                leftHand!.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.LeftHand.Name.ToString().ToLower()}");
            if (survivor.RightHand == null)
                rightHand!.GetComponent<Image>().sprite = Resources.Load<Sprite>("Objects/card_righthand");
            else
                rightHand!.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.RightHand.Name.ToString().ToLower()}");
            for (int i = 0; i < 3; i++)
            {
                if (survivor.BackPack.Count > i)
                {
                    if (survivor.BackPack[i] == null)
                        backPack[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Objects/card_backpack");
                    else
                        backPack[i].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.BackPack[i].Name.ToString().ToLower()}");
                }
            }
        }
        /// <summary>
        /// Updates the player's data on the panel.
        /// </summary>
        public void UpdatePlayerStats(Survivor survivor)
        {
            healthText!.GetComponent<TMP_Text>().text = survivor!.HP.ToString();
            aPointsText!.GetComponent<TMP_Text>().text = survivor.APoints.ToString();
            usedActionsText!.GetComponent<TMP_Text>().text = survivor.UsedAction.ToString();
            foreach (Transform child in freeActionsLayoutGroup!.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (var item in survivor.FreeActions)
            {
                var prefab = Resources.Load<GameObject>($"Prefabs/Players/FreeActionLabelPrefab");
                var newAction = Instantiate(prefab, freeActionsLayoutGroup.transform);
                newAction.GetComponent<TMP_Text>().text = item.Key;
            }
        }
        /// <summary>
        /// Displays the panel associated with the selected character.
        /// </summary>
        public void ShowPlayerUI(Survivor survivor, string name)
        {
            GameObject ui = GameObject.FindWithTag("GameUI");
            GameObject panelPrefab = Resources.Load<GameObject>($"Prefabs/Players/PlayerPanel_{name.Replace(" ", string.Empty)}");
            foreach (Transform child in panelPrefab.transform)
            {
                var component = child.GetComponent<HoverClickHandlerForPanel>();
                if (component != null)
                {
                    CameraZoom.PanelHoverScript = component;
                    CameraDrag.PanelHoverScript = component;
                }
            }
            GameObject player = Instantiate(panelPrefab, ui.transform);
            foreach (Transform child in player.transform)
            {
                if (child.name == "Data")
                {
                    foreach (Transform subChild in child.transform)
                    {
                        if (subChild.name == "Health")
                            healthText = subChild.gameObject;
                        else if (subChild.name == "Points")
                            aPointsText = subChild.gameObject;
                    }
                }
                if (child.name == "UsedActions")
                    usedActionsText = child.gameObject;
                if (child.name == "FreeActions")
                    freeActionsLayoutGroup = child.gameObject;
            }
            UpdatePlayerStats(survivor!);
        }
        /// <summary>
        /// Displays the player order for the current round.
        /// </summary>
        public void ShowPlayerOrder(bool zombieEnabled, Survivor currentP, List<Survivor> survivors)
        {
            if (charImagePrefab == null || charListContainer == null)
            {
                charImagePrefab = Resources.Load<GameObject>($"Prefabs/CharImagePrefab");
                charListContainer = GameObject.FindFirstObjectByType<HorizontalLayoutGroup>();
            }
            foreach (Transform child in charListContainer.transform)
            {
                Destroy(child.gameObject);
            }
            GameObject zombieEntry = Instantiate(charImagePrefab, charListContainer.transform);
            zombieEntry.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Characters/zombie_head");
            Outline zoutline = zombieEntry.GetComponent<Outline>();
            zoutline.enabled = zombieEnabled;

            List<Survivor> reversed = survivors;
            reversed.Reverse();
            foreach (var item in reversed)
            {
                GameObject playerEntry = Instantiate(charImagePrefab, charListContainer.transform);
                playerEntry.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Characters/{item.Name.ToLower().Replace(" ", string.Empty) + "_head"}");
                Outline outline = playerEntry.GetComponent<Outline>();
                outline.enabled = currentP == item;
            }

        }
        /// <summary>
        /// Updates the number and type of zombies on the given tile.
        /// </summary>
        /// <param name="tileID">ID of the tile the zombies should be updated on</param>
        public void UpdateZombieCanvasOnTile(int tileID, List<Zombie> zombies)
        {
            if (zombieCanvases.ContainsKey(tileID))
            {
                Destroy(zombieCanvases[tileID]);
                zombieCanvases.Remove(tileID);
            }
            if (zombies != null && zombies.Count > 0)
            {
                GameObject zombieCanvasPrefab = Resources.Load<GameObject>("Prefabs/ZombieCanvas");
                Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileID}");
                BoxCollider collider = tile.GetComponent<BoxCollider>();
                float startX = collider.transform.position.x - 0.8f;
                float startZ = collider.transform.position.z - 1f;
                float startY = 1.8f;
                Vector3 newPosition = new Vector3();
                newPosition.x = startX; newPosition.y = startY; newPosition.z = startZ;
                GameObject zc = Instantiate(zombieCanvasPrefab);
                zc.transform.position = newPosition;
                Transform panel = zc.transform.GetChild(0);
                GameObject zombiePrefab = Resources.Load<GameObject>("Prefabs/Zombie");
                foreach (string zombie in zombies.Select(x => x.GetType().Name).Distinct())
                {
                    int amount = zombies.Count(x => x.GetType().Name == zombie);
                    GameObject newZombie = Instantiate(zombiePrefab, panel.transform);
                    newZombie.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Characters/Zombies/{zombie}");
                    newZombie.transform.GetChild(0).GetComponent<TMP_Text>().text = amount.ToString();
                }
                zombieCanvases.Add(tileID, zc);
            }
        }
        /// <summary>
        /// Generates the players' figurines on the starting tile.
        /// </summary>
        public void GeneratePlayersOnBoard(int tileId, List<string> names)
        {
            Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileId}");
            BoxCollider collider = tile.GetComponent<BoxCollider>();
            float startX = collider.transform.position.x - 2f;
            float startZ = collider.transform.position.z + 0.5f;
            float startY = 2f;
            Vector3 newPosition = new Vector3();
            newPosition.x = startX; newPosition.y = startY; newPosition.z = startZ;
            int multiply = 1;

            foreach (var item in names)
            {
                GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Players/{item.Replace(" ", string.Empty)}");
                GameObject player = Instantiate(playerPrefab);
                playerPrefabs.Add(item.Replace(" ", string.Empty), player);
                player.transform.position = newPosition;
                if (multiply < 3)
                {
                    newPosition.x = startX + (multiply * 1.5f);
                    newPosition.z = startZ;
                }
                else if (multiply < 5)
                {
                    newPosition.x = startX + ((multiply - 3) * 1.5f);
                    newPosition.z = startZ - 0.7f;
                }
                else
                {
                    newPosition.x = startX + ((multiply - 5) * 1.5f);
                    newPosition.z = startZ - (2 * 0.7f);
                }
                multiply++;
            }
        }
        /// <summary>
        /// Moves the given player's figurine to the specified tile.
        /// </summary>
        /// <param name="tileID">ID of the tile the object should be put to</param>
        /// <param name="player">Object that needs to be moved</param>
        public void MovePlayerToTile(int tileID, string sName, int playerCount, List<string> list)
        {
            GameObject player = playerPrefabs[sName];
            RearrangePlayersOnTile(tileID, player.gameObject.name.Replace("(Clone)", ""), list);
            Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileID}");
            BoxCollider collider = tile.GetComponent<BoxCollider>();
            float startX = collider.transform.position.x - 2f;
            float startZ = collider.transform.position.z + 0.5f;
            float startY = 2f;
            Vector3 newPosition = player.transform.position;

            if (playerCount < 3)
            {
                newPosition.x = startX + (playerCount * 1.5f);
                newPosition.z = startZ;
            }
            else if (playerCount < 5)
            {
                newPosition.x = startX + ((playerCount - 3) * 1.5f);
                newPosition.z = startZ - 0.7f;
            }
            else
            {
                newPosition.x = startX + ((playerCount - 5) * 1.5f);
                newPosition.z = startZ - (2 * 0.7f);
            }
            newPosition.y = startY;
            player.transform.DOMove(newPosition, 0.5f).SetEase(Ease.OutQuad);
        }
        /// <summary>
        /// Rearranges the players that are already on the tile
        /// </summary>
        /// <param name="tileID">ID of the tile</param>
        /// <param name="steppingPlayer">Player who wants to move</param>
        public void RearrangePlayersOnTile(int tileID, string steppingPlayer, List<string> list)
        {
            list.Remove(steppingPlayer);
            if (list.Count == 0) return;
            Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileID}");
            BoxCollider collider = tile.GetComponent<BoxCollider>();
            float startX = collider.transform.position.x - 2f;
            float startZ = collider.transform.position.z + 0.5f;
            float startY = 2f;
            Vector3 newPosition = new Vector3();
            newPosition.x = startX; newPosition.y = startY; newPosition.z = startZ;
            int multiply = 1;
            foreach (string s in list)
            {
                GameObject player = playerPrefabs[s.Replace(" ", string.Empty)];
                player.transform.DOMove(newPosition, 0.5f).SetEase(Ease.OutQuad);
                if (multiply < 3)
                {
                    newPosition.x = startX + (multiply * 1.5f);
                    newPosition.z = startZ;
                }
                else if (multiply < 5)
                {
                    newPosition.x = startX + ((multiply - 3) * 1.5f);
                    newPosition.z = startZ - 0.7f;
                }
                else
                {
                    newPosition.x = startX + ((multiply - 5) * 1.5f);
                    newPosition.z = startZ - (2 * 0.7f);
                }
                multiply++;
            }
        }
        /// <summary>
        /// Deletes the figurine of the given player.
        /// </summary>
        /// <param name="playerName">Name of the player who should be removed</param>
        public void RemovePlayer(string playerName)
        {
            if (!playerPrefabs.ContainsKey(playerName.Replace(" ", string.Empty)))
                return;
            GameObject player = playerPrefabs[playerName.Replace(" ", string.Empty)];
            playerPrefabs.Remove(playerName.Replace(" ", string.Empty));
            Destroy(player);
        }
        public void ShowPopupSearch(string sName)
        {
            AnimationController.Instance.ShowPopupSearch(playerPrefabs[sName].transform.position);
        }
        /// <summary>
        /// Displays the priority selection window.
        /// </summary>
        /// <param name="data">Contains the list of zombies</param>
        public void OpenPriorityMenu(string data, List<Zombie> zombies)
        {
            IsMenuOpen = true;
            cameraDrag!.SetActive(false);
            GameObject gameUI = GameObject.FindGameObjectWithTag("GameUI");
            GameObject sniperPrefab = Resources.Load<GameObject>($"Prefabs/SniperMenu");
            GameObject sniper = Instantiate(sniperPrefab, gameUI.transform);
            EnableBoardInteraction(false);
            List<string> zombieTypeList = zombies.Select(x => x.GetType().Name).Distinct().ToList();
            foreach (Transform child in sniper.transform)
            {
                if (child.name.StartsWith("Zombie"))
                {
                    int i = 0;
                    foreach (Transform subChild in child.transform)
                    {
                        if (i >= zombieTypeList.Count)
                            break;
                        if (zombieTypeList[i] != null)
                        {
                            GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                            GameObject item = Instantiate(itemPrefab, subChild.transform);
                            if (zombieTypeList[i].StartsWith("Hobo") || zombieTypeList[i].StartsWith("Abo") || zombieTypeList[i].StartsWith("Pat"))
                                item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Menu/AbominationSlot");
                            else
                                item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Menu/{zombieTypeList[i]}Slot");
                        }
                        i++;
                    }
                }
                else if (child.name.StartsWith("Ok"))
                {
                    child.GetComponent<Button>().onClick.AddListener(() => {
                        GameController.Instance!.OnOkPriorityMenuClicked(data,sniperMenuForS!);
                    });
                }
            }
            sniperMenuForS = sniper;
        }
        /// <summary>
        /// Displays the inventory window.
        /// </summary>
        /// <param name="additionalItems">New items the player got</param>
        /// <param name="isSearch">Was the method called from a search</param>
        public void OpenInventory(List<Item>? additionalItems, bool isSearch, Survivor survivor)
        {
            IsMenuOpen = true;
            cameraDrag!.SetActive(false);
            GameObject gameUI = GameObject.FindGameObjectWithTag("GameUI");
            GameObject inventoryPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Inventory");
            GameObject inventory = Instantiate(inventoryPrefab, gameUI.transform);
            EnableBoardInteraction(false);
            foreach (Transform child in inventory.transform)
            {
                if (child.name.StartsWith("Hand"))
                {
                    foreach (Transform subChild in child.transform)
                    {
                        if (subChild.name.Substring(14) == "Left" && survivor!.LeftHand != null)
                        {
                            GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                            GameObject item = Instantiate(itemPrefab, subChild.transform);
                            item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.LeftHand.Name.ToString().ToLower()}");
                        }
                        else if (subChild.name.Substring(14) == "Right" && survivor!.RightHand != null)
                        {
                            GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                            GameObject item = Instantiate(itemPrefab, subChild.transform);
                            item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.RightHand.Name.ToString().ToLower()}");
                        }
                    }
                }
                else if (child.name.StartsWith("Back"))
                {
                    int i = 0;
                    foreach (Transform subChild in child.transform)
                    {
                        if (i >= survivor!.BackPack.Count)
                            break;
                        if (survivor.BackPack[i] != null)
                        {
                            GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                            GameObject item = Instantiate(itemPrefab, subChild.transform);
                            item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.BackPack[i].Name.ToString().ToLower()}");
                        }
                        i++;
                    }
                }
                else if (child.name.StartsWith("Throw") && additionalItems != null && additionalItems.Count > 0)
                {
                    int i = 0;
                    foreach (Transform subChild in child.transform)
                    {
                        if (i >= additionalItems.Count)
                            break;
                        if (additionalItems[i] != null)
                        {
                            GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                            GameObject item = Instantiate(itemPrefab, subChild.transform);
                            item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{additionalItems[i].Name.ToString().ToLower()}");
                        }
                        i++;
                    }
                }
                else if (child.name.StartsWith("Ok"))
                {
                    child.GetComponent<Button>().onClick.AddListener(() => {
                        GameController.Instance!.OnOkInventoryClicked(isSearch,inventoryForS!);
                    });
                }
            }
            inventoryForS = inventory;
        }
        /// <summary>
        /// Locks or unlocks map interaction based on the parameter.
        /// </summary>
        /// <param name="enable">If true then enable otherwise disable</param>
        public void EnableBoardInteraction(bool enable)
        {
            if (enable && IsMenuOpen) return;
            foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
            {
                if (child.name.StartsWith("SubTile_"))
                {
                    var collider = child.GetComponent<BoxCollider>();
                    if (collider != null)
                        collider.enabled = enable;
                }
            }
        }
        /// <summary>
        /// Enables or disables clicking on doors on the map based on the parameter.
        /// </summary>
        /// <param name="enable">If true then enable otherwise disable</param>
        public void EnableDoors(bool enable,Survivor survivor)
        {
            IsMenuOpen = true;
            foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
            {
                if (child.name == "Doors")
                {
                    foreach (Transform subChild in child.transform)
                    {
                        if (int.Parse(subChild.name.Substring(5).Split('_')[0]) == survivor!.CurrentTile.Id || int.Parse(subChild.name.Substring(5).Split('_')[1]) == survivor.CurrentTile.Id)
                        {
                            var collider = subChild.GetChild(0).GetComponent<BoxCollider>();
                            if (collider != null)
                                collider.enabled = enable;
                        }
                    }
                }
            }
        }
        public void SetCameraDrag(bool enable)
        {
            cameraDrag!.SetActive(enable);
        }
        /// <summary>
        /// Animates the door gameobject before destroying it.
        /// </summary>
        /// <param name="door">Door that was clicked</param>
        public void DestroyDoorWithTween(GameObject door)
        {
            door.transform.DORotate(new Vector3(0, 90, 0), 1f, RotateMode.WorldAxisAdd)
                .OnComplete(() => Destroy(door));
        }
        /// <summary>
        /// Destroyes the given object with animation
        /// </summary>
        /// <param name="obj">Gameobject that will be destroyed</param>
        public void DestroyWithTween(GameObject obj)
        {
            obj.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() => Destroy(obj));
        }
    }
}
