using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;
using Network;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text errorText;
    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        hostButton.onClick.AddListener(CreateRelay);
        joinButton.onClick.AddListener(() => JoinRelay(codeInput.text));
    }

    async void CreateRelay()
    {
        Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(6);
        string code = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
        codeText.text = code;

        var relayServerData = hostAllocation.ToRelayServerData("dtls"); //Datagram Transport Layer Security
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();
    }
    async void JoinRelay(string joinCode)
    {
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            errorText.gameObject.SetActive(false);
            var relayServerData = joinAllocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            // **Amint csatlakozott a kliens, kérjük le az elérhetõ karaktereket**
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                if (clientId == NetworkManager.Singleton.LocalClientId)
                {
                    NetworkManagerController.Instance.RequestCharacterListServerRpc(clientId);
                }
            };
        }
        catch (RelayServiceException)
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "The code is incorrect!";
        }
        
    }
}
