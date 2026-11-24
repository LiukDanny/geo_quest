using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CharactersController : MonoBehaviour
{
    [SerializeField] private Button _char1Btn;
    [SerializeField] private Button _char2Btn;
    [SerializeField] private MovePositionHex _char1;
    [SerializeField] private MovePositionHex _char2;//need to fix if we had more than to characters
    [SerializeField] private GameObject _char1ActiveOutline;
    [SerializeField] private GameObject _char2ActiveOutline;
    [SerializeField] private CameraToBottomLeft _camera;

    public CharacterService CharacterService { get; private set; }

    public void Init(GridService gridService, FollowConfig followConfig)
    {
        _char1.Init(gridService);
        _char2.Init(gridService);
        CharacterService = new CharacterService(_char1, _char2, followConfig, gridService);
        CharacterService.ActiveChanged += OnActiveChanged;

        _char1Btn.onClick.AddListener(() => SetActiveChar(_char1));
        _char2Btn.onClick.AddListener(() => SetActiveChar(_char2));

        SetActiveChar(_char1);
    }

    private void SetActiveChar(MovePositionHex character)
    {
        CharacterService.SetActive(character);

        if (_camera != null)
            _camera.CenterOnCharacter(character.transform);
    }

    private void OnActiveChanged(MovePositionHex character)
    {
        if (_char1ActiveOutline != null)
            _char1ActiveOutline.SetActive(character == _char1);

        if (_char2ActiveOutline != null)
            _char2ActiveOutline.SetActive(character == _char2);
    }

    public UniTask<ActionResult> MoveActiveChar(Vector3 position) => CharacterService.MoveActive(position);

    private void OnDestroy()
    {
        _char1Btn.onClick.RemoveAllListeners();
        _char2Btn.onClick.RemoveAllListeners();
    }
}
