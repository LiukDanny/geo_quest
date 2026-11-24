using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HubButton : MonoBehaviour
{
    [SerializeField] private Button _hub;

    private void Awake()
    {
        _hub.onClick.AddListener(OnHubClicked);

    }

    private void OnHubClicked()
    {
        SceneManager.LoadScene(SceneNames.Hub);
    }

    private void OnDestroy()
    {
        _hub.onClick.RemoveListener(OnHubClicked);
    }
}
